-- =============================================================================
-- Diagnostic : BonsEnvoi.JetonMedicalId (cause InvalidCastException GET /api/BonEnvoi)
-- =============================================================================
-- Usage UAT / prod :
--   mysql -h <host> -u <user> -p <database> < sql/DiagnoseBonEnvoiJetonMedicalId.idempotent.sql
-- =============================================================================

SELECT '=== 1) Schéma colonne JetonMedicalId ===' AS Section;

SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, COLUMN_TYPE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'BonsEnvoi'
  AND COLUMN_NAME = 'JetonMedicalId';

SELECT '=== 2) Bons sans jeton lié (provocateurs du 500 si modèle C# int non nullable) ===' AS Section;

SELECT COUNT(*) AS NbBonsSansJeton
FROM BonsEnvoi
WHERE JetonMedicalId IS NULL;

SELECT IdBonEnvoi, NumeroBon, AffilieId, PrestationId, JetonMedicalId, DateEmission
FROM BonsEnvoi
WHERE JetonMedicalId IS NULL
ORDER BY IdBonEnvoi
LIMIT 20;

SELECT '=== 3) Backfill possible via DemandesBonEnvoi ===' AS Section;

SELECT COUNT(*) AS NbBonsBackfillables
FROM BonsEnvoi b
INNER JOIN DemandesBonEnvoi d ON d.BonEnvoiId = b.IdBonEnvoi
WHERE b.JetonMedicalId IS NULL
  AND d.JetonMedicalId IS NOT NULL;

SELECT '=== 4) Doublons JetonMedicalId (bloque index unique) ===' AS Section;

SELECT JetonMedicalId, COUNT(*) AS Occurrences
FROM BonsEnvoi
WHERE JetonMedicalId IS NOT NULL
GROUP BY JetonMedicalId
HAVING COUNT(*) > 1;

SELECT '=== Actions recommandées ===' AS Section;
SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM information_schema.COLUMNS
              WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'BonsEnvoi' AND COLUMN_NAME = 'JetonMedicalId') = 0
            THEN 'Appliquer sql/MigrateBonEnvoiJetonMedicalLinkR1.idempotent.sql'
        WHEN (SELECT COUNT(*) FROM BonsEnvoi WHERE JetonMedicalId IS NULL) > 0
            THEN 'Déployer API avec JetonMedicalId int? puis exécuter R1 backfill ; ne pas lancer R2 tant que NULL > 0'
        ELSE 'Données OK — vérifier déploiement API avec JetonMedicalId nullable'
    END AS Recommandation;
