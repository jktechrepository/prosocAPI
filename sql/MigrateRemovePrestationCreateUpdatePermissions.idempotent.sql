-- =============================================================================
-- Migration : retirer CREATE_PRESTATION et UPDATE_PRESTATION (tous rôles)
-- =============================================================================
-- Les prestations sont synchronisées via CREATE/UPDATE Produit Mutuel / Assureur.
-- Idempotent : ne touche que RolePermissions (les lignes Permissions restent).
--
-- Aligné sur SeedData.GetItRolePermissionNames() (CREATE/UPDATE_PRESTATION absents).
-- Catalogue IT : sql/MigrateItRolePermissions.idempotent.sql
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemovePrestationCreateUpdatePermissions.idempotent.sql
--
-- Après exécution : redéployer l'API (POST/PUT/DELETE /api/Prestation → 403) + reconnecter JWT.
-- =============================================================================

START TRANSACTION;

SELECT '=== AVANT : attributions CREATE/UPDATE_PRESTATION ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN Roles r ON r.IdRole = rp.RoleId
WHERE p.Nom IN ('CREATE_PRESTATION', 'UPDATE_PRESTATION')
ORDER BY r.Nom, p.Nom;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE p.Nom IN ('CREATE_PRESTATION', 'UPDATE_PRESTATION');

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : attributions CREATE/UPDATE_PRESTATION (attendu : 0) ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN Roles r ON r.IdRole = rp.RoleId
WHERE p.Nom IN ('CREATE_PRESTATION', 'UPDATE_PRESTATION');

COMMIT;

SELECT '✅ CREATE_PRESTATION / UPDATE_PRESTATION retirés des rôles. Reconnectez les comptes concernés.' AS Resultat;
