-- =============================================================================
-- Migration : permissions du rôle « Affilié » (espace membre)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetAffilieRolePermissionNames()
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAffilieRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @AffilieRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Affilié' LIMIT 1
);

SELECT
    CASE
        WHEN @AffilieRoleId IS NULL THEN '❌ ERREUR : rôle « Affilié » introuvable'
        ELSE CONCAT('✅ Rôle Affilié (IdRole = ', @AffilieRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_affilie_permission_noms;
CREATE TEMPORARY TABLE tmp_affilie_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_affilie_permission_noms (Nom) VALUES
    ('UPDATE_AFFILIE'),
    ('READ_DEPENDANT'),
    ('READ_ANTECEDENT'),
    ('READ_PRODUIT_MUTUEL'),
    ('READ_PRODUIT_ASSUREUR'),
    ('READ_PRESTATION'),
    ('READ_TYPE_ADHESION'),
    ('READ_CATEGORIE_ADHESION'),
    ('READ_DEVISE'),
    ('READ_FRAIS'),
    ('READ_PROVINCE'),
    ('READ_COMMUNE'),
    ('READ_COTISATION_AFFILIE'),
    ('READ_ARRIERES_AFFILIE'),
    ('READ_PENALITE_AFFILIE'),
    ('READ_SOUSCRIPTION_PRESTATION'),
    ('READ_COLLECTE'),
    ('CREATE_COLLECTE'),
    ('READ_TRANSACTION'),
    ('PAIEMENT_AFFILIE'),
    ('READ_BON_ENVOI'),
    ('CREATE_DEMANDE_BON_ENVOI'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('READ_JETON_MEDICAL'),
    ('READ_NOTIFICATION'),
    ('ACCESS_DASHBOARD_AFFILIE');

SELECT '=== AVANT : permissions du rôle Affilié ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AffilieRoleId
ORDER BY p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @AffilieRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
INNER JOIN tmp_affilie_permission_noms t ON t.Nom = p.Nom
WHERE p.Statut = 1
  AND @AffilieRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @AffilieRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_affilie_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AffilieRoleId
  AND @AffilieRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

-- Garde-fou explicite : CREATE_AFFILIE ne doit jamais rester attribuée au rôle Affilié.
DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AffilieRoleId
  AND @AffilieRoleId IS NOT NULL
  AND p.Nom = 'CREATE_AFFILIE';

SELECT ROW_COUNT() AS NbCreateAffilieRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_affilie_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle Affilié ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AffilieRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_affilie_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_affilie_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AffilieRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_affilie_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Affilié terminée. Reconnectez les comptes affiliés.' AS Resultat;
