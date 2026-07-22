-- =============================================================================
-- Migration : permission READ_STATISTIQUES (Admin, Financier, Caissier)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs
-- Idempotent : crée la permission si absente, puis l'attribue aux rôles cibles.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateReadStatistiquesPermission.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_STATISTIQUES', 'Consulter les statistiques', 'STATISTIQUES', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_STATISTIQUES');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'READ_STATISTIQUES' AND Statut = 1 LIMIT 1
);

SET @AdminRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Admin' LIMIT 1);
SET @FinancierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1);
SET @CaissierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Caissier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, @PermissionId, NOW()
FROM Roles r
WHERE r.Nom IN ('Admin', 'Financier', 'Caissier')
  AND @PermissionId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = @PermissionId
  );

COMMIT;

SELECT '✅ Permission READ_STATISTIQUES migrée pour Admin, Financier et Caissier.' AS Resultat;
