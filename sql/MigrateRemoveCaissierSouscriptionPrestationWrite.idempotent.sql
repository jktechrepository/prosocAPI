-- =============================================================================
-- Migration : retirer UPDATE/DELETE SouscriptionPrestation du rôle « Caissier »
-- =============================================================================
-- Retire uniquement les attributions RolePermissions Caissier ↔
--   UPDATE_SOUSCRIPTION_PRESTATION, DELETE_SOUSCRIPTION_PRESTATION.
-- Aligné sur SeedData.GetCaissierRolePermissionNames() (lecture seule).
-- Idempotent : un second run ne retire rien (ROW_COUNT = 0).
--
-- Pour un alignement complet catalogue :
--   sql/MigrateCaissierRolePermissions.idempotent.sql
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveCaissierSouscriptionPrestationWrite.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Caissier (JWT).
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @CaissierRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Caissier' LIMIT 1
);

SELECT
    CASE
        WHEN @CaissierRoleId IS NULL THEN '❌ ERREUR : rôle « Caissier » introuvable'
        ELSE CONCAT('✅ Rôle Caissier (IdRole = ', @CaissierRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_caissier_souscription_write_permission_noms;
CREATE TEMPORARY TABLE tmp_caissier_souscription_write_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_caissier_souscription_write_permission_noms (Nom) VALUES
    ('UPDATE_SOUSCRIPTION_PRESTATION'),
    ('DELETE_SOUSCRIPTION_PRESTATION');

SELECT '=== AVANT : attributions Caissier UPDATE/DELETE SouscriptionPrestation ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_caissier_souscription_write_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @CaissierRoleId
ORDER BY p.Nom;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_caissier_souscription_write_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @CaissierRoleId
  AND @CaissierRoleId IS NOT NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : attributions Caissier ciblées (attendu : 0) ===' AS Section;

SELECT COUNT(*) AS NbAttributionsRestantes
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_caissier_souscription_write_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @CaissierRoleId;

DROP TEMPORARY TABLE IF EXISTS tmp_caissier_souscription_write_permission_noms;

COMMIT;

SELECT '✅ Migration Caissier (retrait UPDATE/DELETE SouscriptionPrestation) terminée. Reconnectez les comptes Caissier.' AS Resultat;
