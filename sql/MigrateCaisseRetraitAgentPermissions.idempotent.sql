-- =============================================================================
-- Migration : permissions caisse + retrait agent (rôle Caissier)
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCaisseRetraitAgentPermissions.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'OPEN_CAISSIER_SESSION', 'Ouvrir une session de caisse', 'CAISSE', 'OPEN', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'OPEN_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CLOSE_CAISSIER_SESSION', 'Clôturer une session de caisse', 'CAISSE', 'CLOSE', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CLOSE_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_CAISSIER_SESSION', 'Consulter session et mouvements de caisse', 'CAISSE', 'READ', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CONFIRM_RETRAIT_AGENT', 'Payer un retrait agent au guichet', 'RETRAIT', 'CONFIRM', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CONFIRM_RETRAIT_AGENT');

SET @CaissierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Caissier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @CaissierRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN (
    'OPEN_CAISSIER_SESSION',
    'CLOSE_CAISSIER_SESSION',
    'READ_CAISSIER_SESSION',
    'CONFIRM_RETRAIT_AGENT'
)
AND @CaissierRoleId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @CaissierRoleId AND rp.PermissionId = p.IdPermission
);

COMMIT;
