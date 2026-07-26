-- =============================================================================
-- Migration : permissions du rôle « Agent (AT) » — liste blanche stricte
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetAgentAtRolePermissionNames()
-- Idempotent :
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre (réduction ~92 → 36)
--
-- Prérequis :
--   - MySQL / MariaDB (base ProsocAPI, schéma EF Core)
--   - Rôle « Agent (AT) » présent dans la table Roles
--
-- Recommandation PRODUCTION :
--   1. Sauvegarder la base avant exécution
--   2. Exécuter ce script
--   3. Demander aux agents AT de se reconnecter (JWT)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAgentAtRolePermissions.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @AtRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Agent (AT)' LIMIT 1
);

SELECT
    CASE
        WHEN @AtRoleId IS NULL THEN '❌ ERREUR : rôle « Agent (AT) » introuvable'
        ELSE CONCAT('✅ Rôle Agent (AT) (IdRole = ', @AtRoleId, ')')
    END AS DiagnosticRole;

DROP TEMPORARY TABLE IF EXISTS tmp_at_permission_noms;
CREATE TEMPORARY TABLE tmp_at_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_at_permission_noms (Nom) VALUES
    ('CREATE_ADHESION'),
    ('READ_ADHESION'),
    ('UPDATE_ADHESION'),
    ('READ_AFFILIE'),
    ('UPDATE_AFFILIE'),
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
    ('CREATE_DEMANDE_RETRAIT_AGENT'),
    ('READ_DEMANDE_RETRAIT_AGENT'),
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
    ('READ_NOTIFICATION');

-- État AVANT
SELECT '=== AVANT : permissions liées au rôle Agent (AT) ===' AS Section;

SELECT COUNT(*) AS NbRolePermissionsExistantes
FROM RolePermissions rp
WHERE rp.RoleId = @AtRoleId;

-- Ajout des permissions manquantes
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @AtRoleId, p.IdPermission, NOW(6)
FROM Permissions p
INNER JOIN tmp_at_permission_noms t ON t.Nom = p.Nom
WHERE @AtRoleId IS NOT NULL
  AND p.Statut = 1
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @AtRoleId AND rp.PermissionId = p.IdPermission
  );

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

-- Retrait des permissions hors périmètre
DELETE rp FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_at_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AtRoleId
  AND @AtRoleId IS NOT NULL
  AND t.Nom IS NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

-- Permissions attendues mais absentes du catalogue
SELECT t.Nom AS PermissionManquante
FROM tmp_at_permission_noms t
LEFT JOIN Permissions p ON p.Nom = t.Nom AND p.Statut = 1
WHERE p.IdPermission IS NULL
ORDER BY t.Nom;

-- État APRÈS
SELECT '=== APRÈS : permissions du rôle Agent (AT) ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @AtRoleId
ORDER BY p.Nom;

SELECT
    COUNT(*) AS NbRolePermissionsTotal,
    (SELECT COUNT(*) FROM tmp_at_permission_noms) AS NbAttendues
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
INNER JOIN tmp_at_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @AtRoleId
  AND p.Statut = 1;

DROP TEMPORARY TABLE IF EXISTS tmp_at_permission_noms;

COMMIT;

SELECT '✅ Migration permissions Agent (AT) terminée. Reconnectez les comptes AT.' AS Resultat;
