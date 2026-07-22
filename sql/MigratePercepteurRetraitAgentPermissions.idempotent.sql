-- =============================================================================
-- Migration : permissions retrait agent + session caisse (rôle Percepteur)
-- =============================================================================
-- Corrige le 403 sur POST /api/RetraitAgent/utiliser-jeton pour le rôle Percepteur
-- (le contrôleur exige le rôle JWT ; ces permissions alimentent le menu frontend).
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePercepteurRetraitAgentPermissions.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'OPEN_CAISSIER_SESSION', 'Ouvrir une session de caisse', 'CAISSE', 'OPEN', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'OPEN_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_CAISSIER_SESSION', 'Consulter session et mouvements de caisse', 'CAISSE', 'READ', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CONFIRM_RETRAIT_AGENT', 'Payer un retrait agent au guichet', 'RETRAIT_AGENT', 'CONFIRM', 1, NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CONFIRM_RETRAIT_AGENT');

SET @PercepteurRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Percepteur' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @PercepteurRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN ('OPEN_CAISSIER_SESSION', 'READ_CAISSIER_SESSION', 'CONFIRM_RETRAIT_AGENT')
  AND @PercepteurRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @PercepteurRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions retrait agent + session caisse attribuées au rôle Percepteur.' AS Resultat;
