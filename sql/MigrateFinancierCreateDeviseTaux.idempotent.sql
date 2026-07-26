-- =============================================================================
-- Migration : CREATE_DEVISE + CREATE_TAUX_CHANGE (rôle Financier)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetFinancierRolePermissionNames()
-- Idempotent : crée CREATE_TAUX_CHANGE si absente, lie les deux permissions au Financier.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierCreateDeviseTaux.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Financier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_TAUX_CHANGE', 'Créer un taux de change', 'TAUX_CHANGE', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_TAUX_CHANGE');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_DEVISE', 'Créer une devise', 'DEVISE', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_DEVISE');

SET @FinancierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @FinancierRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN ('CREATE_DEVISE', 'CREATE_TAUX_CHANGE')
  AND p.Statut = 1
  AND @FinancierRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @FinancierRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions CREATE_DEVISE et CREATE_TAUX_CHANGE migrées pour Financier.' AS Resultat;
