-- =============================================================================
-- Migration : permissions du rôle « Agent (AA) » — liste blanche stricte
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetAgentAaRolePermissionNames()
-- Périmètre : Agent (AT) + encodeur niveau 2 (dépendants, antécédents, lecture assureurs)
--
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre (réduction ~92 → 43)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAgentAaRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @AaRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Agent (AA)' LIMIT 1
);

SELECT
    CASE
        WHEN @AaRoleId IS NULL THEN '❌ ERREUR : rôle « Agent (AA) » introuvable'
        ELSE CONCAT('✅ Rôle Agent (AA) (IdRole = ', @AaRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_aa_permission_noms;
CREATE TEMPORARY TABLE tmp_aa_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_aa_permission_noms (Nom) VALUES
    -- Périmètre Agent (AT)
    ('CREATE_ADHESION'),
    ('READ_ADHESION'),
    ('UPDATE_ADHESION'),
    ('READ_AFFILIE'),
    ('UPDATE_AFFILIE'),
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
    ('ACCESS_DASHBOARD_AGENT_AA'),
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
    -- Spécifique encodeur niveau 2
    ('CREATE_DEPENDANT'),
    ('READ_DEPENDANT'),
    ('UPDATE_DEPENDANT'),
    ('CREATE_ANTECEDENT'),
    ('READ_ANTECEDENT'),
    ('UPDATE_ANTECEDENT'),
    ('READ_ASSUREUR');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'ACCESS_DASHBOARD_AGENT_AA', 'Accéder au dashboard agent administratif (encodeur)', 'DASHBOARD_AGENT_AA', 'ACCESS', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ACCESS_DASHBOARD_AGENT_AA');

SELECT '=== AVANT : permissions liées au rôle Agent (AA) ===' AS Section;

SELECT COUNT(*) AS NbRolePermissionsExistantes
FROM RolePermissions rp
WHERE rp.RoleId = @AaRoleId;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @AaRoleId, p.IdPermission, NOW(6)
FROM Permissions p
INNER JOIN tmp_aa_permission_noms t ON t.Nom = p.Nom
WHERE @AaRoleId IS NOT NULL
  AND p.Statut = 1
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @AaRoleId AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_aa_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AaRoleId
  AND @AaRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT t.Nom AS PermissionManquante
FROM tmp_aa_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

SELECT '=== APRÈS : permissions du rôle Agent (AA) ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AaRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_aa_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_aa_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AaRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_aa_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Agent (AA) terminée. Reconnectez les comptes AA.' AS Resultat;
