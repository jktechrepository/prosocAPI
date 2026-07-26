-- =============================================================================
-- Migration : UPDATE/DELETE SouscriptionPrestation (rôle Financier)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs → GetFinancierRolePermissionNames()
-- Idempotent : crée les permissions si absentes, les lie au rôle Financier.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierSouscriptionPrestationWrite.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Financier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_SOUSCRIPTION_PRESTATION', 'Modifier une souscription prestation', 'SOUSCRIPTION_PRESTATION', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_SOUSCRIPTION_PRESTATION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'DELETE_SOUSCRIPTION_PRESTATION', 'Supprimer une souscription prestation', 'SOUSCRIPTION_PRESTATION', 'DELETE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'DELETE_SOUSCRIPTION_PRESTATION');

SET @FinancierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @FinancierRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN ('UPDATE_SOUSCRIPTION_PRESTATION', 'DELETE_SOUSCRIPTION_PRESTATION')
  AND p.Statut = 1
  AND @FinancierRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @FinancierRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions UPDATE/DELETE_SOUSCRIPTION_PRESTATION migrées pour Financier.' AS Resultat;
