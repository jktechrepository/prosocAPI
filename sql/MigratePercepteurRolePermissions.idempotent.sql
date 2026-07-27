-- =============================================================================
-- Migration : permissions du rôle « Percepteur » (guichet terrain)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetPercepteurRolePermissionNames()
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePercepteurRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @PercepteurRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Percepteur' LIMIT 1
);

SELECT
    CASE
        WHEN @PercepteurRoleId IS NULL THEN '❌ ERREUR : rôle « Percepteur » introuvable'
        ELSE CONCAT('✅ Rôle Percepteur (IdRole = ', @PercepteurRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_percepteur_permission_noms;
CREATE TEMPORARY TABLE tmp_percepteur_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_percepteur_permission_noms (Nom) VALUES
    ('CREATE_ADHESION'),
    ('READ_ADHESION'),
    ('UPDATE_ADHESION'),
    ('READ_AFFILIE'),
    ('UPDATE_AFFILIE'),
    ('CREATE_COLLECTE'),
    ('READ_COLLECTE'),
    ('CREATE_TRANSACTION'),
    ('READ_TRANSACTION'),
    ('UPDATE_TRANSACTION'),
    ('READ_FRAIS'),
    ('READ_DEVISE'),
    ('READ_PRESTATION'),
    ('READ_PRODUIT_MUTUEL'),
    ('READ_PRODUIT_ASSUREUR'),
    ('READ_SOUSCRIPTION_PRESTATION'),
    ('READ_TYPE_ADHESION'),
    ('READ_CATEGORIE_ADHESION'),
    ('READ_COTISATION_AFFILIE'),
    ('READ_AGENT'),
    ('READ_CATEGORIE_AGENT'),
    ('READ_DEPENDANT'),
    ('READ_PROVINCE'),
    ('READ_COMMUNE'),
    ('READ_ZONE_SOCIALE'),
    ('READ_NOTIFICATION'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('READ_BON_ENVOI'),
    ('CONFIRM_DEMANDE_BON_ENVOI'),
    ('READ_PERCEPTION_VIRTUAL'),
    ('CONFIRM_PERCEPTION_VIRTUAL'),
    ('OPEN_CAISSIER_SESSION'),
    ('CLOSE_CAISSIER_SESSION'),
    ('READ_CAISSIER_SESSION'),
    ('READ_DEMANDE_RETRAIT_AGENT'),
    ('VALIDATE_DEMANDE_RETRAIT_AGENT'),
    ('CONFIRM_RETRAIT_AGENT');

SELECT '=== AVANT : permissions du rôle Percepteur ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @PercepteurRoleId
ORDER BY p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @PercepteurRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
INNER JOIN tmp_percepteur_permission_noms t ON t.Nom = p.Nom
WHERE p.Statut = 1
  AND @PercepteurRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @PercepteurRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_percepteur_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @PercepteurRoleId
  AND @PercepteurRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_percepteur_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle Percepteur ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @PercepteurRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_percepteur_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_percepteur_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @PercepteurRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_percepteur_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Percepteur terminée. Reconnectez les comptes percepteur.' AS Resultat;
