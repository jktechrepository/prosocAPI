-- =============================================================================
-- Migration : permission ACCESS_DASHBOARD_AGENT_AA pour le rôle Agent (AA)
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateDashboardAgentAaPermission.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'ACCESS_DASHBOARD_AGENT_AA', 'Accéder au dashboard agent administratif (encodeur)', 'DASHBOARD_AGENT_AA', 'ACCESS', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ACCESS_DASHBOARD_AGENT_AA');

SET @AaRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Agent (AA)' LIMIT 1
);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @AaRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom = 'ACCESS_DASHBOARD_AGENT_AA'
  AND p.Statut = 1
  AND @AaRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @AaRoleId AND rp.PermissionId = p.IdPermission
  );

-- Retirer ACCESS_DASHBOARD_AGENT du rôle AA si présent (dashboard terrain réservé à l'AT)
DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AaRoleId
  AND p.Nom = 'ACCESS_DASHBOARD_AGENT'
  AND @AaRoleId IS NOT NULL;

COMMIT;

SELECT '✅ Permission ACCESS_DASHBOARD_AGENT_AA migrée pour Agent (AA).' AS Resultat;
