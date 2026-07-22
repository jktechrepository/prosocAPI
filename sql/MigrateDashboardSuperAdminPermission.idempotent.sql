-- =============================================================================
-- Migration : permission ACCESS_DASHBOARD_SUPERADMIN
-- Le rôle SuperAdmin reçoit toutes les permissions actives via
-- sql/MigrateSuperAdminRolePermissions.idempotent.sql
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateDashboardSuperAdminPermission.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'ACCESS_DASHBOARD_SUPERADMIN', 'Accéder au dashboard super administrateur', 'DASHBOARD_SUPERADMIN', 'ACCESS', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ACCESS_DASHBOARD_SUPERADMIN');

SET @SuperAdminRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'SuperAdmin' LIMIT 1
);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @SuperAdminRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom = 'ACCESS_DASHBOARD_SUPERADMIN'
  AND p.Statut = 1
  AND @SuperAdminRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @SuperAdminRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permission ACCESS_DASHBOARD_SUPERADMIN migrée.' AS Resultat;
