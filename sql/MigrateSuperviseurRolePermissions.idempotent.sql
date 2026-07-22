-- =============================================================================
-- Migration : permissions du rôle « Superviseur »
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetSuperviseurRolePermissionNames()
-- Périmètre : Agent (AT) + supervision (targets, performances, wallet virtuel agents)
-- Hors périmètre : UPDATE_ADHESION, UPDATE_AFFILIE,
--                  CREATE_ASSUREUR, READ_ASSUREUR, UPDATE_ASSUREUR,
--                  CREATE_PRODUIT_ASSUREUR
--
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateSuperviseurRolePermissions.idempotent.sql
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

DROP TEMPORARY TABLE IF EXISTS tmp_superviseur_permission_noms;
CREATE TEMPORARY TABLE tmp_superviseur_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_superviseur_permission_noms (Nom) VALUES
    -- Périmètre Agent (AT) — sans UPDATE_ADHESION / UPDATE_AFFILIE
    ('CREATE_ADHESION'),
    ('READ_ADHESION'),
    ('READ_AFFILIE'),
    ('READ_DEPENDANT'),
    ('CREATE_COLLECTE'),
    ('READ_COLLECTE'),
    ('READ_FRAIS'),
    ('READ_DEVISE'),
    ('READ_PRESTATION'),
    ('READ_PRODUIT_MUTUEL'),
    ('READ_PRODUIT_ASSUREUR'),
    ('READ_SOUSCRIPTION_PRESTATION'),
    ('READ_TYPE_ADHESION'),
    ('READ_CATEGORIE_ADHESION'),
    ('READ_WALLET_AGENT'),
    ('UPDATE_WALLET_AGENT'),
    ('READ_WALLET_VIRTUEL'),
    ('READ_WALLET_MOVEMENT'),
    ('CREATE_WALLET_MOVEMENT'),
    ('READ_TRANSACTION'),
    ('CREATE_TRANSACTION'),
    ('ACCESS_DASHBOARD_AGENT'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('CREATE_DEMANDE_BON_ENVOI'),
    ('CONFIRM_DEMANDE_BON_ENVOI'),
    ('SCAN_BON_ENVOI'),
    ('READ_BON_ENVOI'),
    ('READ_ZONE_SOCIALE'),
    ('READ_COMMUNE'),
    ('READ_PROVINCE'),
    ('READ_CATEGORIE_AGENT'),
    ('READ_COTISATION_AFFILIE'),
    ('READ_NOTIFICATION'),
    -- Supervision équipe
    ('READ_AGENT'),
    ('UPDATE_AGENT'),
    ('READ_HIERARCHIE'),
    ('MANAGE_SUPERVISION'),
    ('MANAGE_OBJECTIFS'),
    ('VALIDATE_PERFORMANCE'),
    ('ACCESS_DASHBOARD_SUPERVISEUR'),
    ('UPDATE_WALLET_VIRTUEL'),
    ('GENERATE_RAPPORT'),
    ('EXPORT_DATA');

SELECT '=== AVANT : permissions liées au rôle Superviseur ===' AS Section;

SELECT COUNT(*) AS NbRolePermissionsExistantes
FROM RolePermissions rp
WHERE rp.RoleId = @SuperviseurRoleId;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @SuperviseurRoleId, p.IdPermission, NOW(6)
FROM Permissions p
INNER JOIN tmp_superviseur_permission_noms t ON t.Nom = p.Nom
WHERE @SuperviseurRoleId IS NOT NULL
  AND p.Statut = 1
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @SuperviseurRoleId AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_superviseur_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @SuperviseurRoleId
  AND @SuperviseurRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_superviseur_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle Superviseur ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @SuperviseurRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_superviseur_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_superviseur_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @SuperviseurRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_superviseur_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Superviseur terminée. Reconnectez les comptes Superviseur.' AS Resultat;
