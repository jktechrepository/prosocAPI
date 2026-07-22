-- =============================================================================
-- Migration : TargetAgent -> objectifs par role applicatif (RoleId)
-- Fichier : production-ready, idempotent, relancable
-- =============================================================================
-- Objectif:
--   - migrer TargetsAgents.AgentId -> TargetsAgents.RoleId
--   - conserver les donnees et dedoublonner (RoleId, Periodicite)
--   - ajouter index + FK si absents
--   - seed des objectifs par defaut Agent (AT) si manquants
--
-- Usage:
--   mysql -h <host> -u <user> -p <database> < sql/MigrateTargetAgentRoleId.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 0) Diagnostics schema
-- ---------------------------------------------------------------------------
SET @hasTargetsAgents := (
    SELECT COUNT(*)
    FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
);

SET @hasRoleId := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND COLUMN_NAME = 'RoleId'
);

SET @hasAgentId := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND COLUMN_NAME = 'AgentId'
);

SELECT
    CASE
        WHEN @hasTargetsAgents = 0 THEN 'ERREUR: table TargetsAgents introuvable'
        WHEN @hasRoleId > 0 AND @hasAgentId = 0 THEN 'OK: deja migre (RoleId present, AgentId absent)'
        WHEN @hasAgentId > 0 THEN 'INFO: migration AgentId -> RoleId en cours'
        ELSE 'INFO: etat intermediaire detecte'
    END AS Diagnostic;

-- ---------------------------------------------------------------------------
-- 1) Ajouter RoleId si necessaire
-- ---------------------------------------------------------------------------
SET @sql := IF(@hasTargetsAgents = 1 AND @hasRoleId = 0,
    'ALTER TABLE TargetsAgents ADD COLUMN RoleId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Recalcul apres possible ajout
SET @hasRoleId := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND COLUMN_NAME = 'RoleId'
);

-- ---------------------------------------------------------------------------
-- 2) Remplir RoleId depuis Utilisateurs.RoleId puis fallback Agent.RoleAgent
-- ---------------------------------------------------------------------------
SET @sql := IF(@hasTargetsAgents = 1 AND @hasAgentId > 0 AND @hasRoleId > 0,
    'UPDATE TargetsAgents t
     LEFT JOIN Utilisateurs u ON u.AgentId = t.AgentId AND u.RoleId IS NOT NULL
     LEFT JOIN Agents a ON a.IdAgent = t.AgentId
     LEFT JOIN Roles r ON r.Nom = COALESCE(
         (SELECT r2.Nom FROM Roles r2 WHERE r2.IdRole = u.RoleId LIMIT 1),
         a.RoleAgent
     )
     SET t.RoleId = r.IdRole
     WHERE t.RoleId IS NULL
       AND r.IdRole IS NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Fallback final : Agent (AT)
SET @sql := IF(@hasTargetsAgents = 1 AND @hasAgentId > 0 AND @hasRoleId > 0,
    'UPDATE TargetsAgents t
     SET t.RoleId = (SELECT IdRole FROM Roles WHERE Nom = ''Agent (AT)'' LIMIT 1)
     WHERE t.RoleId IS NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 3) Dedoublonnage avant contrainte
-- ---------------------------------------------------------------------------
SET @sql := IF(@hasTargetsAgents = 1 AND @hasRoleId > 0,
    'DELETE t1
     FROM TargetsAgents t1
     INNER JOIN TargetsAgents t2
       ON t1.RoleId = t2.RoleId
      AND t1.Periodicite = t2.Periodicite
      AND t1.IdTargetAgent < t2.IdTargetAgent',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 4) Drop FK/index AgentId si existants, puis drop colonne AgentId
-- ---------------------------------------------------------------------------
SET @fkAgentExists := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND CONSTRAINT_NAME = 'FK_TargetsAgents_Agents_AgentId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@fkAgentExists > 0,
    'ALTER TABLE TargetsAgents DROP FOREIGN KEY FK_TargetsAgents_Agents_AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @idxAgentExists := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND INDEX_NAME = 'IX_TargetsAgents_AgentId'
);

SET @sql := IF(@idxAgentExists > 0,
    'ALTER TABLE TargetsAgents DROP INDEX IX_TargetsAgents_AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasTargetsAgents = 1 AND @hasAgentId > 0,
    'ALTER TABLE TargetsAgents DROP COLUMN AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 5) Forcer RoleId NOT NULL
-- ---------------------------------------------------------------------------
SET @sql := IF(@hasTargetsAgents = 1 AND @hasRoleId > 0,
    'ALTER TABLE TargetsAgents MODIFY RoleId INT NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 6) Index + FK RoleId
-- ---------------------------------------------------------------------------
SET @idxRolePeriodiciteExists := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND INDEX_NAME = 'IX_TargetsAgents_RoleId_Periodicite'
);

SET @sql := IF(@idxRolePeriodiciteExists = 0 AND @hasTargetsAgents = 1 AND @hasRoleId > 0,
    'CREATE INDEX IX_TargetsAgents_RoleId_Periodicite ON TargetsAgents (RoleId, Periodicite)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkRoleExists := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND CONSTRAINT_NAME = 'FK_TargetsAgents_Roles_RoleId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@fkRoleExists = 0 AND @hasTargetsAgents = 1 AND @hasRoleId > 0,
    'ALTER TABLE TargetsAgents
     ADD CONSTRAINT FK_TargetsAgents_Roles_RoleId
     FOREIGN KEY (RoleId) REFERENCES Roles (IdRole) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 7) Seed objectifs Agent (AT) si manquants
-- ---------------------------------------------------------------------------
SET @AtRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Agent (AT)' LIMIT 1);

INSERT INTO TargetsAgents (RoleId, LibelleTarget, Periodicite, Nombre, Statut, DateCreation)
SELECT @AtRoleId, 'Objectif adhesions AT - journalier', 1, 5, 1, NOW()
WHERE @AtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM TargetsAgents
      WHERE RoleId = @AtRoleId AND Periodicite = 1 AND Statut = 1
  );

INSERT INTO TargetsAgents (RoleId, LibelleTarget, Periodicite, Nombre, Statut, DateCreation)
SELECT @AtRoleId, 'Objectif adhesions AT - hebdomadaire', 2, 25, 1, NOW()
WHERE @AtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM TargetsAgents
      WHERE RoleId = @AtRoleId AND Periodicite = 2 AND Statut = 1
  );

INSERT INTO TargetsAgents (RoleId, LibelleTarget, Periodicite, Nombre, Statut, DateCreation)
SELECT @AtRoleId, 'Objectif adhesions AT - mensuel', 3, 100, 1, NOW()
WHERE @AtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM TargetsAgents
      WHERE RoleId = @AtRoleId AND Periodicite = 3 AND Statut = 1
  );

-- ---------------------------------------------------------------------------
-- 8) Verification rapide post-migration
-- ---------------------------------------------------------------------------
SELECT COLUMN_NAME
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'TargetsAgents'
  AND COLUMN_NAME IN ('AgentId', 'RoleId')
ORDER BY COLUMN_NAME;

SELECT IdTargetAgent, RoleId, Periodicite, Nombre, Statut
FROM TargetsAgents
ORDER BY IdTargetAgent DESC
LIMIT 20;

COMMIT;

SELECT 'OK: Migration TargetAgent RoleId terminee (production-ready).' AS Resultat;
