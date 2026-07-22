-- =============================================================================
-- Migration : retirer UPDATE_COLLECTE de tous les rôles
-- =============================================================================
-- Aucun rôle ne doit modifier une collecte via PUT /api/Collecte/{id}.
-- La validation admin reste via POST /api/DashboardAdmin/validate-collecte/{id}.
-- Idempotent : supprime uniquement les attributions RolePermissions.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveUpdateCollectePermission.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @PermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'UPDATE_COLLECTE' LIMIT 1
);

SELECT
    CASE
        WHEN @PermissionId IS NULL THEN '⚠ Permission UPDATE_COLLECTE introuvable'
        ELSE CONCAT('✅ Permission UPDATE_COLLECTE (IdPermission = ', @PermissionId, ')')
    END AS Diagnostic;

SELECT '=== AVANT : rôles ayant UPDATE_COLLECTE ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.RoleId
WHERE rp.PermissionId = @PermissionId
ORDER BY r.Nom;

DELETE rp FROM RolePermissions rp
WHERE rp.PermissionId = @PermissionId
  AND @PermissionId IS NOT NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : rôles ayant UPDATE_COLLECTE (attendu : 0) ===' AS Section;

SELECT COUNT(*) AS NbAttributionsRestantes
FROM RolePermissions rp
WHERE rp.PermissionId = @PermissionId;

COMMIT;

SELECT '✅ Migration UPDATE_COLLECTE terminée. Reconnectez les utilisateurs pour purger le JWT.' AS Resultat;
