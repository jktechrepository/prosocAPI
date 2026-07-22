-- =============================================================================
-- Migration PRODUCTION : retrait colonne TypeDemande sur DemandesBonEnvoi
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateDropDemandeBonEnvoiTypeDemande.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'DemandesBonEnvoi'
      AND COLUMN_NAME = 'TypeDemande'
);
SET @sql := IF(@hasCol > 0,
    'ALTER TABLE DemandesBonEnvoi DROP COLUMN TypeDemande',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260629163015_DropDemandeBonEnvoiTypeDemande', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260629163015_DropDemandeBonEnvoiTypeDemande'
);

COMMIT;
