-- =============================================================================
-- Migration : permissions READ/UPDATE_PARAMETRES_METIER (Admin, IT)
-- =============================================================================
-- Idempotent : crée les permissions si absentes, puis les attribue aux rôles.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateParametresMetierPermissions.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_PARAMETRES_METIER', 'Consulter les paramètres métier', 'PARAMETRES_METIER', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_PARAMETRES_METIER');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_PARAMETRES_METIER', 'Modifier les paramètres métier', 'PARAMETRES_METIER', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_PARAMETRES_METIER');

SET @ReadId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_PARAMETRES_METIER' AND Statut = 1 LIMIT 1);
SET @UpdateId := (SELECT IdPermission FROM Permissions WHERE Nom = 'UPDATE_PARAMETRES_METIER' AND Statut = 1 LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN (
    SELECT @ReadId AS IdPermission
    UNION ALL SELECT @UpdateId
) p
WHERE r.Nom IN ('Admin', 'IT')
  AND p.IdPermission IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions READ/UPDATE_PARAMETRES_METIER migrées pour Admin et IT.' AS Resultat;
