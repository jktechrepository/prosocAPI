-- =============================================================================
-- Migration : READ_DEMANDE_RETRAIT_AGENT (rôle Percepteur — consultation demandes)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetPercepteurRolePermissionNames()
-- Idempotent : crée la permission si absente, puis l'attribue au rôle Percepteur.
-- Lecture seule sur ce script ; VALIDATE est dans MigratePercepteurValidateDemandeRetraitAgent.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePercepteurReadDemandeRetraitAgent.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Percepteur (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_DEMANDE_RETRAIT_AGENT', 'Consulter les demandes de retrait agent', 'DEMANDE_RETRAIT_AGENT', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_DEMANDE_RETRAIT_AGENT');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'READ_DEMANDE_RETRAIT_AGENT' AND Statut = 1 LIMIT 1
);

SET @PercepteurRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Percepteur' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @PercepteurRoleId, @PermissionId, NOW()
WHERE @PercepteurRoleId IS NOT NULL
  AND @PermissionId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @PercepteurRoleId AND rp.PermissionId = @PermissionId
  );

COMMIT;

SELECT '✅ Permission READ_DEMANDE_RETRAIT_AGENT migrée pour Percepteur.' AS Resultat;
