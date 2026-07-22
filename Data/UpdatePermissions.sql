-- Script de mise à jour des permissions DEPENDANT et ASSUREUR
-- Exécuter ce script pour synchroniser les permissions manquantes

-- Étape 1: Insérer les nouvelles permissions si elles n'existent pas
INSERT IGNORE INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
VALUES 
    ('CREATE_DEPENDANT', 'Créer un dépendant', 'DEPENDANT', 'CREATE', 1, NOW()),
    ('READ_DEPENDANT', 'Voir les dépendants', 'DEPENDANT', 'READ', 1, NOW()),
    ('UPDATE_DEPENDANT', 'Modifier un dépendant', 'DEPENDANT', 'UPDATE', 1, NOW()),
    ('DELETE_DEPENDANT', 'Supprimer un dépendant', 'DEPENDANT', 'DELETE', 1, NOW()),
    ('CREATE_ASSUREUR', 'Créer un assureur', 'ASSUREUR', 'CREATE', 1, NOW()),
    ('READ_ASSUREUR', 'Voir les assureurs', 'ASSUREUR', 'READ', 1, NOW()),
    ('UPDATE_ASSUREUR', 'Modifier un assureur', 'ASSUREUR', 'UPDATE', 1, NOW()),
    ('DELETE_ASSUREUR', 'Supprimer un assureur', 'ASSUREUR', 'DELETE', 1, NOW());

-- Étape 2: Attribuer les nouvelles permissions aux rôles existants
-- Rôle IT
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom IN ('IT') 
AND p.Nom IN ('CREATE_DEPENDANT', 'READ_DEPENDANT', 'UPDATE_DEPENDANT', 
              'CREATE_ASSUREUR', 'READ_ASSUREUR', 'UPDATE_ASSUREUR');

-- Rôle Superviseur : ne pas attribuer CREATE/READ/UPDATE_ASSUREUR ni DEPENDANT write
-- (liste blanche SeedData.GetSuperviseurRolePermissionNames)

-- Rôles Agent (AT) et Agent (AA)
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom IN ('Agent (AT)', 'Agent (AA)') 
AND p.Nom IN ('CREATE_DEPENDANT', 'READ_DEPENDANT', 'UPDATE_DEPENDANT', 
              'CREATE_ASSUREUR', 'READ_ASSUREUR', 'UPDATE_ASSUREUR');

-- Étape 3: Vérification
SELECT r.Nom as Role, p.Nom as Permission, rp.DateAttribution
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE p.Nom LIKE '%DEPENDANT%' OR p.Nom LIKE '%ASSUREUR%'
ORDER BY r.Nom, p.Nom;
