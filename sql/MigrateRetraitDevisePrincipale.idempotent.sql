-- =============================================================================
-- Migration : retrait agent en devise principale (soldes wallet USD)
-- =============================================================================
-- Contexte : avant la correction CommissionService / RetraitAgentService, les
-- commissions étaient souvent créditées sur le wallet de la devise collecte
-- (ex. CDF, SoldeCourant seul) alors que le retrait vérifie SoldeDisponible
-- sur le wallet en devise principale (USD).
--
-- Ce script :
--   1. Aligne SoldeDisponible sur SoldeCourant pour les wallets principaux
--      lorsque SoldeDisponible < SoldeCourant.
--   2. Transfère les SoldeCourant > 0 des wallets non principaux vers le
--      wallet principal (conversion via TauxChangeDevises), avec mouvements
--      d'audit Source = 'MIG_RETRAIT_DEVISE'.
--
-- Idempotent : ignore les wallets déjà migrés (mouvement MIG_RETRAIT_DEVISE
-- existant sur le wallet source).
--
-- Prérequis : devise principale (EstDevisePrincipale = 1) et taux actifs
-- USD ↔ CDF dans TauxChangeDevises.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRetraitDevisePrincipale.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @PrincipalDeviseId := (
    SELECT IdDevise FROM Devises WHERE EstDevisePrincipale = 1 AND Statut = 1 LIMIT 1
);

-- ---------------------------------------------------------------------------
-- Phase 1 : synchroniser SoldeDisponible sur les wallets principaux
-- ---------------------------------------------------------------------------
UPDATE WalletsAgents w
INNER JOIN Devises d ON d.IdDevise = w.DeviseId AND d.EstDevisePrincipale = 1 AND d.Statut = 1
SET w.SoldeDisponible = w.SoldeCourant,
    w.DateModification = NOW()
WHERE w.Statut = 1
  AND @PrincipalDeviseId IS NOT NULL
  AND w.SoldeDisponible < w.SoldeCourant;

-- ---------------------------------------------------------------------------
-- Phase 2 : transférer les soldes non principaux vers le wallet principal
-- ---------------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS tmp_mig_retrait_devise;

CREATE TEMPORARY TABLE tmp_mig_retrait_devise AS
SELECT
    w.IdWalletAgent AS SourceWalletId,
    w.AgentId,
    w.DeviseId AS SourceDeviseId,
    d.Code AS SourceDeviseCode,
    w.SoldeCourant AS MontantSource,
  (
    SELECT t.Taux
    FROM TauxChangeDevises t
    WHERE t.Statut = 1
      AND t.DeviseSourceId = w.DeviseId
      AND t.DeviseCibleId = @PrincipalDeviseId
    ORDER BY t.DateEffet DESC
    LIMIT 1
  ) AS TauxDirect,
  (
    SELECT t.Taux
    FROM TauxChangeDevises t
    WHERE t.Statut = 1
      AND t.DeviseSourceId = @PrincipalDeviseId
      AND t.DeviseCibleId = w.DeviseId
      AND t.Taux <> 0
    ORDER BY t.DateEffet DESC
    LIMIT 1
  ) AS TauxInverse,
    CAST(NULL AS DECIMAL(18,2)) AS MontantPrincipal,
    CAST(NULL AS UNSIGNED) AS PrincipalWalletId
FROM WalletsAgents w
INNER JOIN Devises d ON d.IdDevise = w.DeviseId
WHERE w.Statut = 1
  AND @PrincipalDeviseId IS NOT NULL
  AND w.DeviseId <> @PrincipalDeviseId
  AND w.SoldeCourant > 0
  AND NOT EXISTS (
      SELECT 1
      FROM WalletMouvements m
      WHERE m.WalletId = w.IdWalletAgent
        AND m.Source = 'MIG_RETRAIT_DEVISE'
        AND m.Statut = 1
  );

UPDATE tmp_mig_retrait_devise
SET MontantPrincipal = CASE
    WHEN TauxDirect IS NOT NULL THEN ROUND(MontantSource * TauxDirect, 2)
    WHEN TauxInverse IS NOT NULL AND TauxInverse <> 0 THEN ROUND(MontantSource / TauxInverse, 2)
    ELSE NULL
END;

DELETE FROM tmp_mig_retrait_devise
WHERE MontantPrincipal IS NULL OR MontantPrincipal <= 0;

-- Créer les wallets principaux manquants
INSERT INTO WalletsAgents (AgentId, DeviseId, SoldeCourant, SoldeDisponible, DateCreation, Statut)
SELECT DISTINCT t.AgentId, @PrincipalDeviseId, 0, 0, NOW(), 1
FROM tmp_mig_retrait_devise t
WHERE NOT EXISTS (
    SELECT 1
    FROM WalletsAgents p
    WHERE p.AgentId = t.AgentId
      AND p.DeviseId = @PrincipalDeviseId
      AND p.Statut = 1
);

UPDATE tmp_mig_retrait_devise t
INNER JOIN WalletsAgents p
    ON p.AgentId = t.AgentId
   AND p.DeviseId = @PrincipalDeviseId
   AND p.Statut = 1
SET t.PrincipalWalletId = p.IdWalletAgent;

DELETE FROM tmp_mig_retrait_devise
WHERE PrincipalWalletId IS NULL;

-- Mouvements d'audit (débit source)
INSERT INTO WalletMouvements (
    WalletId, DeviseId, Montant, TypeOperation, Source, Description,
    DateOperation, DateCreation, Statut
)
SELECT
    t.SourceWalletId,
    t.SourceDeviseId,
    t.MontantSource,
    'DEBIT',
    'MIG_RETRAIT_DEVISE',
    CONCAT(
        'Migration retrait devise principale — transfert ',
        t.SourceDeviseCode,
        ' vers devise principale'
    ),
    NOW(),
    NOW(),
    1
FROM tmp_mig_retrait_devise t;

-- Mouvements d'audit (crédit principal)
INSERT INTO WalletMouvements (
    WalletId, DeviseId, Montant, TypeOperation, Source, Description,
    DateOperation, DateCreation, Statut
)
SELECT
    t.PrincipalWalletId,
    @PrincipalDeviseId,
    t.MontantPrincipal,
    'CREDIT',
    'MIG_RETRAIT_DEVISE',
    CONCAT(
        'Migration retrait devise principale — reçu depuis wallet #',
        t.SourceWalletId,
        ' (',
        t.SourceDeviseCode,
        ')'
    ),
    NOW(),
    NOW(),
    1
FROM tmp_mig_retrait_devise t;

-- Soldes : vider la source, créditer le principal
UPDATE WalletsAgents w
INNER JOIN tmp_mig_retrait_devise t ON t.SourceWalletId = w.IdWalletAgent
SET w.SoldeCourant = 0,
    w.SoldeDisponible = 0,
    w.DateModification = NOW();

UPDATE WalletsAgents w
INNER JOIN tmp_mig_retrait_devise t ON t.PrincipalWalletId = w.IdWalletAgent
SET w.SoldeCourant = w.SoldeCourant + t.MontantPrincipal,
    w.SoldeDisponible = w.SoldeDisponible + t.MontantPrincipal,
    w.DateModification = NOW();

DROP TEMPORARY TABLE IF EXISTS tmp_mig_retrait_devise;

COMMIT;

-- Résumé post-migration
SELECT
    CASE
        WHEN @PrincipalDeviseId IS NULL THEN 'ERREUR : devise principale introuvable.'
        ELSE 'Migration retrait devise principale terminée.'
    END AS Resultat;

SELECT
    w.AgentId,
    a.Matricule,
    d.Code AS DeviseWallet,
    w.SoldeCourant,
    w.SoldeDisponible
FROM WalletsAgents w
INNER JOIN Agents a ON a.IdAgent = w.AgentId
INNER JOIN Devises d ON d.IdDevise = w.DeviseId
WHERE w.Statut = 1
  AND (w.SoldeCourant <> 0 OR w.SoldeDisponible <> 0)
ORDER BY w.AgentId, d.EstDevisePrincipale DESC, d.Code;
