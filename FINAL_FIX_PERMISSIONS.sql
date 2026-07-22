-- =====================================================
-- SOLUTION FINALE : FIX DES PERMISSIONS DEPENDANT/ASSUREUR/ANTECEDENT
-- =====================================================
-- Exécuter ce script directement sur votre base de données MySQL

-- ÉTAPE 1: Vérifier l'état actuel
SELECT '=== VÉRIFICATION INITIALE ===' as info;
SELECT COUNT(*) as permissions_existantes 
FROM Permissions 
WHERE Nom LIKE '%DEPENDANT%' OR Nom LIKE '%ASSUREUR%' OR Nom LIKE '%ANTECEDENT%';

-- ÉTAPE 2: Insérer les permissions manquantes
INSERT IGNORE INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
VALUES 
    -- Permissions Dépendant
    ('CREATE_DEPENDANT', 'Créer un dépendant', 'DEPENDANT', 'CREATE', 1, NOW()),
    ('READ_DEPENDANT', 'Voir les dépendants', 'DEPENDANT', 'READ', 1, NOW()),
    ('UPDATE_DEPENDANT', 'Modifier un dépendant', 'DEPENDANT', 'UPDATE', 1, NOW()),
    ('DELETE_DEPENDANT', 'Supprimer un dépendant', 'DEPENDANT', 'DELETE', 1, NOW()),
    
    -- Permissions Assureur
    ('CREATE_ASSUREUR', 'Créer un assureur', 'ASSUREUR', 'CREATE', 1, NOW()),
    ('READ_ASSUREUR', 'Voir les assureurs', 'ASSUREUR', 'READ', 1, NOW()),
    ('UPDATE_ASSUREUR', 'Modifier un assureur', 'ASSUREUR', 'UPDATE', 1, NOW()),
    ('DELETE_ASSUREUR', 'Supprimer un assureur', 'ASSUREUR', 'DELETE', 1, NOW()),
    
    -- Permissions Antécédent
    ('CREATE_ANTECEDENT', 'Créer un antécédent', 'ANTECEDENT', 'CREATE', 1, NOW()),
    ('READ_ANTECEDENT', 'Voir les antécédents', 'ANTECEDENT', 'READ', 1, NOW()),
    ('UPDATE_ANTECEDENT', 'Modifier un antécédent', 'ANTECEDENT', 'UPDATE', 1, NOW()),
    ('DELETE_ANTECEDENT', 'Supprimer un antécédent', 'ANTECEDENT', 'DELETE', 1, NOW());

-- ÉTAPE 3: Attribuer les permissions aux rôles cibles
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom IN ('IT', 'Superviseur', 'Agent (AT)', 'Agent (AA)') 
AND p.Nom IN ('CREATE_DEPENDANT', 'READ_DEPENDANT', 'UPDATE_DEPENDANT', 'DELETE_DEPENDANT',
              'CREATE_ASSUREUR', 'READ_ASSUREUR', 'UPDATE_ASSUREUR', 'DELETE_ASSUREUR',
              'CREATE_ANTECEDENT', 'READ_ANTECEDENT', 'UPDATE_ANTECEDENT', 'DELETE_ANTECEDENT');

-- ÉTAPE 4: Vérification finale
SELECT '=== VÉRIFICATION FINALE ===' as info;
SELECT r.Nom as Role, p.Nom as Permission, rp.DateAttribution
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE p.Nom LIKE '%DEPENDANT%' OR p.Nom LIKE '%ASSUREUR%' OR p.Nom LIKE '%ANTECEDENT%'
ORDER BY r.Nom, p.Nom;

-- ÉTAPE 5: Comptage final
SELECT '=== RÉSUMÉ ===' as info;
SELECT 
    COUNT(*) as total_permissions,
    COUNT(DISTINCT r.Nom) as roles_affected,
    COUNT(DISTINCT p.Nom) as unique_permissions,
    COUNT(DISTINCT p.Categorie) as categories_covered
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE p.Nom LIKE '%DEPENDANT%' OR p.Nom LIKE '%ASSUREUR%' OR p.Nom LIKE '%ANTECEDENT%';
