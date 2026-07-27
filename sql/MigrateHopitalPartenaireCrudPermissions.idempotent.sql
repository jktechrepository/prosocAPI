-- =============================================================================
-- Migration : CREATE / READ / UPDATE_HOPITAL_PARTENAIRE (Admin, IT)
-- =============================================================================
-- Aligné sur ProsocAPI/Data/SeedData.cs (catalogue + GetItRolePermissionNames).
-- Idempotent : crée les permissions si absentes, puis les lie aux rôles Admin et IT.
-- Pas de DELETE_HOPITAL_PARTENAIRE dans cette phase.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateHopitalPartenaireCrudPermissions.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Admin et IT (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_HOPITAL_PARTENAIRE', 'Créer un hôpital partenaire', 'HOPITAL_PARTENAIRE', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_HOPITAL_PARTENAIRE');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_HOPITAL_PARTENAIRE', 'Consulter les hôpitaux partenaires', 'HOPITAL_PARTENAIRE', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_HOPITAL_PARTENAIRE');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_HOPITAL_PARTENAIRE', 'Modifier un hôpital partenaire', 'HOPITAL_PARTENAIRE', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_HOPITAL_PARTENAIRE');

SET @CreateId := (SELECT IdPermission FROM Permissions WHERE Nom = 'CREATE_HOPITAL_PARTENAIRE' AND Statut = 1 LIMIT 1);
SET @ReadId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_HOPITAL_PARTENAIRE' AND Statut = 1 LIMIT 1);
SET @UpdateId := (SELECT IdPermission FROM Permissions WHERE Nom = 'UPDATE_HOPITAL_PARTENAIRE' AND Statut = 1 LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN (
    SELECT @CreateId AS IdPermission
    UNION ALL SELECT @ReadId
    UNION ALL SELECT @UpdateId
) p
WHERE r.Nom IN ('Admin', 'IT')
  AND p.IdPermission IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions CREATE/READ/UPDATE_HOPITAL_PARTENAIRE migrées pour Admin et IT.' AS Resultat;
