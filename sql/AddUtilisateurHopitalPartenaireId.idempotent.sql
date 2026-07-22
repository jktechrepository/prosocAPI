-- =============================================================================
-- Migration : HopitalPartenaireId sur Utilisateurs (Agent Hôpital)
-- Aligné sur EF : 20260609093538_AddUtilisateurHopitalPartenaireId
-- Idempotent — réexécutable sans erreur.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/AddUtilisateurHopitalPartenaireId.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Utilisateurs' AND COLUMN_NAME = 'HopitalPartenaireId'
);

SET @sql = IF(
    @col_exists = 0,
    'ALTER TABLE `Utilisateurs` ADD COLUMN `HopitalPartenaireId` INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Utilisateurs'
      AND INDEX_NAME = 'IX_Utilisateurs_HopitalPartenaireId'
);

SET @sql = IF(
    @idx_exists = 0,
    'CREATE INDEX `IX_Utilisateurs_HopitalPartenaireId` ON `Utilisateurs` (`HopitalPartenaireId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Utilisateurs'
      AND CONSTRAINT_NAME = 'FK_Utilisateurs_HopitalPartenaires_HopitalPartenaireId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_exists = 0,
    'ALTER TABLE `Utilisateurs` ADD CONSTRAINT `FK_Utilisateurs_HopitalPartenaires_HopitalPartenaireId` FOREIGN KEY (`HopitalPartenaireId`) REFERENCES `HopitalPartenaires` (`IdHopital`) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260609093538_AddUtilisateurHopitalPartenaireId', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260609093538_AddUtilisateurHopitalPartenaireId'
);

COMMIT;

SELECT 'Migration Utilisateurs.HopitalPartenaireId terminée.' AS Resultat;
