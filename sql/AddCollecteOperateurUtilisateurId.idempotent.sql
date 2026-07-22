-- =============================================================================
-- Migration : OperateurUtilisateurId sur Collectes (traçabilité guichet / caissier)
-- Aligné sur EF : AddCollecteOperateurUtilisateurId
-- Idempotent — réexécutable sans erreur.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/AddCollecteOperateurUtilisateurId.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'OperateurUtilisateurId'
);

SET @sql = IF(
    @col_exists = 0,
    'ALTER TABLE `Collectes` ADD COLUMN `OperateurUtilisateurId` INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes'
      AND INDEX_NAME = 'IX_Collectes_OperateurUtilisateurId'
);

SET @sql = IF(
    @idx_exists = 0,
    'CREATE INDEX `IX_Collectes_OperateurUtilisateurId` ON `Collectes` (`OperateurUtilisateurId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_Utilisateurs_OperateurUtilisateurId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_exists = 0,
    'ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_Utilisateurs_OperateurUtilisateurId` FOREIGN KEY (`OperateurUtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260609135516_AddCollecteOperateurUtilisateurId', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260609135516_AddCollecteOperateurUtilisateurId'
);

COMMIT;

SELECT 'Migration Collectes.OperateurUtilisateurId terminée.' AS Resultat;
