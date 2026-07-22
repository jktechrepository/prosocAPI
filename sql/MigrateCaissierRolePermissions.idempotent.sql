-- =============================================================================
-- Migration : permissions du rôle « Caissier » (caissier principal)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetCaissierRolePermissionNames()
-- Périmètre Percepteur + supervision guichet (wallets, arriérés, bons, rapports)
--
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCaissierRolePermissions.idempotent.sql
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

DROP TEMPORARY TABLE IF EXISTS tmp_caissier_permission_noms;
CREATE TEMPORARY TABLE tmp_caissier_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_caissier_permission_noms (Nom) VALUES
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
    ('READ_WALLET_AGENT'),
    ('UPDATE_WALLET_AGENT'),
    ('READ_WALLET_MOVEMENT'),
    ('CREATE_WALLET_MOVEMENT'),
    ('READ_ARRIERES_AFFILIE'),
    ('READ_PENALITE_AFFILIE'),
    ('READ_ANTECEDENT'),
    ('READ_BON_ENVOI'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('CONFIRM_DEMANDE_BON_ENVOI'),
    ('GENERATE_RAPPORT'),
    ('EXPORT_DATA'),
    ('READ_STATISTIQUES'),
    ('UPDATE_NOTIFICATION'),
    ('ACCESS_DASHBOARD_CAISSIER');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'ACCESS_DASHBOARD_CAISSIER', 'Accéder au dashboard caissier', 'DASHBOARD_CAISSIER', 'ACCESS', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ACCESS_DASHBOARD_CAISSIER');

SELECT '=== AVANT : permissions du rôle Caissier ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @CaissierRoleId
ORDER BY p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @CaissierRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
INNER JOIN tmp_caissier_permission_noms t ON t.Nom = p.Nom
WHERE p.Statut = 1
  AND @CaissierRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @CaissierRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_caissier_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @CaissierRoleId
  AND @CaissierRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_caissier_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle Caissier ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @CaissierRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_caissier_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_caissier_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @CaissierRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_caissier_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Caissier terminée. Reconnectez les comptes caissier principal.' AS Resultat;
