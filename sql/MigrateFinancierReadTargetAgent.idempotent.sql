-- =============================================================================
-- Migration : permission READ_TARGET_AGENT (Financier — lecture TargetAgent)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs
-- Idempotent : crée la permission si absente, puis l'attribue au rôle Financier.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierReadTargetAgent.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_TARGET_AGENT', 'Voir les objectifs / TargetAgent', 'TARGET_AGENT', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_TARGET_AGENT');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'READ_TARGET_AGENT' AND Statut = 1 LIMIT 1
);

SET @FinancierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @FinancierRoleId, @PermissionId, NOW()
WHERE @FinancierRoleId IS NOT NULL
  AND @PermissionId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @FinancierRoleId AND rp.PermissionId = @PermissionId
  );

COMMIT;

SELECT '✅ Permission READ_TARGET_AGENT migrée pour Financier.' AS Resultat;
