-- =============================================================================
-- Migration : permissions DemandeRechargeWalletVirtuel (Superviseur + Admin)
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateDemandeRechargeWalletVirtuelPermissions.idempotent.sql
-- Après déploiement : reconnexion JWT des Superviseurs / Admin.
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL',
       'Créer une demande de recharge wallet virtuel',
       'DEMANDE_RECHARGE_WALLET_VIRTUEL', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_DEMANDE_RECHARGE_WALLET_VIRTUEL',
       'Consulter les demandes de recharge wallet virtuel',
       'DEMANDE_RECHARGE_WALLET_VIRTUEL', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_DEMANDE_RECHARGE_WALLET_VIRTUEL');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL',
       'Confirmer ou rejeter une demande de recharge wallet virtuel',
       'DEMANDE_RECHARGE_WALLET_VIRTUEL', 'CONFIRM', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL');

SET @CreateId := (SELECT IdPermission FROM Permissions WHERE Nom = 'CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL' AND Statut = 1 LIMIT 1);
SET @ReadId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_DEMANDE_RECHARGE_WALLET_VIRTUEL' AND Statut = 1 LIMIT 1);
SET @ConfirmId := (SELECT IdPermission FROM Permissions WHERE Nom = 'CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL' AND Statut = 1 LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN (
    SELECT @CreateId AS IdPermission
    UNION ALL SELECT @ReadId
    UNION ALL SELECT @ConfirmId
) p
WHERE r.Nom IN ('Admin', 'Superviseur')
  AND p.IdPermission IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions DemandeRechargeWalletVirtuel migrées pour Admin et Superviseur.' AS Resultat;
