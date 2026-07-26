-- =============================================================================
-- Migration : retirer UPDATE_AFFILIE du rôle « Financier »
-- =============================================================================
-- Retire uniquement l'attribution RolePermissions Financier ↔ UPDATE_AFFILIE.
-- Aligné sur SeedData.GetFinancierRolePermissionNames().
-- Idempotent : un second run ne retire rien (ROW_COUNT = 0).
--
-- Pour un alignement complet catalogue (ajoute manquant + purge tout surplus) :
--   sql/MigrateFinancierRolePermissions.idempotent.sql
--   (ou sql/MigrateFinancierRolePermissions.production.idempotent.sql)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveFinancierUpdateAffilie.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Financier (JWT).
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @FinancierRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = 'Financier' LIMIT 1
);

SET @UpdateAffiliePermissionId := (
    SELECT IdPermission FROM Permissions WHERE Nom = 'UPDATE_AFFILIE' LIMIT 1
);

SELECT
    CASE
        WHEN @FinancierRoleId IS NULL THEN '❌ ERREUR : rôle « Financier » introuvable'
        ELSE CONCAT('✅ Rôle Financier (IdRole = ', @FinancierRoleId, ')')
    END AS DiagnosticRole;

SELECT '=== AVANT : attribution Financier UPDATE_AFFILIE ===' AS Section;

SELECT
    p.Nom AS PermissionNom,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @FinancierRoleId
  AND p.Nom = 'UPDATE_AFFILIE';

DELETE FROM RolePermissions
WHERE RoleId = @FinancierRoleId
  AND PermissionId = @UpdateAffiliePermissionId
  AND @FinancierRoleId IS NOT NULL
  AND @UpdateAffiliePermissionId IS NOT NULL;

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : attribution Financier UPDATE_AFFILIE (attendu : 0) ===' AS Section;

SELECT COUNT(*) AS NbAttributionsRestantes
FROM RolePermissions rp
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE rp.RoleId = @FinancierRoleId
  AND p.Nom = 'UPDATE_AFFILIE';

COMMIT;

SELECT '✅ Migration Financier (retrait UPDATE_AFFILIE) terminée. Reconnectez les comptes Financier.' AS Resultat;
