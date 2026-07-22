-- =============================================================================
-- Migration : normalisation Collecte.StatutPaiement → EN_ATTENTE | VALIDE
-- =============================================================================
-- Modèle retenu :
--   EN_ATTENTE = paiement FlexPay non finalisé
--   VALIDE     = paiement effectué (état final)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCollecteStatutPaiementValide.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SELECT '=== AVANT : répartition StatutPaiement ===' AS Section;

SELECT
    COALESCE(StatutPaiement, '(NULL)') AS StatutPaiement,
    COUNT(*) AS NbCollectes
FROM Collectes
GROUP BY StatutPaiement
ORDER BY NbCollectes DESC;

UPDATE Collectes
SET StatutPaiement = 'VALIDE'
WHERE StatutPaiement IS NOT NULL
  AND UPPER(TRIM(StatutPaiement)) NOT IN ('EN_ATTENTE', 'VALIDE')
  AND UPPER(TRIM(StatutPaiement)) IN (
    'OK',
    'VALIDE', 'VALIDÉ',
    'PAYE', 'PAYÉ',
    'CONFIRME', 'CONFIRMÉ'
  );

SELECT ROW_COUNT() AS NbCollectesNormaliseesValide;

UPDATE Collectes
SET StatutPaiement = 'VALIDE'
WHERE StatutPaiement IS NOT NULL
  AND UPPER(TRIM(StatutPaiement)) NOT IN ('EN_ATTENTE', 'VALIDE')
  AND StatutPaiement IN ('Validé', 'Valide', 'Payé', 'Paye', 'Confirmé', 'Confirme');

SELECT ROW_COUNT() AS NbCollectesNormaliseesVariantesAccent;

SELECT '=== APRÈS : répartition StatutPaiement ===' AS Section;

SELECT
    COALESCE(StatutPaiement, '(NULL)') AS StatutPaiement,
    COUNT(*) AS NbCollectes
FROM Collectes
GROUP BY StatutPaiement
ORDER BY NbCollectes DESC;

SELECT
    COUNT(*) AS NbHorsCanonique
FROM Collectes
WHERE StatutPaiement IS NOT NULL
  AND UPPER(TRIM(StatutPaiement)) NOT IN ('EN_ATTENTE', 'VALIDE');

COMMIT;

SELECT '✅ Migration StatutPaiement collecte terminée.' AS Resultat;
