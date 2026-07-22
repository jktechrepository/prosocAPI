-- =============================================================================
-- Migration : retrait CREATE_DEPENDANT pour Agent (AT), Chef d'équipe et Superviseur
-- =============================================================================
-- Idempotent : supprime uniquement la liaison RolePermissions CREATE_DEPENDANT
-- pour les rôles terrain / supervision (création réservée à AA, IT, Affilié).
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateRemoveAgentAtChefEquipeCreateDependant.idempotent.sql
-- =============================================================================

START TRANSACTION;

SELECT '=== AVANT : CREATE_DEPENDANT sur AT / Chef d''équipe / Superviseur ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.RoleId
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE p.Nom = 'CREATE_DEPENDANT'
  AND r.Nom IN ('Agent (AT)', 'Chef d''équipe', 'Superviseur')
ORDER BY r.Nom;

DELETE rp
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.RoleId
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE p.Nom = 'CREATE_DEPENDANT'
  AND r.Nom IN ('Agent (AT)', 'Chef d''équipe', 'Superviseur');

SELECT ROW_COUNT() AS NbAttributionsRetirees;

SELECT '=== APRÈS : CREATE_DEPENDANT sur AT / Chef d''équipe / Superviseur (doit être vide) ===' AS Section;

SELECT
    r.Nom AS RoleNom,
    p.Nom AS PermissionNom
FROM RolePermissions rp
INNER JOIN Roles r ON r.IdRole = rp.RoleId
INNER JOIN Permissions p ON p.IdPermission = rp.PermissionId
WHERE p.Nom = 'CREATE_DEPENDANT'
  AND r.Nom IN ('Agent (AT)', 'Chef d''équipe', 'Superviseur');

COMMIT;

SELECT '✅ Migration terminée. Reconnectez les comptes AT, Chef d''équipe et Superviseur.' AS Resultat;
