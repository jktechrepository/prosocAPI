-- =============================================================================
-- Migration : DeviseId sur WalletsVirtuelsAgents
-- Aligné sur EF : AddWalletVirtuelAgentDeviseId (20260610132427)
-- Idempotent — réexécutable sans erreur.
--
-- Backfill : devise principale active, puis première devise disponible.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/AddWalletVirtuelAgentDeviseId.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'WalletsVirtuelsAgents' AND COLUMN_NAME = 'DeviseId'
);

SET @sql = IF(
    @col_exists = 0,
    'ALTER TABLE `WalletsVirtuelsAgents` ADD COLUMN `DeviseId` INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE `WalletsVirtuelsAgents` w
SET w.`DeviseId` = (
    SELECT d.`IdDevise` FROM `Devises` d
    WHERE d.`EstDevisePrincipale` = 1 AND d.`Statut` = 1
    ORDER BY d.`IdDevise` LIMIT 1
)
WHERE w.`DeviseId` IS NULL;

UPDATE `WalletsVirtuelsAgents` w
SET w.`DeviseId` = (SELECT d.`IdDevise` FROM `Devises` d ORDER BY d.`IdDevise` LIMIT 1)
WHERE w.`DeviseId` IS NULL;

SET @sql = IF(
    (SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'WalletsVirtuelsAgents' AND COLUMN_NAME = 'DeviseId') = 'YES',
    'ALTER TABLE `WalletsVirtuelsAgents` MODIFY COLUMN `DeviseId` INT NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'WalletsVirtuelsAgents'
      AND INDEX_NAME = 'IX_WalletsVirtuelsAgents_DeviseId'
);

SET @sql = IF(
    @idx_exists = 0,
    'CREATE INDEX `IX_WalletsVirtuelsAgents_DeviseId` ON `WalletsVirtuelsAgents` (`DeviseId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'WalletsVirtuelsAgents'
      AND CONSTRAINT_NAME = 'FK_WalletsVirtuelsAgents_Devises_DeviseId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_exists = 0,
    'ALTER TABLE `WalletsVirtuelsAgents` ADD CONSTRAINT `FK_WalletsVirtuelsAgents_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260610132427_AddWalletVirtuelAgentDeviseId', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260610132427_AddWalletVirtuelAgentDeviseId'
);

COMMIT;

SELECT 'Migration WalletsVirtuelsAgents.DeviseId terminée.' AS Resultat;
