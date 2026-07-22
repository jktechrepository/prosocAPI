-- ═══════════════════════════════════════════════════════════════════════════════
-- 🔧 CORRECTION AUTOMATIQUE DES PERMISSIONS POUR LE RÔLE ADMIN
-- ═══════════════════════════════════════════════════════════════════════════════
-- Base de données: dev-prosoc_db
-- Utilisateur: admin@prosoc.cd (IdUtilisateur = 2)
-- Rôle: Admin (IdRole = 2)
-- ═══════════════════════════════════════════════════════════════════════════════

USE `dev-prosoc_db`;

-- 1️⃣ Supprimer les anciennes permissions du rôle Admin (si elles existent)
DELETE FROM RolePermissions WHERE RoleId = 2;

-- 2️⃣ Ajouter les permissions pour le rôle Admin (RoleId = 2)
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution) VALUES
(2, 1, NOW()),  -- users.read
(2, 2, NOW()),  -- users.write
(2, 4, NOW()),  -- roles.read
(2, 5, NOW()),  -- roles.write
(2, 7, NOW()),  -- permissions.read
(2, 10, NOW()), -- reports.read
(2, 11, NOW()), -- financial.read
(2, 12, NOW()); -- financial.write

-- 3️⃣ Vérification
SELECT '✅ PERMISSIONS AJOUTÉES POUR LE RÔLE ADMIN' as Resultat;
SELECT 
    rp.IdRolePermission,
    r.Nom as RoleName,
    p.Nom as PermissionName,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Roles r ON rp.RoleId = r.IdRole
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2
ORDER BY p.IdPermission;

-- 4️⃣ Vérifier aussi le Super-Admin (RoleId = 1)
SELECT '✅ PERMISSIONS DU SUPER-ADMIN' as Resultat;
SELECT 
    rp.IdRolePermission,
    r.Nom as RoleName,
    p.Nom as PermissionName
FROM RolePermissions rp
INNER JOIN Roles r ON rp.RoleId = r.IdRole
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 1
ORDER BY p.IdPermission;
