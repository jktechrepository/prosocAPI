-- ═══════════════════════════════════════════════════════════════════════════════
-- 🔧 CORRECTION COMPLÈTE DES PERMISSIONS - AVEC CRÉATION DES PERMISSIONS
-- ═══════════════════════════════════════════════════════════════════════════════
-- Base de données: dev-prosoc_db
-- ═══════════════════════════════════════════════════════════════════════════════

USE `dev-prosoc_db`;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 1️⃣ NETTOYAGE : Supprimer les anciennes RolePermissions
-- ═══════════════════════════════════════════════════════════════════════════════
DELETE FROM RolePermissions;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 2️⃣ VÉRIFIER ET CRÉER LES PERMISSIONS SI ELLES N'EXISTENT PAS
-- ═══════════════════════════════════════════════════════════════════════════════

-- Supprimer les permissions existantes pour recommencer proprement
DELETE FROM Permissions;

-- Créer les 12 permissions de base
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(1, 'users.read', 'Lire les informations des utilisateurs', 'Utilisateurs', 'read', TRUE, NOW()),
(2, 'users.write', 'Créer et modifier les utilisateurs', 'Utilisateurs', 'write', TRUE, NOW()),
(3, 'users.delete', 'Supprimer les utilisateurs', 'Utilisateurs', 'delete', TRUE, NOW()),
(4, 'roles.read', 'Lire les informations des rôles', 'Rôles', 'read', TRUE, NOW()),
(5, 'roles.write', 'Créer et modifier les rôles', 'Rôles', 'write', TRUE, NOW()),
(6, 'roles.delete', 'Supprimer les rôles', 'Rôles', 'delete', TRUE, NOW()),
(7, 'permissions.read', 'Lire les permissions', 'Permissions', 'read', TRUE, NOW()),
(8, 'permissions.write', 'Créer et modifier les permissions', 'Permissions', 'write', TRUE, NOW()),
(9, 'system.admin', 'Administration système complète', 'Système', 'admin', TRUE, NOW()),
(10, 'reports.read', 'Lire les rapports', 'Rapports', 'read', TRUE, NOW()),
(11, 'financial.read', 'Lire les informations financières', 'Financier', 'read', TRUE, NOW()),
(12, 'financial.write', 'Créer et modifier les données financières', 'Financier', 'write', TRUE, NOW());

-- ═══════════════════════════════════════════════════════════════════════════════
-- 3️⃣ SUPER-ADMIN (RoleId = 1) : TOUTES LES 12 PERMISSIONS
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
(1, 9, NOW()),   -- system.admin ⭐
(1, 10, NOW()),  -- reports.read
(1, 11, NOW()),  -- financial.read
(1, 12, NOW());  -- financial.write

-- ═══════════════════════════════════════════════════════════════════════════════
-- 4️⃣ ADMIN (RoleId = 2) : 8 PERMISSIONS (sans delete et system.admin)
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
-- 5️⃣ SUPERVISEUR (RoleId = 3) : 5 PERMISSIONS (lecture uniquement)
-- ═══════════════════════════════════════════════════════════════════════════════
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution) VALUES
(3, 1, NOW()),   -- users.read
(3, 4, NOW()),   -- roles.read
(3, 7, NOW()),   -- permissions.read
(3, 10, NOW()),  -- reports.read
(3, 11, NOW());  -- financial.read

-- ═══════════════════════════════════════════════════════════════════════════════
-- 6️⃣ VÉRIFICATION FINALE
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '✅ PERMISSIONS CRÉÉES' as Resultat;
SELECT IdPermission, Nom, Description, Categorie 
FROM Permissions 
ORDER BY IdPermission;

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

SELECT '✅ DÉTAIL SUPER-ADMIN (12 permissions)' as Resultat;
SELECT 
    rp.IdRolePermission,
    p.Nom as PermissionName,
    p.Description
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 1
ORDER BY p.IdPermission;

SELECT '✅ DÉTAIL ADMIN (8 permissions)' as Resultat;
SELECT 
    rp.IdRolePermission,
    p.Nom as PermissionName,
    p.Description
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2
ORDER BY p.IdPermission;

SELECT '✅ DÉTAIL SUPERVISEUR (5 permissions)' as Resultat;
SELECT 
    rp.IdRolePermission,
    p.Nom as PermissionName,
    p.Description
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 3
ORDER BY p.IdPermission;
