-- ═══════════════════════════════════════════════════════════════════════════════
-- 🔧 CORRECTION COMPLÈTE DES PERMISSIONS POUR TOUS LES RÔLES
-- ═══════════════════════════════════════════════════════════════════════════════
-- Base de données: dev-prosoc_db
-- Basé sur le seed data de ProsocDbContext.cs
-- ═══════════════════════════════════════════════════════════════════════════════

USE `dev-prosoc_db`;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 1️⃣ NETTOYAGE : Supprimer toutes les anciennes RolePermissions
-- ═══════════════════════════════════════════════════════════════════════════════
DELETE FROM RolePermissions;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 2️⃣ SUPER-ADMIN (RoleId = 1) : TOUTES LES 12 PERMISSIONS
-- ═══════════════════════════════════════════════════════════════════════════════
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution) VALUES
(1, 1, NOW()),   -- users.read
(1, 2, NOW()),   -- users.write
(1, 3, NOW()),   -- users.delete
(1, 4, NOW()),   -- roles.read
(1, 5, NOW()),   -- roles.write
(1, 6, NOW()),   -- roles.delete
(1, 7, NOW()),   -- permissions.read
(1, 8, NOW()),   -- permissions.write
(1, 9, NOW()),   -- system.admin ⭐ (EXCLUSIF AU SUPER-ADMIN)
(1, 10, NOW()),  -- reports.read
(1, 11, NOW()),  -- financial.read
(1, 12, NOW());  -- financial.write

-- ═══════════════════════════════════════════════════════════════════════════════
-- 3️⃣ ADMIN (RoleId = 2) : 8 PERMISSIONS (sans delete et system.admin)
-- ═══════════════════════════════════════════════════════════════════════════════
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution) VALUES
(2, 1, NOW()),   -- users.read
(2, 2, NOW()),   -- users.write
(2, 4, NOW()),   -- roles.read
(2, 5, NOW()),   -- roles.write
(2, 7, NOW()),   -- permissions.read
(2, 10, NOW()),  -- reports.read
(2, 11, NOW()),  -- financial.read
(2, 12, NOW());  -- financial.write

-- ═══════════════════════════════════════════════════════════════════════════════
-- 4️⃣ SUPERVISEUR (RoleId = 3) : 5 PERMISSIONS (lecture uniquement)
-- ═══════════════════════════════════════════════════════════════════════════════
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution) VALUES
(3, 1, NOW()),   -- users.read
(3, 4, NOW()),   -- roles.read
(3, 7, NOW()),   -- permissions.read
(3, 10, NOW()),  -- reports.read
(3, 11, NOW());  -- financial.read

-- ═══════════════════════════════════════════════════════════════════════════════
-- 5️⃣ VÉRIFICATION FINALE
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '✅ RÉSUMÉ DES PERMISSIONS PAR RÔLE' as Resultat;

SELECT 
    r.Nom as RoleName,
    COUNT(rp.IdRolePermission) as NombrePermissions,
    GROUP_CONCAT(p.Nom ORDER BY p.IdPermission SEPARATOR ', ') as Permissions
FROM Roles r
LEFT JOIN RolePermissions rp ON r.IdRole = rp.RoleId
LEFT JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE r.IdRole IN (1, 2, 3)
GROUP BY r.IdRole, r.Nom
ORDER BY r.IdRole;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 6️⃣ DÉTAIL DES PERMISSIONS SUPER-ADMIN
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '✅ DÉTAIL SUPER-ADMIN (12 permissions)' as Resultat;

SELECT 
    rp.IdRolePermission,
    p.Nom as PermissionName,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 1
ORDER BY p.IdPermission;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 7️⃣ DÉTAIL DES PERMISSIONS ADMIN
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '✅ DÉTAIL ADMIN (8 permissions)' as Resultat;

SELECT 
    rp.IdRolePermission,
    p.Nom as PermissionName,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2
ORDER BY p.IdPermission;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 8️⃣ DÉTAIL DES PERMISSIONS SUPERVISEUR
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '✅ DÉTAIL SUPERVISEUR (5 permissions)' as Resultat;

SELECT 
    rp.IdRolePermission,
    p.Nom as PermissionName,
    p.Description,
    rp.DateAttribution
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 3
ORDER BY p.IdPermission;
