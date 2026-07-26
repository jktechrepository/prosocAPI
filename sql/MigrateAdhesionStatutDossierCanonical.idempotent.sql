-- =============================================================================
-- Migration : normalisation Adhesions.StatutDossier → EN ATTENTE | VALIDÉ
-- =============================================================================
-- Canons (alignés sur AdhesionStatutDossierRegles) :
--   EN ATTENTE = dossier à compléter / encoder
--   VALIDÉ     = dossier validé (encodeur) / utilisable métier
--
-- Mapping legacy :
--   COMPLET, Complet, VALIDE, VALIDÉ, Validé, … → VALIDÉ
--   EN ATTENTE, En Attente, A, B, vide, autres → EN ATTENTE
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAdhesionStatutDossierCanonical.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SELECT '=== AVANT : répartition StatutDossier ===' AS Section;

SELECT
    COALESCE(StatutDossier, '(NULL)') AS StatutDossier,
    COUNT(*) AS NbAdhesions
FROM Adhesions
GROUP BY StatutDossier
ORDER BY NbAdhesions DESC;

-- → VALIDÉ (dossiers considérés clos / complets)
UPDATE Adhesions
SET StatutDossier = 'VALIDÉ'
WHERE StatutDossier IS NOT NULL
  AND StatutDossier <> 'VALIDÉ'
  AND (
        UPPER(TRIM(StatutDossier)) IN ('COMPLET', 'VALIDE', 'VALIDÉ')
        OR TRIM(StatutDossier) IN (
            'Complet', 'complet',
            'Validé', 'Valide', 'validé', 'valide'
        )
      );

SELECT ROW_COUNT() AS NbAdhesionsNormaliseesValide;

-- → EN ATTENTE (tout le reste hors canons déjà OK)
UPDATE Adhesions
SET StatutDossier = 'EN ATTENTE'
WHERE StatutDossier IS NULL
   OR (
        StatutDossier <> 'EN ATTENTE'
        AND StatutDossier <> 'VALIDÉ'
      );

SELECT ROW_COUNT() AS NbAdhesionsNormaliseesEnAttente;

SELECT '=== APRÈS : répartition StatutDossier (attendu : EN ATTENTE | VALIDÉ) ===' AS Section;

SELECT
    COALESCE(StatutDossier, '(NULL)') AS StatutDossier,
    COUNT(*) AS NbAdhesions
FROM Adhesions
GROUP BY StatutDossier
ORDER BY NbAdhesions DESC;

SELECT
    CASE
        WHEN EXISTS (
            SELECT 1 FROM Adhesions
            WHERE StatutDossier IS NULL
               OR StatutDossier NOT IN ('EN ATTENTE', 'VALIDÉ')
        ) THEN 'ERREUR : des valeurs non canoniques restent'
        ELSE 'OK : StatutDossier uniquement EN ATTENTE | VALIDÉ'
    END AS Controle;

COMMIT;

SELECT 'Migration StatutDossier adhésion terminée.' AS Resultat;
