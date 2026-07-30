-- =============================================================================
-- Migration : MARQUER_PAYER_RETRAIT_AGENT (Percepteur, Caissier, Financier)
-- =============================================================================
-- Endpoint : POST /api/RetraitAgent/marquer-paye
-- Aligné sur ProsocAPI/Data/SeedData.cs
--   → GetPercepteurRolePermissionNames / GetCaissierRolePermissionNames
--   → GetFinancierRolePermissionNames
-- Idempotent : crée la permission si absente, puis l'attribue aux 3 rôles.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateMarquerPayerRetraitAgentPermission.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter Percepteur, Caissier, Financier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT
    'MARQUER_PAYER_RETRAIT_AGENT',
    'Marquer un retrait agent comme payé (jeton)',
    'PAYER_RETRAIT_AGENT',
    'MARQUER',
    1,
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'MARQUER_PAYER_RETRAIT_AGENT');

SET @PermissionId := (
    SELECT IdPermission FROM Permissions
    WHERE Nom = 'MARQUER_PAYER_RETRAIT_AGENT' AND Statut = 1
    LIMIT 1
);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, @PermissionId, NOW()
FROM Roles r
WHERE r.Nom IN ('Percepteur', 'Caissier', 'Financier')
  AND @PermissionId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = @PermissionId
  );

COMMIT;

SELECT '✅ Permission MARQUER_PAYER_RETRAIT_AGENT migrée pour Percepteur, Caissier, Financier.' AS Resultat;
