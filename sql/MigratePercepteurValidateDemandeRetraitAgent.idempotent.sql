-- =============================================================================
-- Migration : VALIDATE_DEMANDE_RETRAIT_AGENT (rôle Percepteur)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetPercepteurRolePermissionNames()
-- Idempotent : crée la permission si absente, puis l'attribue au rôle Percepteur.
-- Percepteur : READ + VALIDATE + CONFIRM (pas de CREATE).
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePercepteurValidateDemandeRetraitAgent.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Percepteur (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'VALIDATE_DEMANDE_RETRAIT_AGENT', 'Valider une demande de retrait agent et générer le jeton', 'DEMANDE_RETRAIT_AGENT', 'VALIDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'VALIDATE_DEMANDE_RETRAIT_AGENT');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'VALIDATE_DEMANDE_RETRAIT_AGENT' AND Statut = 1 LIMIT 1
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

SELECT '✅ Permission VALIDATE_DEMANDE_RETRAIT_AGENT migrée pour Percepteur.' AS Resultat;
