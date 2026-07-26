-- =============================================================================
-- Migration : UPDATE_ADHESION + UPDATE_AFFILIE (rôle Caissier)
-- =============================================================================
-- Aligné sur SeedData.GetCaissierRolePermissionNames() (héritage Percepteur).
-- Idempotent : crée les permissions si absentes, les lie au rôle Caissier.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCaissierUpdateAdhesionAffilie.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Caissier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_ADHESION', 'Modifier une adhésion', 'ADHESION', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_ADHESION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_AFFILIE', 'Modifier un affilié', 'AFFILIE', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_AFFILIE');

SET @CaissierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Caissier' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @CaissierRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN ('UPDATE_ADHESION', 'UPDATE_AFFILIE')
  AND p.Statut = 1
  AND @CaissierRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @CaissierRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions UPDATE_ADHESION et UPDATE_AFFILIE migrées pour Caissier.' AS Resultat;
