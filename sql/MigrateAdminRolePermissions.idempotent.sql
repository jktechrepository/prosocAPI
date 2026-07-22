-- =============================================================================
-- Migration : permissions du rôle « Admin »
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → FilterPermissionsForAdminRole()
-- Politique : toutes les permissions actives SAUF :
--   - celles dont le nom commence par DELETE_
--   - MANAGE_SYSTEM (réservé SuperAdmin)
--
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire DELETE_*, MANAGE_SYSTEM et toute permission inactive/hors règle
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAdminRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @AdminRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Admin' LIMIT 1
);

SELECT
    CASE
        WHEN @AdminRoleId IS NULL THEN '❌ ERREUR : rôle « Admin » introuvable'
        ELSE CONCAT('✅ Rôle Admin (IdRole = ', @AdminRoleId, ')')
    END AS DiagnosticRole;

SELECT '=== AVANT : permissions du rôle Admin ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AdminRoleId
ORDER BY p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @AdminRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
WHERE p.Statut = 1
  AND p.Nom NOT LIKE 'DELETE\_%' ESCAPE '\\'
  AND p.Nom <> 'MANAGE_SYSTEM'
  AND @AdminRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @AdminRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AdminRoleId
  AND @AdminRoleId IS NOT NULL
  AND (
      p.Statut = 0
      OR p.Nom LIKE 'DELETE\_%' ESCAPE '\\'
      OR p.Nom = 'MANAGE_SYSTEM'
  );

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : permissions du rôle Admin ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AdminRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (
        SELECT COUNT(*)
        FROM Permissions p
        WHERE p.Statut = 1
          AND p.Nom NOT LIKE 'DELETE\_%' ESCAPE '\\'
          AND p.Nom <> 'MANAGE_SYSTEM'
    ) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AdminRoleId
  AND p.Statut = 1
  AND p.Nom NOT LIKE 'DELETE\_%' ESCAPE '\\'
  AND p.Nom <> 'MANAGE_SYSTEM';

COMMIT;

SELECT '✅ Migration permissions Admin terminée. Reconnectez les comptes Admin.' AS Resultat;
