-- =============================================================================
-- Retrait des permissions CREATE_AFFILIE et DELETE_AFFILIE
-- =============================================================================
-- La création d'un affilié se fait via le flux adhésion (CREATE_ADHESION).
-- Idempotent : désactive les permissions et retire toutes les attributions.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/RetireCreateDeleteAffiliePermissions.idempotent.sql
-- =============================================================================

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_retired_affilie_permissions;
CREATE TEMPORARY TABLE tmp_retired_affilie_permissions (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_retired_affilie_permissions (Nom) VALUES
    ('CREATE_AFFILIE'),
    ('DELETE_AFFILIE');

-- Retrait des attributions rôles
DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_retired_affilie_permissions t ON t.Nom = p.Nom;

SELECT ROW_COUNT() AS NbRolePermissionsRetirees;

-- Retrait des attributions utilisateurs directes
DELETE up FROM UserPermissions up
INNER JOIN Permissions p ON p.IdPermission = up.PermissionId
INNER JOIN tmp_retired_affilie_permissions t ON t.Nom = p.Nom;

SELECT ROW_COUNT() AS NbUserPermissionsRetirees;

-- Désactivation catalogue
UPDATE Permissions p
INNER JOIN tmp_retired_affilie_permissions t ON t.Nom = p.Nom
SET p.Statut = 0;

SELECT ROW_COUNT() AS NbPermissionsDesactivees;

SELECT p.Nom, p.Statut, p.Description
FROM Permissions p
INNER JOIN tmp_retired_affilie_permissions t ON t.Nom = p.Nom;

DROP TEMPORARY TABLE IF EXISTS tmp_retired_affilie_permissions;

COMMIT;

SELECT '✅ CREATE_AFFILIE et DELETE_AFFILIE retirées. Reconnectez les utilisateurs (JWT).' AS Resultat;
