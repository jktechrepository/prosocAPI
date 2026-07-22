-- =============================================================================
-- Déploiement UAT/prod : correction GET /api/BonEnvoi (InvalidCastException)
-- =============================================================================
-- Exécuter AVANT ou APRÈS déploiement API (JetonMedicalId int?).
-- Idempotent.
--
-- Usage :
--   mysql -h <host-uat> -u <user> -p <database> < sql/DeployBonEnvoiGetAllFixUat.idempotent.sql
--
-- Diagnostic détaillé : sql/DiagnoseBonEnvoiJetonMedicalId.idempotent.sql
-- Backfill complet R1   : sql/MigrateBonEnvoiJetonMedicalLinkR1.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- Backfill depuis DemandesBonEnvoi
UPDATE BonsEnvoi b
INNER JOIN DemandesBonEnvoi d ON d.BonEnvoiId = b.IdBonEnvoi
SET b.JetonMedicalId = d.JetonMedicalId
WHERE b.JetonMedicalId IS NULL
  AND d.JetonMedicalId IS NOT NULL;

-- Garantir nullable (annule R2 NOT NULL si appliqué)
SET @is_not_null := (
    SELECT IS_NULLABLE
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'BonsEnvoi'
      AND COLUMN_NAME = 'JetonMedicalId'
    LIMIT 1
);

SET @sql := IF(
    @is_not_null = 'NO',
    'ALTER TABLE BonsEnvoi MODIFY COLUMN JetonMedicalId INT NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

COMMIT;

SELECT COUNT(*) AS NbBonsSansJetonRestants
FROM BonsEnvoi
WHERE JetonMedicalId IS NULL;

SELECT '✅ Backfill UAT appliqué. Déployer l''API puis retester GET /api/BonEnvoi.' AS Resultat;
