-- =============================================================================
-- Migration : suppression CREATE_USER et DELETE_USER du système
-- =============================================================================
-- Objectif :
--   - retirer les attributions RolePermissions liées à CREATE_USER / DELETE_USER
--   - supprimer les permissions CREATE_USER / DELETE_USER de la table Permissions
-- Idempotent : ré-exécutable sans effet de bord.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveCreateDeleteUserPermissions.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @CreateUserPermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'CREATE_USER' LIMIT 1
);
SET @DeleteUserPermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'DELETE_USER' LIMIT 1
);

SELECT
    CASE
        WHEN @CreateUserPermissionId IS NULL THEN '⚠ Permission CREATE_USER introuvable'
        ELSE CONCAT('✅ Permission CREATE_USER (IdPermission = ', @CreateUserPermissionId, ')')
    END AS DiagnosticCreateUser;

SELECT
    CASE
        WHEN @DeleteUserPermissionId IS NULL THEN '⚠ Permission DELETE_USER introuvable'
        ELSE CONCAT('✅ Permission DELETE_USER (IdPermission = ', @DeleteUserPermissionId, ')')
    END AS DiagnosticDeleteUser;

SELECT '=== AVANT : attributions CREATE_USER / DELETE_USER ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.RoleId
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.PermissionId IN (@CreateUserPermissionId, @DeleteUserPermissionId)
ORDER BY r.Nom, p.Nom;

DELETE rp
FROM RolePermissions rp
WHERE rp.PermissionId IN (@CreateUserPermissionId, @DeleteUserPermissionId)
  AND (@CreateUserPermissionId IS NOT NULL OR @DeleteUserPermissionId IS NOT NULL);

SELECT ROW_COUNT() AS NbAttributionsRetirees;

DELETE FROM Permissions
WHERE IdPermission IN (@CreateUserPermissionId, @DeleteUserPermissionId)
  AND (@CreateUserPermissionId IS NOT NULL OR @DeleteUserPermissionId IS NOT NULL);

SELECT ROW_COUNT() AS NbPermissionsSupprimees;

SELECT '=== APRÈS : vérification CREATE_USER / DELETE_USER ===' AS Section;

SELECT COUNT(*) AS NbAttributionsRestantes
FROM RolePermissions rp
WHERE rp.PermissionId IN (@CreateUserPermissionId, @DeleteUserPermissionId);

SELECT COUNT(*) AS NbPermissionsRestantes
FROM Permissions p
WHERE p.Nom IN ('CREATE_USER', 'DELETE_USER');

COMMIT;

SELECT '✅ Migration CREATE_USER/DELETE_USER terminée. Reconnectez les utilisateurs pour renouveler le JWT.' AS Resultat;
