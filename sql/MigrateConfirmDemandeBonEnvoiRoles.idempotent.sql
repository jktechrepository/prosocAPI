-- =============================================================================
-- Ajouter permissions workflow bon d'envoi aux rôles Percepteur et IT
-- =============================================================================
-- Cible :
--   - Percepteur : READ_DEMANDE_BON_ENVOI, READ_BON_ENVOI, CONFIRM_DEMANDE_BON_ENVOI
--   - IT         : CONFIRM_DEMANDE_BON_ENVOI (READ_* déjà attendues)
--   - Caissier   : inchangé (déjà couvert par MigrateCaissierRolePermissions)
--
-- Idempotent :
--   - crée la permission si absente
--   - ajoute uniquement les RolePermissions manquantes
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateConfirmDemandeBonEnvoiRoles.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CONFIRM_DEMANDE_BON_ENVOI', 'Confirmer ou rejeter une demande de bon d''envoi', 'DEMANDE_BON_ENVOI', 'CONFIRM', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CONFIRM_DEMANDE_BON_ENVOI');

SET @PercepteurRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Percepteur' LIMIT 1);
SET @ItRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'IT' LIMIT 1);

DROP TEMPORARY TABLE IF EXISTS tmp_confirm_bon_role_permissions;
CREATE TEMPORARY TABLE tmp_confirm_bon_role_permissions (
    RoleNom VARCHAR(100) NOT NULL,
    PermissionNom VARCHAR(100) NOT NULL,
    PRIMARY KEY (RoleNom, PermissionNom)
);

INSERT INTO tmp_confirm_bon_role_permissions (RoleNom, PermissionNom) VALUES
    ('Percepteur', 'READ_DEMANDE_BON_ENVOI'),
    ('Percepteur', 'READ_BON_ENVOI'),
    ('Percepteur', 'CONFIRM_DEMANDE_BON_ENVOI'),
    ('IT', 'CONFIRM_DEMANDE_BON_ENVOI');

SELECT '=== AVANT : attributions ciblées ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom,
    COUNT(*) AS NbAttributions
FROM tmp_confirm_bon_role_permissions t
INNER JOIN Roles r ON r.Nom = t.RoleNom
INNER JOIN Permissions p ON p.Nom = t.PermissionNom
LEFT JOIN RolePermissions rp ON rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
GROUP BY r.Nom, p.Nom
ORDER BY r.Nom, p.Nom;

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT
    r.IdRole,
    p.IdPermission,
    NOW()
FROM tmp_confirm_bon_role_permissions t
INNER JOIN Roles r ON r.Nom = t.RoleNom
INNER JOIN Permissions p ON p.Nom = t.PermissionNom AND p.Statut = 1
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
);

SELECT ROW_COUNT() AS NbAttributionsAjoutees;

SELECT '=== APRÈS : attributions ciblées ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom,
    rp.DateAttribution
FROM tmp_confirm_bon_role_permissions t
INNER JOIN Roles r ON r.Nom = t.RoleNom
INNER JOIN Permissions p ON p.Nom = t.PermissionNom
INNER JOIN RolePermissions rp ON rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
ORDER BY r.Nom, p.Nom;

SELECT
    t.RoleNom,
    t.PermissionNom AS PermissionManquante
FROM tmp_confirm_bon_role_permissions t
LEFT JOIN Roles r ON r.Nom = t.RoleNom
LEFT JOIN Permissions p ON p.Nom = t.PermissionNom AND p.Statut = 1
LEFT JOIN RolePermissions rp ON rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
WHERE r.IdRole IS NULL
   OR p.IdPermission IS NULL
   OR rp.IdRolePermission IS NULL
ORDER BY t.RoleNom, t.PermissionNom;

DROP TEMPORARY TABLE IF EXISTS tmp_confirm_bon_role_permissions;

COMMIT;

SELECT '✅ Permissions bon d''envoi ajoutées pour Percepteur et IT. Reconnectez les utilisateurs concernés.' AS Resultat;
