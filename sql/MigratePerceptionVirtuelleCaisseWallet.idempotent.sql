-- =============================================================================
-- Migration : PerceptionVirtuelle ↔ MouvementCaisse (CW)
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePerceptionVirtuelleCaisseWallet.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'MouvementsCaisses'
      AND COLUMN_NAME = 'PerceptionVirtuelleId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE MouvementsCaisses ADD COLUMN PerceptionVirtuelleId INT NULL AFTER WalletMouvementId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasIdx := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'MouvementsCaisses'
      AND INDEX_NAME = 'IX_MouvementsCaisses_PerceptionVirtuelleId'
);
SET @sql := IF(@hasIdx = 0,
    'CREATE INDEX IX_MouvementsCaisses_PerceptionVirtuelleId ON MouvementsCaisses (PerceptionVirtuelleId)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'MouvementsCaisses'
      AND CONSTRAINT_NAME = 'FK_MouvementsCaisses_PerceptionsVirtuelles_PerceptionVirtuelleId'
);
SET @sql := IF(@hasFk = 0,
    'ALTER TABLE MouvementsCaisses
     ADD CONSTRAINT FK_MouvementsCaisses_PerceptionsVirtuelles_PerceptionVirtuelleId
     FOREIGN KEY (PerceptionVirtuelleId) REFERENCES PerceptionsVirtuelles (IdPerceptionVirtuelle)
     ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

COMMIT;

SELECT '✅ Migration PerceptionVirtuelle ↔ Caisse/Wallet terminée.' AS Resultat;
