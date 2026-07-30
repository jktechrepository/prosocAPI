-- =============================================================================
-- Migration : READ_RETRAIT_AGENT (Percepteur, Caissier, Financier, Admin)
-- =============================================================================
-- Claim menu UI « retraitsagent » — distinct de READ_DEMANDE_RETRAIT_AGENT (API).
-- Aligné sur ProsocAPI/Data/SeedData.cs
--   → GetPercepteurRolePermissionNames / GetCaissierRolePermissionNames
--   → GetFinancierRolePermissionNames
--   → MigrateAdminRolePermissionsAsync (Admin reçoit aussi via seed)
-- Idempotent : crée la permission si absente, puis l'attribue aux 4 rôles.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateReadRetraitAgentPermission.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter Percepteur, Caissier, Financier, Admin (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT
    'READ_RETRAIT_AGENT',
    'Accéder au module / menu retraits agent',
    'RETRAIT_AGENT',
    'READ',
    1,
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_RETRAIT_AGENT');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions
    WHERE Nom = 'READ_RETRAIT_AGENT' AND Statut = 1
    LIMIT 1
);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, @PermissionId, NOW()
FROM Roles r
WHERE r.Nom IN ('Percepteur', 'Caissier', 'Financier', 'Admin')
  AND @PermissionId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = @PermissionId
  );

COMMIT;

SELECT '✅ Permission READ_RETRAIT_AGENT migrée pour Percepteur, Caissier, Financier, Admin.' AS Resultat;
