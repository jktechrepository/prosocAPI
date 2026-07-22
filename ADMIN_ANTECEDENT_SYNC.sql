-- =====================================================
-- SYNCHRONISATION SPÉCIFIQUE : Permissions ANTECEDENT pour Admin
-- =====================================================
-- Script pour s'assurer que le rôle Admin a bien toutes les permissions ANTECEDENT

-- ÉTAPE 1: Vérifier l'état actuel des permissions Admin
SELECT '=== VÉRIFICATION PERMISSIONS ADMIN ANTECEDENT ===' as info;
SELECT 
    r.Nom as Role, 
    p.Nom as Permission, 
    rp.DateAttribution
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE r.Nom = 'Admin' 
AND p.Nom LIKE '%ANTECEDENT%'
ORDER BY p.Nom;

-- ÉTAPE 2: Forcer l'attribution des permissions ANTECEDENT à l'Admin
INSERT IGNORE INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT 
    r.IdRole, 
    p.IdPermission, 
    NOW()
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Nom = 'Admin' 
AND p.Nom IN ('CREATE_ANTECEDENT', 'READ_ANTECEDENT', 'UPDATE_ANTECEDENT', 'DELETE_ANTECEDENT');

-- ÉTAPE 3: Vérification finale
SELECT '=== VÉRIFICATION FINALE ===' as info;
SELECT 
    COUNT(*) as total_anthecedent_permissions,
    COUNT(DISTINCT p.Nom) as unique_permissions
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE r.Nom = 'Admin' 
AND p.Nom LIKE '%ANTECEDENT%';

-- ÉTAPE 4: Résumé complet des permissions Admin
SELECT '=== RÉSUMÉ COMPLET PERMISSIONS ADMIN ===' as info;
SELECT 
    r.Nom as Role,
    COUNT(*) as total_permissions,
    COUNT(CASE WHEN p.Nom LIKE '%DEPENDANT%' THEN 1 END) as dependant_permissions,
    COUNT(CASE WHEN p.Nom LIKE '%ASSUREUR%' THEN 1 END) as assureur_permissions,
    COUNT(CASE WHEN p.Nom LIKE '%ANTECEDENT%' THEN 1 END) as antecedent_permissions
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE r.Nom = 'Admin'
GROUP BY r.Nom;
