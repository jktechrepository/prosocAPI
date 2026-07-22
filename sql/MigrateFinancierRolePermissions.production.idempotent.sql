-- =============================================================================
-- Migration PRODUCTION : permissions du role « Financier »
-- =============================================================================
-- Corrige notamment l'absence de CREATE_ADHESION (et bloc adhesion complet)
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetFinancierRolePermissionNames()
--
-- Idempotent :
--   - cree les permissions catalogue manquantes (si besoin)
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors perimetre
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierRolePermissions.production.idempotent.sql
--
-- IMPORTANT : apres execution, demander aux comptes Financier de se reconnecter
-- (le JWT est regenere a la connexion).
-- =============================================================================

START TRANSACTION;

SET @FinancierRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1
);

SELECT
    CASE
        WHEN @FinancierRoleId IS NULL THEN 'ERREUR : role Financier introuvable'
        ELSE CONCAT('OK : role Financier (IdRole = ', @FinancierRoleId, ')')
    END AS DiagnosticRole;

-- ---------------------------------------------------------------------------
-- 0) S'assurer que le catalogue contient les permissions attendues
-- ---------------------------------------------------------------------------
INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_ADHESION', 'Creer une adhesion', 'ADHESION', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_ADHESION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_ADHESION', 'Modifier une adhesion', 'ADHESION', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_ADHESION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_AFFILIE', 'Modifier un affilie', 'AFFILIE', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_AFFILIE');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_DEPENDANT', 'Voir les dependants', 'DEPENDANT', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_DEPENDANT');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_TYPE_ADHESION', 'Voir les types adhesion', 'TYPE_ADHESION', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_TYPE_ADHESION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_CATEGORIE_ADHESION', 'Voir les categories adhesion', 'CATEGORIE_ADHESION', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_CATEGORIE_ADHESION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_COTISATION_AFFILIE', 'Voir les tarifs cotisation', 'COTISATION_AFFILIE', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_COTISATION_AFFILIE');

-- ---------------------------------------------------------------------------
-- 1) Liste blanche Financier (source SeedData)
-- ---------------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS tmp_financier_permission_noms;
CREATE TEMPORARY TABLE tmp_financier_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_financier_permission_noms (Nom) VALUES
    ('READ_AGENT'),
    ('READ_WALLET_AGENT'),
    ('UPDATE_WALLET_AGENT'),
    ('READ_WALLET_VIRTUEL'),
    ('READ_WALLET_MOVEMENT'),
    ('CREATE_WALLET_MOVEMENT'),
    ('READ_COLLECTE'),
    ('CREATE_COLLECTE'),
    ('READ_TRANSACTION'),
    ('CREATE_TRANSACTION'),
    ('UPDATE_TRANSACTION'),
    ('READ_FRAIS'),
    ('UPDATE_FRAIS'),
    ('READ_DEVISE'),
    ('CREATE_ADHESION'),
    ('READ_ADHESION'),
    ('UPDATE_ADHESION'),
    ('READ_AFFILIE'),
    ('UPDATE_AFFILIE'),
    ('READ_DEPENDANT'),
    ('READ_TYPE_ADHESION'),
    ('READ_CATEGORIE_ADHESION'),
    ('READ_COTISATION_AFFILIE'),
    ('READ_SOUSCRIPTION_PRESTATION'),
    ('READ_ARRIERES_AFFILIE'),
    ('READ_PENALITE_AFFILIE'),
    ('READ_PRESTATION'),
    ('READ_PRODUIT_MUTUEL'),
    ('READ_PRODUIT_ASSUREUR'),
    ('READ_PROVINCE'),
    ('READ_COMMUNE'),
    ('READ_ZONE_SOCIALE'),
    ('READ_CATEGORIE_AGENT'),
    ('READ_HIERARCHIE'),
    ('GENERATE_RAPPORT'),
    ('EXPORT_DATA'),
    ('READ_STATISTIQUES'),
    ('READ_NOTIFICATION');

SELECT '=== AVANT : permissions role Financier ===' AS Section;

SELECT p.Nom
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @FinancierRoleId
ORDER BY p.Nom;

-- Ajout des attributions manquantes
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @FinancierRoleId, p.IdPermission, NOW(6)
FROM Permissions p
INNER JOIN tmp_financier_permission_noms t ON t.Nom = p.Nom
WHERE @FinancierRoleId IS NOT NULL
  AND p.Statut = 1
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @FinancierRoleId AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

-- Retrait hors perimetre
DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_financier_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @FinancierRoleId
  AND @FinancierRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

-- Permissions attendues mais absentes du catalogue
SELECT t.Nom AS PermissionManquanteCatalogue
FROM tmp_financier_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRES : permissions role Financier ===' AS Section;

SELECT p.Nom AS PermissionNom
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @FinancierRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_financier_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_financier_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @FinancierRoleId
  AND p.Statut = 1;

-- Verification explicite CREATE_ADHESION
SELECT
    CASE
        WHEN EXISTS (
            SELECT 1
            FROM RolePermissions rp
            INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
            WHERE rp.RoleId = @FinancierRoleId
              AND p.Nom = 'CREATE_ADHESION'
        ) THEN 'OK : CREATE_ADHESION presente pour Financier'
        ELSE 'ERREUR : CREATE_ADHESION toujours absente pour Financier'
    END AS VerificationCreateAdhesion;

DROP TEMPORARY TABLE IF EXISTS tmp_financier_permission_noms;

COMMIT;

SELECT 'OK : migration permissions Financier terminee. Reconnectez les comptes Financier.' AS Resultat;
