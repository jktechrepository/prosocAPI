-- =============================================================================
-- Migration : OPEN / CLOSE / READ_CAISSIER_SESSION (Percepteur, Caissier)
-- =============================================================================
-- Aligné sur SeedData GetPercepteurRolePermissionNames / GetCaissierRolePermissionNames.
-- Idempotent : crée les permissions si absentes, puis les lie aux 2 rôles.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCaisseSessionPercepteurCaissier.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter Percepteur et Caissier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'OPEN_CAISSIER_SESSION', 'Ouvrir une session de caisse', 'CAISSIER_SESSION', 'OPEN', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'OPEN_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CLOSE_CAISSIER_SESSION', 'Clôturer une session de caisse', 'CAISSIER_SESSION', 'CLOSE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CLOSE_CAISSIER_SESSION');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_CAISSIER_SESSION', 'Consulter session et mouvements de caisse', 'CAISSIER_SESSION', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_CAISSIER_SESSION');

SET @OpenId := (SELECT IdPermission FROM Permissions WHERE Nom = 'OPEN_CAISSIER_SESSION' AND Statut = 1 LIMIT 1);
SET @CloseId := (SELECT IdPermission FROM Permissions WHERE Nom = 'CLOSE_CAISSIER_SESSION' AND Statut = 1 LIMIT 1);
SET @ReadId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_CAISSIER_SESSION' AND Statut = 1 LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN (
    SELECT @OpenId AS IdPermission
    UNION ALL SELECT @CloseId
    UNION ALL SELECT @ReadId
) p
WHERE r.Nom IN ('Percepteur', 'Caissier')
  AND p.IdPermission IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions OPEN/CLOSE/READ_CAISSIER_SESSION migrées pour Percepteur et Caissier.' AS Resultat;
