-- =============================================================================
-- Migration : retirer UPDATE_WALLET_VIRTUEL du rôle « Financier »
-- =============================================================================
-- Retire uniquement l'attribution RolePermissions Financier ↔ UPDATE_WALLET_VIRTUEL.
-- Aligné sur SeedData.GetFinancierRolePermissionNames() (permission absente).
-- Idempotent : un second run ne retire rien (ROW_COUNT = 0).
--
-- L'API exige désormais UPDATE_WALLET_VIRTUEL pour créditer un wallet virtuel
-- (ajouter-solde, ajustements, solde initial > 0).
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveFinancierUpdateWalletVirtuel.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Financier (JWT).
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @FinancierRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1
);

SET @UpdateWalletVirtuelPermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'UPDATE_WALLET_VIRTUEL' LIMIT 1
);

SELECT
    CASE
        WHEN @FinancierRoleId IS NULL THEN '❌ ERREUR : rôle « Financier » introuvable'
        ELSE CONCAT('✅ Rôle Financier (IdRole = ', @FinancierRoleId, ')')
    END AS DiagnosticRole;

SELECT '=== AVANT : attribution Financier UPDATE_WALLET_VIRTUEL ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @FinancierRoleId
  AND p.Nom = 'UPDATE_WALLET_VIRTUEL';

DELETE FROM RolePermissions
WHERE RoleId = @FinancierRoleId
  AND PermissionId = @UpdateWalletVirtuelPermissionId
  AND @FinancierRoleId IS NOT NULL
  AND @UpdateWalletVirtuelPermissionId IS NOT NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : attribution Financier UPDATE_WALLET_VIRTUEL (attendu : 0) ===' AS Section;

SELECT COUNT(*) AS NbAttributionsRestantes
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @FinancierRoleId
  AND p.Nom = 'UPDATE_WALLET_VIRTUEL';

COMMIT;

SELECT '✅ Migration Financier (retrait UPDATE_WALLET_VIRTUEL) terminée. Reconnectez les comptes Financier.' AS Resultat;
