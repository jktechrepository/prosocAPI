-- =============================================================================
-- Migration : permissions du rôle « Chef d'équipe »
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetChefEquipeRolePermissionNames()
-- Idempotent :
--   - crée le rôle "Chef d'équipe" si absent
--   - ajoute les RolePermissions manquantes
--   - retire les permissions hors périmètre
-- =============================================================================

START TRANSACTION;

INSERT INTO Roles (Nom, Code, Description, Niveau, Statut, DateCreation)
SELECT "Chef d'équipe", "CE", "Chef d'équipe de zone", 6, 1, NOW(6)
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Nom = "Chef d'équipe"
);

SET @ChefEquipeRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = "Chef d'équipe" LIMIT 1
);

DROP TEMPORARY TABLE IF EXISTS tmp_chef_equipe_permission_noms;
CREATE TEMPORARY TABLE tmp_chef_equipe_permission_noms (
    Nom VARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO tmp_chef_equipe_permission_noms (Nom) VALUES
    ("READ_ADHESION"),
    ("CREATE_ADHESION"),
    ("UPDATE_ADHESION"),
    ("READ_AFFILIE"),
    ("UPDATE_AFFILIE"),
    ("READ_COLLECTE"),
    ("CREATE_COLLECTE"),
    ("READ_WALLET_AGENT"),
    ("UPDATE_WALLET_AGENT"),
    ("READ_WALLET_VIRTUEL"),
    ("READ_WALLET_MOVEMENT"),
    ("CREATE_WALLET_MOVEMENT"),
    ("READ_TRANSACTION"),
    ("CREATE_TRANSACTION"),
    ("ACCESS_DASHBOARD_AGENT"),
    ("READ_DEMANDE_BON_ENVOI"),
    ("CREATE_DEMANDE_BON_ENVOI"),
    ("CONFIRM_DEMANDE_BON_ENVOI"),
    ("SCAN_BON_ENVOI"),
    ("READ_BON_ENVOI"),
    ("READ_ZONE_SOCIALE"),
    ("READ_COMMUNE"),
    ("READ_PROVINCE"),
    ("READ_CATEGORIE_AGENT"),
    ("READ_COTISATION_AFFILIE"),
    ("READ_NOTIFICATION"),
    ("ACCESS_DASHBOARD_CHEF_EQUIPE"),
    ("READ_EQUIPE_ZONE"),
    ("READ_EQUIPE_WALLET_MOVEMENT"),
    ("READ_EQUIPE_COLLECTE");

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @ChefEquipeRoleId, p.IdPermission, NOW(6)
FROM Permissions p
INNER JOIN tmp_chef_equipe_permission_noms t ON t.Nom = p.Nom
LEFT JOIN RolePermissions rp
    ON rp.RoleId = @ChefEquipeRoleId
   AND rp.PermissionId = p.IdPermission
WHERE p.Statut = 1
  AND rp.IdRolePermission IS NULL;

DELETE rp
FROM RolePermissions rp
LEFT JOIN Permissions p ON p.IdPermission = rp.PermissionId
LEFT JOIN tmp_chef_equipe_permission_noms t ON t.Nom = p.Nom
WHERE rp.RoleId = @ChefEquipeRoleId
  AND t.Nom IS NULL;

DROP TEMPORARY TABLE IF EXISTS tmp_chef_equipe_permission_noms;

COMMIT;
