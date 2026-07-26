-- =============================================================================
-- Migration : CREATE/READ/VALIDATE DemandeRetraitAgent
-- =============================================================================
-- Aligné sur SeedData :
--   - Caissier     : CREATE + READ + VALIDATE
--   - Agent (AT)   : CREATE + READ  (parcours agent)
--   - Superviseur  : VALIDATE       (+ CREATE/READ via héritage AT au seed)
--
-- CONFIRM_RETRAIT_AGENT (paiement jeton) reste distinct et inchangé.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCaissierDemandeRetraitAgentPermissions.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes concernés (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'CREATE_DEMANDE_RETRAIT_AGENT', 'Créer une demande de retrait agent', 'DEMANDE_RETRAIT_AGENT', 'CREATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'CREATE_DEMANDE_RETRAIT_AGENT');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_DEMANDE_RETRAIT_AGENT', 'Consulter les demandes de retrait agent', 'DEMANDE_RETRAIT_AGENT', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_DEMANDE_RETRAIT_AGENT');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'VALIDATE_DEMANDE_RETRAIT_AGENT', 'Valider une demande de retrait agent et générer le jeton', 'DEMANDE_RETRAIT_AGENT', 'VALIDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'VALIDATE_DEMANDE_RETRAIT_AGENT');

SET @CaissierRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Caissier' LIMIT 1);
SET @AgentAtRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Agent (AT)' LIMIT 1);
SET @SuperviseurRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Superviseur' LIMIT 1);

-- Caissier : create + read + validate
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @CaissierRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN (
    'CREATE_DEMANDE_RETRAIT_AGENT',
    'READ_DEMANDE_RETRAIT_AGENT',
    'VALIDATE_DEMANDE_RETRAIT_AGENT'
)
  AND p.Statut = 1
  AND @CaissierRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @CaissierRoleId AND rp.PermissionId = p.IdPermission
  );

-- Agent (AT) : create + read
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @AgentAtRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN (
    'CREATE_DEMANDE_RETRAIT_AGENT',
    'READ_DEMANDE_RETRAIT_AGENT'
)
  AND p.Statut = 1
  AND @AgentAtRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @AgentAtRoleId AND rp.PermissionId = p.IdPermission
  );

-- Superviseur : validate (+ create/read pour UI validation / file d'attente)
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @SuperviseurRoleId, p.IdPermission, NOW()
FROM Permissions p
WHERE p.Nom IN (
    'CREATE_DEMANDE_RETRAIT_AGENT',
    'READ_DEMANDE_RETRAIT_AGENT',
    'VALIDATE_DEMANDE_RETRAIT_AGENT'
)
  AND p.Statut = 1
  AND @SuperviseurRoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @SuperviseurRoleId AND rp.PermissionId = p.IdPermission
  );

COMMIT;

SELECT '✅ Permissions CREATE/READ/VALIDATE_DEMANDE_RETRAIT_AGENT migrées (Caissier, Agent AT, Superviseur).' AS Resultat;
