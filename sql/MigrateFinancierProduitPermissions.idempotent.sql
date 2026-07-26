-- =============================================================================
-- Migration : CREATE/UPDATE ProduitAssureur + ProduitMutuel (rôle Financier)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetFinancierRolePermissionNames()
-- Idempotent : lie les 4 permissions au rôle Financier.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierProduitPermissions.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Financier (JWT).
-- =============================================================================

START TRANSACTION;

SET @FinancierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @FinancierRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN (
    'CREATE_PRODUIT_ASSUREUR',
    'UPDATE_PRODUIT_ASSUREUR',
    'CREATE_PRODUIT_MUTUEL',
    'UPDATE_PRODUIT_MUTUEL'
)
  AND p.Statut = 1
  AND @FinancierRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @FinancierRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions CREATE/UPDATE ProduitAssureur et ProduitMutuel migrées pour Financier.' AS Resultat;
