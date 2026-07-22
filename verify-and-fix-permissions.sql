-- ═══════════════════════════════════════════════════════════════════════════════
-- 🔍 SCRIPT DE VÉRIFICATION ET CORRECTION DES PERMISSIONS
-- ═══════════════════════════════════════════════════════════════════════════════

-- 1️⃣ Vérifier l'utilisateur admin@prosoc.cd
SELECT '1. UTILISATEUR ADMIN' as Etape;
SELECT IdUtilisateur, NomUtilisateur, DefaultUsername, Statut 
FROM Utilisateurs 
WHERE NomUtilisateur = 'admin@prosoc.cd' OR DefaultUsername = 'admin@prosoc.cd';

-- 2️⃣ Vérifier les UserRoles pour cet utilisateur
SELECT '2. USER_ROLES' as Etape;
SELECT ur.IdUserRole, ur.UtilisateurId, ur.RoleId, ur.IsPrimary, ur.Statut, r.Nom as RoleName
FROM UserRoles ur
LEFT JOIN Roles r ON ur.RoleId = r.IdRole
WHERE ur.UtilisateurId = 2;

-- 3️⃣ Vérifier TOUTES les permissions disponibles
SELECT '3. TOUTES LES PERMISSIONS' as Etape;
SELECT IdPermission, Nom, Description, Statut
FROM Permissions
ORDER BY IdPermission;

-- 4️⃣ Vérifier les RolePermissions pour le rôle Admin (RoleId = 2)
SELECT '4. ROLE_PERMISSIONS POUR ADMIN (RoleId=2)' as Etape;
SELECT rp.IdRolePermission, rp.RoleId, rp.PermissionId, p.Nom as PermissionName, p.Statut
FROM RolePermissions rp
LEFT JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2;

-- 5️⃣ Vérifier TOUTES les RolePermissions
SELECT '5. TOUTES LES ROLE_PERMISSIONS' as Etape;
SELECT rp.IdRolePermission, r.Nom as RoleName, p.Nom as PermissionName
FROM RolePermissions rp
LEFT JOIN Roles r ON rp.RoleId = r.IdRole
LEFT JOIN Permissions p ON rp.PermissionId = p.IdPermission
ORDER BY rp.RoleId, rp.PermissionId;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 🔧 CORRECTION : Ajouter les permissions manquantes pour le rôle Admin
-- ═══════════════════════════════════════════════════════════════════════════════

-- Si les RolePermissions sont vides, les ajouter :
SELECT '6. CORRECTION - Ajout des permissions pour Admin' as Etape;

-- Vérifier si les permissions existent déjà
SELECT COUNT(*) as 'Permissions_Existantes_Pour_Admin' 
FROM RolePermissions 
WHERE RoleId = 2;

-- Si le count est 0, exécuter les INSERT suivants :
-- (Décommenter les lignes ci-dessous si nécessaire)

/*
-- Admin (RoleId = 2) : permissions de gestion sauf système complet
INSERT INTO RolePermissions (IdRolePermission, RoleId, PermissionId, DateAttribution) VALUES
(13, 2, 1, NOW()),  -- users.read
(14, 2, 2, NOW()),  -- users.write
(15, 2, 4, NOW()),  -- roles.read
(16, 2, 5, NOW()),  -- roles.write
(17, 2, 7, NOW()),  -- permissions.read
(18, 2, 10, NOW()), -- reports.read
(19, 2, 11, NOW()), -- financial.read
(20, 2, 12, NOW()); -- financial.write
*/

-- 7️⃣ Vérification finale après correction
SELECT '7. VERIFICATION FINALE' as Etape;
SELECT rp.IdRolePermission, rp.RoleId, rp.PermissionId, p.Nom as PermissionName
FROM RolePermissions rp
LEFT JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2
ORDER BY rp.PermissionId;
