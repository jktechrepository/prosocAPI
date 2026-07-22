-- =============================================================================
-- Migration : Antecedants.DependantId nullable (antécédents par dépendant)
-- =============================================================================
-- Aligné sur EF : 20260706150010_AntecedentDependantIdNullable
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAntecedentDependantId.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasAntecedants := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Antecedants'
);

SET @hasDependantIdColumn := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Antecedants'
      AND COLUMN_NAME = 'DependantId'
);

SET @sql := IF(@hasAntecedants = 1 AND @hasDependantIdColumn = 0,
    'ALTER TABLE Antecedants ADD COLUMN DependantId INT NULL', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasIndex := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Antecedants'
      AND INDEX_NAME = 'IX_Antecedants_DependantId'
);

SET @sql := IF(@hasAntecedants = 1 AND @hasIndex = 0,
    'CREATE INDEX IX_Antecedants_DependantId ON Antecedants (DependantId)', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Antecedants'
      AND CONSTRAINT_NAME = 'FK_Antecedants_Dependants_DependantId' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@hasAntecedants = 1 AND @hasFk = 0,
    'ALTER TABLE Antecedants ADD CONSTRAINT FK_Antecedants_Dependants_DependantId FOREIGN KEY (DependantId) REFERENCES Dependants (IdDependant)', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260706150010_AntecedentDependantIdNullable', '6.0.25' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260706150010_AntecedentDependantIdNullable');

COMMIT;

SELECT '✅ Migration Antecedants.DependantId terminée.' AS Resultat;
