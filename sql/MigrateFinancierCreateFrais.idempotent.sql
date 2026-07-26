-- =============================================================================
-- Migration : CREATE_FRAIS (rôle Financier)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetFinancierRolePermissionNames()
-- Idempotent : crée la permission si absente, lie au rôle Financier.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierCreateFrais.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Financier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_FRAIS', 'Créer un FRAIS', 'FRAIS', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_FRAIS');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'CREATE_FRAIS' AND Statut = 1 LIMIT 1
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

SELECT '✅ Permission CREATE_FRAIS migrée pour Financier.' AS Resultat;
