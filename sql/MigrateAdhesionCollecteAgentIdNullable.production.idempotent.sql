-- =============================================================================
-- Migration PRODUCTION : Adhesions.AgentId et Collectes.AgentId nullable
-- =============================================================================
-- Objectif :
--   - permettre Adhesion.AgentId = NULL (adhésions en ligne sans gestionnaire AT)
--   - permettre Collecte.AgentId = NULL (collecte FlexPay sans agent)
--   - recréer les FK vers Agents sans ON DELETE CASCADE
--
-- Prérequis :
--   - migration 20260625092924_AddPrestationPeriodicite déjà appliquée (ou équivalent)
--
-- Idempotent : relançable sans erreur si déjà appliqué.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAdhesionCollecteAgentIdNullable.production.idempotent.sql
--
-- Script EF brut (référence) :
--   sql/MigrateAdhesionCollecteAgentIdNullable.ef.generated.sql
-- =============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 0) Diagnostics
-- ---------------------------------------------------------------------------
SET @hasAdhesions := (
    SELECT COUNT(*)
    FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
);

SET @hasCollectes := (
    SELECT COUNT(*)
    FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Collectes'
);

SET @adhesionAgentIdNullable := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND COLUMN_NAME = 'AgentId'
      AND IS_NULLABLE = 'YES'
);

SET @collecteAgentIdNullable := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Collectes'
      AND COLUMN_NAME = 'AgentId'
      AND IS_NULLABLE = 'YES'
);

SELECT
    CASE
        WHEN @hasAdhesions = 0 THEN 'ERREUR : table Adhesions introuvable'
        WHEN @hasCollectes = 0 THEN 'ERREUR : table Collectes introuvable'
        WHEN @adhesionAgentIdNullable > 0 AND @collecteAgentIdNullable > 0
            THEN 'OK : AgentId déjà nullable sur Adhesions et Collectes'
        ELSE 'INFO : migration AgentId nullable en cours'
    END AS Diagnostic;

-- ---------------------------------------------------------------------------
-- 1) Nettoyage données invalides (AgentId = 0 ou agent inexistant)
-- ---------------------------------------------------------------------------
UPDATE Adhesions a
LEFT JOIN Agents ag ON ag.IdAgent = a.AgentId
SET a.AgentId = NULL
WHERE a.AgentId IS NOT NULL
  AND (a.AgentId = 0 OR ag.IdAgent IS NULL);

UPDATE Collectes c
LEFT JOIN Agents ag ON ag.IdAgent = c.AgentId
SET c.AgentId = NULL
WHERE c.AgentId IS NOT NULL
  AND (c.AgentId = 0 OR ag.IdAgent IS NULL);

-- ---------------------------------------------------------------------------
-- 2) Adhesions.AgentId nullable
-- ---------------------------------------------------------------------------
SET @fkAdhesion := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND CONSTRAINT_NAME = 'FK_Adhesions_Agents_AgentId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@hasAdhesions = 1 AND @fkAdhesion > 0,
    'ALTER TABLE Adhesions DROP FOREIGN KEY FK_Adhesions_Agents_AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @adhesionAgentIdNullable := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND COLUMN_NAME = 'AgentId'
      AND IS_NULLABLE = 'YES'
);

SET @sql := IF(@hasAdhesions = 1 AND @adhesionAgentIdNullable = 0,
    'ALTER TABLE Adhesions MODIFY COLUMN AgentId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkAdhesion := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND CONSTRAINT_NAME = 'FK_Adhesions_Agents_AgentId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@hasAdhesions = 1 AND @fkAdhesion = 0,
    'ALTER TABLE Adhesions ADD CONSTRAINT FK_Adhesions_Agents_AgentId FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260626083038_AdhesionAgentIdNullable', '6.0.25'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM __EFMigrationsHistory
    WHERE MigrationId = '20260626083038_AdhesionAgentIdNullable'
);

-- ---------------------------------------------------------------------------
-- 3) Collectes.AgentId nullable
-- ---------------------------------------------------------------------------
SET @fkCollecte := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_Agents_AgentId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@hasCollectes = 1 AND @fkCollecte > 0,
    'ALTER TABLE Collectes DROP FOREIGN KEY FK_Collectes_Agents_AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @collecteAgentIdNullable := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Collectes'
      AND COLUMN_NAME = 'AgentId'
      AND IS_NULLABLE = 'YES'
);

SET @sql := IF(@hasCollectes = 1 AND @collecteAgentIdNullable = 0,
    'ALTER TABLE Collectes MODIFY COLUMN AgentId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkCollecte := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_Agents_AgentId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@hasCollectes = 1 AND @fkCollecte = 0,
    'ALTER TABLE Collectes ADD CONSTRAINT FK_Collectes_Agents_AgentId FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260626083844_CollecteAgentIdNullable', '6.0.25'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM __EFMigrationsHistory
    WHERE MigrationId = '20260626083844_CollecteAgentIdNullable'
);

-- ---------------------------------------------------------------------------
-- 4) Vérification
-- ---------------------------------------------------------------------------
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_TYPE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN ('Adhesions', 'Collectes')
  AND COLUMN_NAME = 'AgentId'
ORDER BY TABLE_NAME;

SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
WHERE MigrationId IN (
    '20260626083038_AdhesionAgentIdNullable',
    '20260626083844_CollecteAgentIdNullable'
)
ORDER BY MigrationId;

COMMIT;
