-- =============================================================================
-- Migration : permissions du rôle « SuperAdmin »
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → FilterPermissionsForSuperAdminRole()
-- Politique : toutes les permissions actives (catalogue complet, incl. DELETE_* et MANAGE_SYSTEM)
--
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions inactives ou hors catalogue
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateSuperAdminRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @SuperAdminRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'SuperAdmin' LIMIT 1
);

SELECT
    CASE
        WHEN @SuperAdminRoleId IS NULL THEN '❌ ERREUR : rôle « SuperAdmin » introuvable'
        ELSE CONCAT('✅ Rôle SuperAdmin (IdRole = ', @SuperAdminRoleId, ')')
    END AS DiagnosticRole;

SELECT '=== AVANT : permissions du rôle SuperAdmin ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Statut AS PermissionActive,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @SuperAdminRoleId
ORDER BY p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @SuperAdminRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
WHERE p.Statut = 1
  AND @SuperAdminRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @SuperAdminRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
LEFT JOIN Permissions p ON p.IdPermission = rp.PermissionId AND p.Statut = 1
WHERE rp.RoleId = @SuperAdminRoleId
  AND @SuperAdminRoleId IS NOT NULL
  AND p.IdPermission IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : permissions du rôle SuperAdmin ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @SuperAdminRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM Permissions WHERE Statut = 1) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @SuperAdminRoleId
  AND p.Statut = 1;

COMMIT;

SELECT '✅ Migration permissions SuperAdmin terminée. Reconnectez le compte superadmin@prosoc.cd.' AS Resultat;
