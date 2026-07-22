-- =============================================================================
-- Migration : permissions du rôle « Agent Hôpital » (accueil partenaire)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetAgentHopitalRolePermissionNames()
-- Idempotent :
--   - crée le rôle et les permissions workflow si absents
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAgentHopitalRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'USE_JETON_MEDICAL', 'Valider et utiliser un jeton médical', 'JETON_MEDICAL', 'USE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'USE_JETON_MEDICAL');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_HOPITAL_PARTENAIRE', 'Consulter les hôpitaux partenaires', 'HOPITAL_PARTENAIRE', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_HOPITAL_PARTENAIRE');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'ACCESS_DASHBOARD_HOPITAL', 'Accéder au dashboard hôpital', 'DASHBOARD_HOPITAL', 'ACCESS', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ACCESS_DASHBOARD_HOPITAL');

INSERT INTO Roles (Nom, Code, Description, Niveau, Statut, DateCreation)
SELECT 'Agent Hôpital', 'AH', 'Personnel accueil hôpital', 11, 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE Nom = 'Agent Hôpital');

SET @AgentHopitalRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Agent Hôpital' LIMIT 1
);

SELECT
    CASE
        WHEN @AgentHopitalRoleId IS NULL THEN '❌ ERREUR : rôle « Agent Hôpital » introuvable'
        ELSE CONCAT('✅ Rôle Agent Hôpital (IdRole = ', @AgentHopitalRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_agent_hopital_permission_noms;
CREATE TEMPORARY TABLE tmp_agent_hopital_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_agent_hopital_permission_noms (Nom) VALUES
    ('SCAN_BON_ENVOI'),
    ('READ_BON_ENVOI'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('READ_JETON_MEDICAL'),
    ('USE_JETON_MEDICAL'),
    ('READ_AFFILIE'),
    ('READ_ADHESION'),
    ('READ_DEPENDANT'),
    ('READ_ANTECEDENT'),
    ('READ_PRESTATION'),
    ('READ_PRODUIT_MUTUEL'),
    ('READ_PRODUIT_ASSUREUR'),
    ('READ_HOPITAL_PARTENAIRE'),
    ('ACCESS_DASHBOARD_HOPITAL'),
    ('READ_NOTIFICATION'),
    ('UPDATE_NOTIFICATION');

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @AgentHopitalRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
INNER JOIN tmp_agent_hopital_permission_noms t ON t.Nom = p.Nom
WHERE p.Statut = 1
  AND @AgentHopitalRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @AgentHopitalRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_agent_hopital_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AgentHopitalRoleId
  AND @AgentHopitalRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

-- Retirer SCAN_BON_ENVOI du rôle Assureur (hors périmètre assurance)
SET @AssureurRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Assureur' LIMIT 1);

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AssureurRoleId
  AND @AssureurRoleId IS NOT NULL
  AND p.Nom = 'SCAN_BON_ENVOI';

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_agent_hopital_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_agent_hopital_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AgentHopitalRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_agent_hopital_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Agent Hôpital terminée. Reconnectez les comptes hôpital.' AS Resultat;
