-- =============================================================================
-- Migration : TargetAgent — objectifs par rôle applicatif (RoleId)
-- =============================================================================
-- Aligné sur EF migration 20260613124018_TargetAgentRoleId
-- Idempotent : peut être relancé si RoleId déjà renseigné (étapes UPDATE ignorées)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateTargetAgentRoleId.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- Étape 1 : ajouter RoleId si absent (migration EF déjà appliquée → colonne existe)
SET @hasRoleId := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND COLUMN_NAME = 'RoleId'
);

SET @hasAgentId := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND COLUMN_NAME = 'AgentId'
);

SELECT
    CASE
        WHEN @hasRoleId > 0 AND @hasAgentId = 0 THEN '✅ Déjà migré (RoleId présent, AgentId absent)'
        WHEN @hasAgentId > 0 THEN '⏳ Migration AgentId → RoleId en cours…'
        ELSE '❌ Table TargetsAgents introuvable ou état inattendu'
    END AS Diagnostic;

-- Migration données uniquement si AgentId existe encore
SET @sql := IF(@hasAgentId > 0 AND @hasRoleId = 0,
    'ALTER TABLE TargetsAgents ADD COLUMN RoleId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasAgentId > 0,
    'UPDATE TargetsAgents t
     LEFT JOIN Utilisateurs u ON u.AgentId = t.AgentId AND u.RoleId IS NOT NULL
     LEFT JOIN Agents a ON a.IdAgent = t.AgentId
     LEFT JOIN Roles r ON r.Nom = COALESCE(
         (SELECT r2.Nom FROM Roles r2 WHERE r2.IdRole = u.RoleId LIMIT 1),
         a.RoleAgent
     )
     SET t.RoleId = r.IdRole
     WHERE r.IdRole IS NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasAgentId > 0,
    'UPDATE TargetsAgents t
     SET t.RoleId = (SELECT IdRole FROM Roles WHERE Nom = ''Agent (AT)'' LIMIT 1)
     WHERE t.RoleId IS NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasAgentId > 0,
    'DELETE t1 FROM TargetsAgents t1
     INNER JOIN TargetsAgents t2
         ON t1.RoleId = t2.RoleId
         AND t1.Periodicite = t2.Periodicite
         AND t1.IdTargetAgent < t2.IdTargetAgent',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasAgentId > 0,
    'ALTER TABLE TargetsAgents DROP FOREIGN KEY FK_TargetsAgents_Agents_AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasAgentId > 0,
    'ALTER TABLE TargetsAgents DROP INDEX IX_TargetsAgents_AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasAgentId > 0,
    'ALTER TABLE TargetsAgents DROP COLUMN AgentId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(@hasRoleId > 0 OR @hasAgentId > 0,
    'ALTER TABLE TargetsAgents MODIFY RoleId INT NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Index + FK (ignorés si déjà présents)
SET @idxExists := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND INDEX_NAME = 'IX_TargetsAgents_RoleId_Periodicite'
);

SET @sql := IF(@idxExists = 0,
    'CREATE INDEX IX_TargetsAgents_RoleId_Periodicite ON TargetsAgents (RoleId, Periodicite)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkExists := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TargetsAgents'
      AND CONSTRAINT_NAME = 'FK_TargetsAgents_Roles_RoleId'
);

SET @sql := IF(@fkExists = 0,
    'ALTER TABLE TargetsAgents
     ADD CONSTRAINT FK_TargetsAgents_Roles_RoleId
     FOREIGN KEY (RoleId) REFERENCES Roles (IdRole) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Seed objectifs AT (5 / 25 / 100) si manquants
SET @AtRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Agent (AT)' LIMIT 1);

INSERT INTO TargetsAgents (RoleId, LibelleTarget, Periodicite, Nombre, Statut, DateCreation)
SELECT @AtRoleId, 'Objectif adhésions AT — journalier', 1, 5, 1, NOW()
WHERE @AtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM TargetsAgents
      WHERE RoleId = @AtRoleId AND Periodicite = 1 AND Statut = 1
  );

INSERT INTO TargetsAgents (RoleId, LibelleTarget, Periodicite, Nombre, Statut, DateCreation)
SELECT @AtRoleId, 'Objectif adhésions AT — hebdomadaire', 2, 25, 1, NOW()
WHERE @AtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM TargetsAgents
      WHERE RoleId = @AtRoleId AND Periodicite = 2 AND Statut = 1
  );

INSERT INTO TargetsAgents (RoleId, LibelleTarget, Periodicite, Nombre, Statut, DateCreation)
SELECT @AtRoleId, 'Objectif adhésions AT — mensuel', 3, 100, 1, NOW()
WHERE @AtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM TargetsAgents
      WHERE RoleId = @AtRoleId AND Periodicite = 3 AND Statut = 1
  );

COMMIT;

SELECT '✅ Migration TargetAgent RoleId terminée.' AS Resultat;
