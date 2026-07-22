-- =============================================================================
-- Migration prod : Utilisateur.HopitalPartenaireId + TarifsCotisation.DeviseId
-- Aligné sur migrations EF :
--   20260609093538_AddUtilisateurHopitalPartenaireId
--   20260609100034_AddTarifCotisationDeviseId
-- Idempotent — réexécutable sans erreur.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateProdSchema_HopitalPartenaire_Devise.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

-- ---------------------------------------------------------------------------
-- 1) Utilisateurs.HopitalPartenaireId (nullable, FK SetNull)
-- ---------------------------------------------------------------------------
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

-- ---------------------------------------------------------------------------
-- 2) TarifsCotisation.DeviseId (backfill USD, NOT NULL, FK Restrict)
-- ---------------------------------------------------------------------------
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
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation'
      AND INDEX_NAME = 'IX_TarifsCotisation_DeviseId'
);

SET @sql = IF(
    @idx_exists = 0,
    'CREATE INDEX `IX_TarifsCotisation_DeviseId` ON `TarifsCotisation` (`DeviseId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation'
      AND CONSTRAINT_NAME = 'FK_TarifsCotisation_Devises_DeviseId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_exists = 0,
    'ALTER TABLE `TarifsCotisation` ADD CONSTRAINT `FK_TarifsCotisation_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 3) Enregistrer les migrations EF (évite rejeu dotnet ef database update)
-- ---------------------------------------------------------------------------
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260609093538_AddUtilisateurHopitalPartenaireId', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260609093538_AddUtilisateurHopitalPartenaireId'
);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260609100034_AddTarifCotisationDeviseId', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260609100034_AddTarifCotisationDeviseId'
);

COMMIT;

-- Vérification
SELECT 'Utilisateurs.HopitalPartenaireId' AS CheckItem,
       COUNT(*) AS Present
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Utilisateurs' AND COLUMN_NAME = 'HopitalPartenaireId';

SELECT 'TarifsCotisation.DeviseId' AS CheckItem,
       COUNT(*) AS Present
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'TarifsCotisation' AND COLUMN_NAME = 'DeviseId';

SELECT MigrationId FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%HopitalPartenaire%' OR MigrationId LIKE '%TarifCotisationDevise%';

SELECT 'Migration prod HopitalPartenaireId + DeviseId terminée.' AS Resultat;
