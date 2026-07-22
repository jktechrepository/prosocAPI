-- SCRIPT DE FIX RAPIDE DES PERMISSIONS DEPENDANT/ASSUREUR
-- À exécuter directement sur la base de données MySQL

-- Étape 1: Vérifier si les permissions existent déjà
SELECT COUNT(*) as permissions_existantes 
FROM Permissions 
WHERE Nom LIKE '%DEPENDANT%' OR Nom LIKE '%ASSUREUR%';

-- Étape 2: Insérer les permissions manquantes (si elles n'existent pas)
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

-- Étape 3: Attribuer les permissions aux rôles cibles
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom IN ('IT', 'Superviseur', 'Agent (AT)', 'Agent (AA)') 
AND p.Nom IN ('CREATE_DEPENDANT', 'READ_DEPENDANT', 'UPDATE_DEPENDANT', 'DELETE_DEPENDANT',
              'CREATE_ASSUREUR', 'READ_ASSUREUR', 'UPDATE_ASSUREUR', 'DELETE_ASSUREUR');

-- Étape 4: Vérification finale
SELECT r.Nom as Role, p.Nom as Permission, rp.DateAttribution
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE p.Nom LIKE '%DEPENDANT%' OR p.Nom LIKE '%ASSUREUR%'
ORDER BY r.Nom, p.Nom;
