-- =============================================================================
-- Migration : permissions du rôle « IT » (technicien / paramétrage)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetItRolePermissionNames()
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateItRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @ItRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'IT' LIMIT 1
);

SELECT
    CASE
        WHEN @ItRoleId IS NULL THEN '❌ ERREUR : rôle « IT » introuvable'
        ELSE CONCAT('✅ Rôle IT (IdRole = ', @ItRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_it_permission_noms;
CREATE TEMPORARY TABLE tmp_it_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_it_permission_noms (Nom) VALUES
    ('VIEW_LOGS'),
    ('MANAGE_BACKUP'),
    ('READ_USER'),
    ('UPDATE_USER'),
    ('CREATE_AGENT'),
    ('READ_AGENT'),
    ('UPDATE_AGENT'),
    ('CREATE_PROVINCE'),
    ('READ_PROVINCE'),
    ('UPDATE_PROVINCE'),
    ('CREATE_COMMUNE'),
    ('READ_COMMUNE'),
    ('UPDATE_COMMUNE'),
    ('CREATE_ZONE_SOCIALE'),
    ('READ_ZONE_SOCIALE'),
    ('UPDATE_ZONE_SOCIALE'),
    ('CREATE_DEVISE'),
    ('READ_DEVISE'),
    ('UPDATE_DEVISE'),
    ('CREATE_TAUX_CHANGE'),
    ('CREATE_CATEGORIE_AGENT'),
    ('READ_CATEGORIE_AGENT'),
    ('UPDATE_CATEGORIE_AGENT'),
    ('CREATE_CATEGORIE_ADHESION'),
    ('READ_CATEGORIE_ADHESION'),
    ('UPDATE_CATEGORIE_ADHESION'),
    ('CREATE_TYPE_ADHESION'),
    ('READ_TYPE_ADHESION'),
    ('UPDATE_TYPE_ADHESION'),
    ('CREATE_PRODUIT_ASSUREUR'),
    ('READ_PRODUIT_ASSUREUR'),
    ('UPDATE_PRODUIT_ASSUREUR'),
    ('CREATE_PRODUIT_MUTUEL'),
    ('READ_PRODUIT_MUTUEL'),
    ('UPDATE_PRODUIT_MUTUEL'),
    ('CREATE_PRESTATION'),
    ('READ_PRESTATION'),
    ('UPDATE_PRESTATION'),
    ('CREATE_FRAIS'),
    ('READ_FRAIS'),
    ('UPDATE_FRAIS'),
    ('CREATE_ASSUREUR'),
    ('READ_ASSUREUR'),
    ('UPDATE_ASSUREUR'),
    ('CREATE_NOTIFICATION'),
    ('READ_NOTIFICATION'),
    ('UPDATE_NOTIFICATION'),
    ('DELETE_NOTIFICATION'),
    ('READ_COLLECTE'),
    ('READ_ADHESION'),
    ('READ_AFFILIE'),
    ('READ_TRANSACTION'),
    ('READ_SOUSCRIPTION_PRESTATION'),
    ('READ_BON_ENVOI'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('CONFIRM_DEMANDE_BON_ENVOI'),
    ('READ_HIERARCHIE'),
    ('CREATE_DEPENDANT'),
    ('READ_DEPENDANT'),
    ('UPDATE_DEPENDANT'),
    ('CREATE_ANTECEDENT'),
    ('READ_ANTECEDENT'),
    ('UPDATE_ANTECEDENT'),
    ('READ_WALLET_AGENT'),
    ('UPDATE_WALLET_AGENT'),
    ('READ_WALLET_VIRTUEL'),
    ('UPDATE_WALLET_VIRTUEL'),
    ('READ_WALLET_MOVEMENT'),
    ('ACCESS_DASHBOARD_ADMIN'),
    ('GENERATE_RAPPORT'),
    ('EXPORT_DATA');

SELECT '=== AVANT : permissions liées au rôle IT ===' AS Section;

SELECT COUNT(*) AS NbRolePermissionsExistantes
FROM RolePermissions rp
WHERE rp.RoleId = @ItRoleId;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @ItRoleId, p.IdPermission, NOW(6)
FROM Permissions p
INNER JOIN tmp_it_permission_noms t ON t.Nom = p.Nom
WHERE @ItRoleId IS NOT NULL
  AND p.Statut = 1
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @ItRoleId AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_it_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @ItRoleId
  AND @ItRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_it_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle IT ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @ItRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_it_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_it_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @ItRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_it_permission_noms;

COMMIT;

SELECT '✅ Migration permissions IT terminée. Reconnectez les comptes IT.' AS Resultat;
