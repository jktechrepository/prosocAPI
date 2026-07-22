-- =============================================================================
-- Migration PRODUCTION : enrichissement WalletVirtuelMouvements
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateWalletVirtuelMouvementEnrichi.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WalletVirtuelMouvements' AND COLUMN_NAME = 'DeviseId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE WalletVirtuelMouvements ADD COLUMN DeviseId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WalletVirtuelMouvements' AND COLUMN_NAME = 'SoldeAvant'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE WalletVirtuelMouvements ADD COLUMN SoldeAvant DECIMAL(18,2) NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WalletVirtuelMouvements' AND COLUMN_NAME = 'SoldeApres'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE WalletVirtuelMouvements ADD COLUMN SoldeApres DECIMAL(18,2) NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WalletVirtuelMouvements' AND COLUMN_NAME = 'OperateurUtilisateurId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE WalletVirtuelMouvements ADD COLUMN OperateurUtilisateurId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'WalletVirtuelMouvements'
      AND CONSTRAINT_NAME = 'FK_WalletVirtuelMouvements_Devises_DeviseId'
);
SET @sql := IF(@hasFk = 0,
    'ALTER TABLE WalletVirtuelMouvements ADD CONSTRAINT FK_WalletVirtuelMouvements_Devises_DeviseId FOREIGN KEY (DeviseId) REFERENCES Devises (IdDevise)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'WalletVirtuelMouvements'
      AND CONSTRAINT_NAME = 'FK_WalletVirtuelMouvements_Utilisateurs_OperateurUtilisateurId'
);
SET @sql := IF(@hasFk = 0,
    'ALTER TABLE WalletVirtuelMouvements ADD CONSTRAINT FK_WalletVirtuelMouvements_Utilisateurs_OperateurUtilisateurId FOREIGN KEY (OperateurUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasIdx := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'WalletVirtuelMouvements'
      AND INDEX_NAME = 'IX_WalletVirtuelMouvements_OperateurUtilisateurId'
);
SET @sql := IF(@hasIdx = 0,
    'CREATE INDEX IX_WalletVirtuelMouvements_OperateurUtilisateurId ON WalletVirtuelMouvements (OperateurUtilisateurId)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE WalletVirtuelMouvements m
INNER JOIN WalletsVirtuelsAgents w ON w.IdWalletVirtuelAgent = m.WalletVirtuelId
SET m.DeviseId = w.DeviseId
WHERE m.DeviseId IS NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260629124757_WalletVirtuelMouvementEnrichi', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260629124757_WalletVirtuelMouvementEnrichi'
);

COMMIT;
