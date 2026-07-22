-- =============================================================================
-- Migration : permissions perception compte virtuel (rôle Percepteur)
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePerceptionVirtuellePermissions.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_PERCEPTION_VIRTUAL', 'Consulter les collectes compte virtuel à percevoir', 'PERCEPTION', 'READ', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_PERCEPTION_VIRTUAL');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CONFIRM_PERCEPTION_VIRTUAL', 'Confirmer la perception physique des collectes compte virtuel', 'PERCEPTION', 'CONFIRM', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CONFIRM_PERCEPTION_VIRTUAL');

SET @PercepteurRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Percepteur' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @PercepteurRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN ('READ_PERCEPTION_VIRTUAL', 'CONFIRM_PERCEPTION_VIRTUAL')
AND @PercepteurRoleId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @PercepteurRoleId AND rp.PermissionId = p.IdPermission
);

COMMIT;
