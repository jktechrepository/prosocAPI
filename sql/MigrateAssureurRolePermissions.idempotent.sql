-- =============================================================================
-- Migration : permissions du rôle « Assureur » (partenaire)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetAssureurRolePermissionNames()
-- Idempotent :
--   - crée le rôle s'il est absent
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre (ex. ancien wildcard READ_*)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAssureurRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

INSERT INTO Roles (Nom, Code, Description, Niveau, Statut, DateCreation)
SELECT 'Assureur', 'AS', 'Partenaire assureur', 10, 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE Nom = 'Assureur');

SET @AssureurRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Assureur' LIMIT 1
);

SELECT
    CASE
        WHEN @AssureurRoleId IS NULL THEN '❌ ERREUR : rôle « Assureur » introuvable'
        ELSE CONCAT('✅ Rôle Assureur (IdRole = ', @AssureurRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_assureur_permission_noms;
CREATE TEMPORARY TABLE tmp_assureur_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_assureur_permission_noms (Nom) VALUES
    ('READ_ASSUREUR'),
    ('READ_PRODUIT_ASSUREUR'),
    ('READ_PRESTATION'),
    ('READ_SOUSCRIPTION_PRESTATION'),
    ('READ_COLLECTE'),
    ('READ_TRANSACTION'),
    ('READ_AFFILIE'),
    ('READ_ADHESION'),
    ('READ_DEPENDANT'),
    ('READ_ANTECEDENT'),
    ('READ_BON_ENVOI'),
    ('READ_DEMANDE_BON_ENVOI'),
    ('READ_JETON_MEDICAL'),
    ('READ_DEVISE'),
    ('READ_FRAIS'),
    ('READ_TYPE_ADHESION'),
    ('READ_CATEGORIE_ADHESION'),
    ('READ_PROVINCE'),
    ('READ_COMMUNE'),
    ('GENERATE_RAPPORT'),
    ('EXPORT_DATA'),
    ('READ_NOTIFICATION'),
    ('ACCESS_DASHBOARD_ASSUREUR');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'ACCESS_DASHBOARD_ASSUREUR', 'Accéder au dashboard assureur', 'DASHBOARD_ASSUREUR', 'ACCESS', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ACCESS_DASHBOARD_ASSUREUR');

SELECT '=== AVANT : permissions du rôle Assureur ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AssureurRoleId
ORDER BY p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    @AssureurRoleId,
    p.IdPermission,
    NOW()
FROM Permissions p
INNER JOIN tmp_assureur_permission_noms t ON t.Nom = p.Nom
WHERE p.Statut = 1
  AND @AssureurRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM RolePermissions rp
      WHERE rp.RoleId = @AssureurRoleId
        AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_assureur_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AssureurRoleId
  AND @AssureurRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_assureur_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle Assureur ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AssureurRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_assureur_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_assureur_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AssureurRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_assureur_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Assureur terminée. Reconnectez les comptes partenaires.' AS Resultat;
