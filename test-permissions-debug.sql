-- Script de diagnostic pour analyser le problème des permissions vides

-- 1. Vérifier l'utilisateur admin@prosoc.cd
SELECT 'UTILISATEUR' as Type, IdUtilisateur, NomUtilisateur, DefaultUsername, Statut 
FROM Utilisateurs 
WHERE NomUtilisateur = 'admin@prosoc.cd' OR DefaultUsername = 'admin@prosoc.cd';

-- 2. Vérifier les UserRoles pour cet utilisateur (IdUtilisateur = 2)
SELECT 'USER_ROLES' as Type, ur.IdUserRole, ur.UtilisateurId, ur.RoleId, ur.IsPrimary, ur.Statut, r.Nom as RoleName
FROM UserRoles ur
LEFT JOIN Roles r ON ur.RoleId = r.IdRole
WHERE ur.UtilisateurId = 2;

-- 3. Vérifier les RolePermissions pour le rôle Admin (RoleId = 2)
SELECT 'ROLE_PERMISSIONS' as Type, rp.IdRolePermission, rp.RoleId, rp.PermissionId, p.Nom as PermissionName, p.Statut
FROM RolePermissions rp
LEFT JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2;

-- 4. Vérifier toutes les permissions disponibles
SELECT 'ALL_PERMISSIONS' as Type, IdPermission, Nom, Description, Statut
FROM Permissions
ORDER BY IdPermission;

-- 5. Vérifier tous les rôles
SELECT 'ALL_ROLES' as Type, IdRole, Nom, Description, Niveau, Statut
FROM Roles
ORDER BY IdRole;

-- 6. Vérifier toutes les associations RolePermissions
SELECT 'ALL_ROLE_PERMISSIONS' as Type, rp.IdRolePermission, r.Nom as RoleName, p.Nom as PermissionName
FROM RolePermissions rp
LEFT JOIN Roles r ON rp.RoleId = r.IdRole
LEFT JOIN Permissions p ON rp.PermissionId = p.IdPermission
ORDER BY rp.RoleId, rp.PermissionId;
