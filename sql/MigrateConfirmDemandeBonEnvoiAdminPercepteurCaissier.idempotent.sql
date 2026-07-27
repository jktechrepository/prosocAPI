-- =============================================================================
-- Migration : CONFIRM_DEMANDE_BON_ENVOI (Admin, Percepteur, Caissier)
-- =============================================================================
-- Aligné sur SeedData GetPercepteurRolePermissionNames / GetCaissierRolePermissionNames.
-- Idempotent : crée les permissions si absentes, puis les lie aux 3 rôles.
-- Couvre POST /api/DemandeBonEnvoi/valider-et-generer et POST .../{id}/confirmer.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateConfirmDemandeBonEnvoiAdminPercepteurCaissier.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter Admin, Percepteur, Caissier (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_DEMANDE_BON_ENVOI', 'Consulter ses demandes de bon d''envoi', 'DEMANDE_BON_ENVOI', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_DEMANDE_BON_ENVOI');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_BON_ENVOI', 'Voir les bons d''envoi', 'BON_ENVOI', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_BON_ENVOI');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CONFIRM_DEMANDE_BON_ENVOI', 'Confirmer ou rejeter une demande de bon d''envoi', 'DEMANDE_BON_ENVOI', 'CONFIRM', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CONFIRM_DEMANDE_BON_ENVOI');

SET @ReadDemandeId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_DEMANDE_BON_ENVOI' AND Statut = 1 LIMIT 1);
SET @ReadBonId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_BON_ENVOI' AND Statut = 1 LIMIT 1);
SET @ConfirmId := (SELECT IdPermission FROM Permissions WHERE Nom = 'CONFIRM_DEMANDE_BON_ENVOI' AND Statut = 1 LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN (
    SELECT @ReadDemandeId AS IdPermission
    UNION ALL SELECT @ReadBonId
    UNION ALL SELECT @ConfirmId
) p
WHERE r.Nom IN ('Admin', 'Percepteur', 'Caissier')
  AND p.IdPermission IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions READ/CONFIRM DemandeBonEnvoi migrées pour Admin, Percepteur, Caissier.' AS Resultat;
