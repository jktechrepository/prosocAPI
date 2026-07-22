-- =============================================================================
-- Migration : DeviseId sur TarifsCotisation
-- =============================================================================
-- Backfill USD (aligné Multidevise:DeviseTarifCotisationCode), puis NOT NULL + FK.
-- Idempotent : peut être rejoué sans erreur si la colonne existe déjà.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/AddTarifCotisationDeviseId.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation' AND COLUMN_NAME = 'DeviseId'
);

SET @sql = IF(
    @col_exists = 0,
    'ALTER TABLE `TarifsCotisation` ADD COLUMN `DeviseId` INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE `TarifsCotisation` tc
SET tc.`DeviseId` = (
    SELECT d.`IdDevise` FROM `Devises` d
    WHERE d.`Code` = 'USD' AND d.`Statut` = 1
    ORDER BY d.`IdDevise` LIMIT 1
)
WHERE tc.`DeviseId` IS NULL;

UPDATE `TarifsCotisation` tc
SET tc.`DeviseId` = (SELECT d.`IdDevise` FROM `Devises` d ORDER BY d.`IdDevise` LIMIT 1)
WHERE tc.`DeviseId` IS NULL;

SET @sql = IF(
    (SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation' AND COLUMN_NAME = 'DeviseId') = 'YES',
    'ALTER TABLE `TarifsCotisation` MODIFY COLUMN `DeviseId` INT NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation' AND INDEX_NAME = 'IX_TarifsCotisation_DeviseId'
);

SET @sql = IF(
    @idx_exists = 0,
    'CREATE INDEX `IX_TarifsCotisation_DeviseId` ON `TarifsCotisation` (`DeviseId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation'
      AND CONSTRAINT_NAME = 'FK_TarifsCotisation_Devises_DeviseId' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_exists = 0,
    'ALTER TABLE `TarifsCotisation` ADD CONSTRAINT `FK_TarifsCotisation_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

COMMIT;

SELECT 'Migration TarifsCotisation.DeviseId terminée.' AS Resultat;
