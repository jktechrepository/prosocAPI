-- =============================================================================
-- Migration : Adhesions.AgentId et Collectes.AgentId nullable
-- =============================================================================
-- Aligné sur EF :
--   - 20260626083038_AdhesionAgentIdNullable
--   - 20260626083844_CollecteAgentIdNullable
--
-- Pour la production, utiliser de préférence :
--   sql/MigrateAdhesionCollecteAgentIdNullable.production.idempotent.sql
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAdhesionCollecteAgentIdNullable.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasAdhesions := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Adhesions'
);
SET @hasCollectes := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes'
);

UPDATE Adhesions a
LEFT JOIN Agents ag ON ag.IdAgent = a.AgentId
SET a.AgentId = NULL
WHERE a.AgentId IS NOT NULL AND (a.AgentId = 0 OR ag.IdAgent IS NULL);

UPDATE Collectes c
LEFT JOIN Agents ag ON ag.IdAgent = c.AgentId
SET c.AgentId = NULL
WHERE c.AgentId IS NOT NULL AND (c.AgentId = 0 OR ag.IdAgent IS NULL);

SET @fkAdhesion := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Adhesions'
      AND CONSTRAINT_NAME = 'FK_Adhesions_Agents_AgentId' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);
SET @sql := IF(@hasAdhesions = 1 AND @fkAdhesion > 0,
    'ALTER TABLE Adhesions DROP FOREIGN KEY FK_Adhesions_Agents_AgentId', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @adhesionAgentIdNullable := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Adhesions'
      AND COLUMN_NAME = 'AgentId' AND IS_NULLABLE = 'YES'
);
SET @sql := IF(@hasAdhesions = 1 AND @adhesionAgentIdNullable = 0,
    'ALTER TABLE Adhesions MODIFY COLUMN AgentId INT NULL', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkAdhesion := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Adhesions'
      AND CONSTRAINT_NAME = 'FK_Adhesions_Agents_AgentId' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);
SET @sql := IF(@hasAdhesions = 1 AND @fkAdhesion = 0,
    'ALTER TABLE Adhesions ADD CONSTRAINT FK_Adhesions_Agents_AgentId FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent)', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260626083038_AdhesionAgentIdNullable', '6.0.25' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260626083038_AdhesionAgentIdNullable');

SET @fkCollecte := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_Agents_AgentId' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);
SET @sql := IF(@hasCollectes = 1 AND @fkCollecte > 0,
    'ALTER TABLE Collectes DROP FOREIGN KEY FK_Collectes_Agents_AgentId', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @collecteAgentIdNullable := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes'
      AND COLUMN_NAME = 'AgentId' AND IS_NULLABLE = 'YES'
);
SET @sql := IF(@hasCollectes = 1 AND @collecteAgentIdNullable = 0,
    'ALTER TABLE Collectes MODIFY COLUMN AgentId INT NULL', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkCollecte := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_Agents_AgentId' AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);
SET @sql := IF(@hasCollectes = 1 AND @fkCollecte = 0,
    'ALTER TABLE Collectes ADD CONSTRAINT FK_Collectes_Agents_AgentId FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent)', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260626083844_CollecteAgentIdNullable', '6.0.25' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260626083844_CollecteAgentIdNullable');

COMMIT;
