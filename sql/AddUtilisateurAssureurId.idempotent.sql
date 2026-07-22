-- =============================================================================
-- Migration : AssureurId sur Utilisateurs (portail partenaire assureur)
-- Aligné sur EF : AddUtilisateurAssureurId
-- Idempotent — réexécutable sans erreur.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/AddUtilisateurAssureurId.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Utilisateurs' AND COLUMN_NAME = 'AssureurId'
);

SET @sql = IF(
    @col_exists = 0,
    'ALTER TABLE `Utilisateurs` ADD COLUMN `AssureurId` INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idx_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Utilisateurs'
      AND INDEX_NAME = 'IX_Utilisateurs_AssureurId'
);

SET @sql = IF(
    @idx_exists = 0,
    'CREATE INDEX `IX_Utilisateurs_AssureurId` ON `Utilisateurs` (`AssureurId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Utilisateurs'
      AND CONSTRAINT_NAME = 'FK_Utilisateurs_Assureurs_AssureurId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_exists = 0,
    'ALTER TABLE `Utilisateurs` ADD CONSTRAINT `FK_Utilisateurs_Assureurs_AssureurId` FOREIGN KEY (`AssureurId`) REFERENCES `Assureurs` (`IdAssureur`) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260609150229_AddUtilisateurAssureurId', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260609150229_AddUtilisateurAssureurId'
);

COMMIT;

SELECT 'Migration Utilisateurs.AssureurId terminée.' AS Resultat;
