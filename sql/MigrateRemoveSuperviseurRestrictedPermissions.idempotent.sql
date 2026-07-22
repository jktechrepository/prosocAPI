-- =============================================================================
-- Migration : retirer des permissions restreintes du rôle « Superviseur »
-- =============================================================================
-- Retire uniquement ces attributions RolePermissions pour Superviseur :
--   UPDATE_ADHESION, UPDATE_AFFILIE,
--   CREATE_ASSUREUR, READ_ASSUREUR, UPDATE_ASSUREUR,
--   CREATE_PRODUIT_ASSUREUR
--
-- Aligné sur SeedData.GetSuperviseurRolePermissionNames().
-- Idempotent : un second run ne retire rien (ROW_COUNT = 0).
--
-- Pour un alignement complet catalogue (ajoute manquant + purge tout surplus) :
--   sql/MigrateSuperviseurRolePermissions.idempotent.sql
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveSuperviseurRestrictedPermissions.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Superviseur (JWT).
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @SuperviseurRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Superviseur' LIMIT 1
);

SELECT
    CASE
        WHEN @SuperviseurRoleId IS NULL THEN '❌ ERREUR : rôle « Superviseur » introuvable'
        ELSE CONCAT('✅ Rôle Superviseur (IdRole = ', @SuperviseurRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_superviseur_restricted_permission_noms;
CREATE TEMPORARY TABLE tmp_superviseur_restricted_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_superviseur_restricted_permission_noms (Nom) VALUES
    ('UPDATE_ADHESION'),
    ('UPDATE_AFFILIE'),
    ('CREATE_ASSUREUR'),
    ('READ_ASSUREUR'),
    ('UPDATE_ASSUREUR'),
    ('CREATE_PRODUIT_ASSUREUR');

SELECT '=== AVANT : attributions Superviseur à retirer ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_superviseur_restricted_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @SuperviseurRoleId
ORDER BY p.Nom;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_superviseur_restricted_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @SuperviseurRoleId
  AND @SuperviseurRoleId IS NOT NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : attributions Superviseur ciblées (attendu : 0) ===' AS Section;

SELECT COUNT(*) AS NbAttributionsRestantes
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_superviseur_restricted_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @SuperviseurRoleId;

DROP TEMPORARY TABLE IF EXISTS tmp_superviseur_restricted_permission_noms;

COMMIT;

SELECT '✅ Migration Superviseur (permissions restreintes) terminée. Reconnectez les comptes Superviseur.' AS Resultat;
