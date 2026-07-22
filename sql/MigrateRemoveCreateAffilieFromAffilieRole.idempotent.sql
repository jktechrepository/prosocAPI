-- =============================================================================
-- Retirer permissions sensibles du rôle « Affilié »
-- =============================================================================
-- Idempotent :
--   - supprime uniquement les attributions RolePermissions du rôle Affilié
--   - ne modifie pas les autres rôles
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveCreateAffilieFromAffilieRole.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @AffilieRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Affilié' LIMIT 1
);

DROP TEMPORARY TABLE IF EXISTS tmp_affilie_permissions_to_remove;
CREATE TEMPORARY TABLE tmp_affilie_permissions_to_remove (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_affilie_permissions_to_remove (Nom) VALUES
    ('CREATE_AFFILIE'),
    ('CREATE_DEPENDANT'),
    ('UPDATE_DEPENDANT'),
    ('CREATE_ANTECEDENT'),
    ('UPDATE_ANTECEDENT'),
    ('UPDATE_NOTIFICATION');

SELECT
    @AffilieRoleId AS AffilieRoleId;

SELECT
    p.Nom AS PermissionNom,
    COUNT(*) AS BeforeCount
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_affilie_permissions_to_remove t ON t.Nom = p.Nom
WHERE rp.RoleId = @AffilieRoleId
GROUP BY p.Nom
ORDER BY p.Nom;

DELETE rp
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_affilie_permissions_to_remove t ON t.Nom = p.Nom
WHERE rp.RoleId = @AffilieRoleId
  AND @AffilieRoleId IS NOT NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT
    p.Nom AS PermissionNom,
    COUNT(*) AS AfterCount
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_affilie_permissions_to_remove t ON t.Nom = p.Nom
WHERE rp.RoleId = @AffilieRoleId
GROUP BY p.Nom
ORDER BY p.Nom;

DROP TEMPORARY TABLE IF EXISTS tmp_affilie_permissions_to_remove;

COMMIT;

SELECT '✅ Permissions retirées du rôle Affilié. Reconnectez les utilisateurs concernés.' AS Resultat;

