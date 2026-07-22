-- ═══════════════════════════════════════════════════════════════════════════════
-- 🔧 CRÉATION COMPLÈTE DES 81 PERMISSIONS + ATTRIBUTION AUX RÔLES
-- ═══════════════════════════════════════════════════════════════════════════════
-- Base de données: dev-prosoc_db
-- Basé sur PERMISSIONS_COMPLETES_PROPOSITION.md
-- ═══════════════════════════════════════════════════════════════════════════════

USE `dev-prosoc_db`;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 1️⃣ NETTOYAGE
-- ═══════════════════════════════════════════════════════════════════════════════
DELETE FROM RolePermissions;
DELETE FROM UserPermissions;
DELETE FROM Permissions;

-- ═══════════════════════════════════════════════════════════════════════════════
-- 2️⃣ CRÉATION DES 81 PERMISSIONS
-- ═══════════════════════════════════════════════════════════════════════════════

-- MODULE AUTHENTIFICATION & UTILISATEURS (12 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(1, 'users.read', 'Consulter les utilisateurs', 'Authentification', 'read', TRUE, NOW()),
(2, 'users.write', 'Créer/modifier les utilisateurs', 'Authentification', 'write', TRUE, NOW()),
(3, 'users.delete', 'Supprimer les utilisateurs', 'Authentification', 'delete', TRUE, NOW()),
(4, 'roles.read', 'Consulter les rôles', 'Authentification', 'read', TRUE, NOW()),
(5, 'roles.write', 'Créer/modifier les rôles', 'Authentification', 'write', TRUE, NOW()),
(6, 'roles.delete', 'Supprimer les rôles', 'Authentification', 'delete', TRUE, NOW()),
(7, 'permissions.read', 'Consulter les permissions', 'Authentification', 'read', TRUE, NOW()),
(8, 'permissions.write', 'Créer/modifier les permissions', 'Authentification', 'write', TRUE, NOW()),
(9, 'system.admin', 'Administration système complète', 'Authentification', 'admin', TRUE, NOW()),
(10, 'devices.read', 'Consulter les appareils connectés', 'Authentification', 'read', TRUE, NOW()),
(11, 'devices.write', 'Gérer les appareils', 'Authentification', 'write', TRUE, NOW()),
(12, 'devices.delete', 'Supprimer les appareils', 'Authentification', 'delete', TRUE, NOW());

-- MODULE AGENTS (9 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(13, 'agents.read', 'Consulter les agents', 'Agents', 'read', TRUE, NOW()),
(14, 'agents.write', 'Créer/modifier les agents', 'Agents', 'write', TRUE, NOW()),
(15, 'agents.delete', 'Supprimer les agents', 'Agents', 'delete', TRUE, NOW()),
(16, 'agents.wallet.read', 'Consulter les wallets agents', 'Agents', 'read', TRUE, NOW()),
(17, 'agents.wallet.write', 'Gérer les wallets agents', 'Agents', 'write', TRUE, NOW()),
(18, 'agents.targets.read', 'Consulter les objectifs agents', 'Agents', 'read', TRUE, NOW()),
(19, 'agents.targets.write', 'Gérer les objectifs agents', 'Agents', 'write', TRUE, NOW()),
(20, 'agents.retraits.read', 'Consulter les retraits agents', 'Agents', 'read', TRUE, NOW()),
(21, 'agents.retraits.write', 'Gérer les retraits agents', 'Agents', 'write', TRUE, NOW());

-- MODULE AFFILIÉS & ADHÉSIONS (12 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(22, 'affilies.read', 'Consulter les affiliés', 'Affiliés', 'read', TRUE, NOW()),
(23, 'affilies.write', 'Créer/modifier les affiliés', 'Affiliés', 'write', TRUE, NOW()),
(24, 'affilies.delete', 'Supprimer les affiliés', 'Affiliés', 'delete', TRUE, NOW()),
(25, 'adhesions.read', 'Consulter les adhésions', 'Adhésions', 'read', TRUE, NOW()),
(26, 'adhesions.write', 'Créer/modifier les adhésions', 'Adhésions', 'write', TRUE, NOW()),
(27, 'adhesions.delete', 'Supprimer les adhésions', 'Adhésions', 'delete', TRUE, NOW()),
(28, 'dependants.read', 'Consulter les dépendants', 'Affiliés', 'read', TRUE, NOW()),
(29, 'dependants.write', 'Créer/modifier les dépendants', 'Affiliés', 'write', TRUE, NOW()),
(30, 'dependants.delete', 'Supprimer les dépendants', 'Affiliés', 'delete', TRUE, NOW()),
(31, 'type-adhesions.read', 'Consulter les types d\'adhésion', 'Adhésions', 'read', TRUE, NOW()),
(32, 'type-adhesions.write', 'Créer/modifier les types d\'adhésion', 'Adhésions', 'write', TRUE, NOW()),
(33, 'type-adhesions.delete', 'Supprimer les types d\'adhésion', 'Adhésions', 'delete', TRUE, NOW());

-- MODULE PRODUITS & PRESTATIONS (15 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(34, 'produits-mutuels.read', 'Consulter les produits mutuels', 'Produits', 'read', TRUE, NOW()),
(35, 'produits-mutuels.write', 'Créer/modifier les produits mutuels', 'Produits', 'write', TRUE, NOW()),
(36, 'produits-mutuels.delete', 'Supprimer les produits mutuels', 'Produits', 'delete', TRUE, NOW()),
(37, 'produits-assureurs.read', 'Consulter les produits assureurs', 'Produits', 'read', TRUE, NOW()),
(38, 'produits-assureurs.write', 'Créer/modifier les produits assureurs', 'Produits', 'write', TRUE, NOW()),
(39, 'produits-assureurs.delete', 'Supprimer les produits assureurs', 'Produits', 'delete', TRUE, NOW()),
(40, 'assureurs.read', 'Consulter les assureurs', 'Produits', 'read', TRUE, NOW()),
(41, 'assureurs.write', 'Créer/modifier les assureurs', 'Produits', 'write', TRUE, NOW()),
(42, 'assureurs.delete', 'Supprimer les assureurs', 'Produits', 'delete', TRUE, NOW()),
(43, 'prestations.read', 'Consulter les prestations', 'Prestations', 'read', TRUE, NOW()),
(44, 'prestations.write', 'Créer/modifier les prestations', 'Prestations', 'write', TRUE, NOW()),
(45, 'prestations.delete', 'Supprimer les prestations', 'Prestations', 'delete', TRUE, NOW()),
(46, 'bons-envoi.read', 'Consulter les bons d\'envoi', 'Prestations', 'read', TRUE, NOW()),
(47, 'bons-envoi.write', 'Créer/modifier les bons d\'envoi', 'Prestations', 'write', TRUE, NOW()),
(48, 'bons-envoi.delete', 'Supprimer les bons d\'envoi', 'Prestations', 'delete', TRUE, NOW());

-- MODULE FINANCIER (9 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(49, 'collectes.read', 'Consulter les collectes', 'Financier', 'read', TRUE, NOW()),
(50, 'collectes.write', 'Créer/modifier les collectes', 'Financier', 'write', TRUE, NOW()),
(51, 'collectes.delete', 'Supprimer les collectes', 'Financier', 'delete', TRUE, NOW()),
(52, 'devises.read', 'Consulter les devises', 'Financier', 'read', TRUE, NOW()),
(53, 'devises.write', 'Créer/modifier les devises', 'Financier', 'write', TRUE, NOW()),
(54, 'devises.delete', 'Supprimer les devises', 'Financier', 'delete', TRUE, NOW()),
(55, 'financial.reports', 'Consulter les rapports financiers', 'Financier', 'read', TRUE, NOW()),
(56, 'financial.stats', 'Consulter les statistiques financières', 'Financier', 'read', TRUE, NOW()),
(57, 'financial.export', 'Exporter les données financières', 'Financier', 'export', TRUE, NOW());

-- MODULE MÉDICAL (6 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(58, 'antecedents.read', 'Consulter les antécédents médicaux', 'Médical', 'read', TRUE, NOW()),
(59, 'antecedents.write', 'Créer/modifier les antécédents', 'Médical', 'write', TRUE, NOW()),
(60, 'antecedents.delete', 'Supprimer les antécédents', 'Médical', 'delete', TRUE, NOW()),
(61, 'souscriptions.read', 'Consulter les souscriptions prestations', 'Médical', 'read', TRUE, NOW()),
(62, 'souscriptions.write', 'Créer/modifier les souscriptions', 'Médical', 'write', TRUE, NOW()),
(63, 'souscriptions.delete', 'Supprimer les souscriptions', 'Médical', 'delete', TRUE, NOW());

-- MODULE GÉOGRAPHIQUE (6 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(64, 'provinces.read', 'Consulter les provinces', 'Géographique', 'read', TRUE, NOW()),
(65, 'provinces.write', 'Créer/modifier les provinces', 'Géographique', 'write', TRUE, NOW()),
(66, 'provinces.delete', 'Supprimer les provinces', 'Géographique', 'delete', TRUE, NOW()),
(67, 'communes.read', 'Consulter les communes', 'Géographique', 'read', TRUE, NOW()),
(68, 'communes.write', 'Créer/modifier les communes', 'Géographique', 'write', TRUE, NOW()),
(69, 'communes.delete', 'Supprimer les communes', 'Géographique', 'delete', TRUE, NOW());

-- MODULE NOTIFICATIONS & COMMUNICATION (6 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(70, 'notifications.read', 'Consulter les notifications', 'Communication', 'read', TRUE, NOW()),
(71, 'notifications.write', 'Créer/envoyer des notifications', 'Communication', 'write', TRUE, NOW()),
(72, 'notifications.delete', 'Supprimer les notifications', 'Communication', 'delete', TRUE, NOW()),
(73, 'sms.send', 'Envoyer des SMS', 'Communication', 'send', TRUE, NOW()),
(74, 'emails.send', 'Envoyer des emails', 'Communication', 'send', TRUE, NOW()),
(75, 'push.send', 'Envoyer des notifications push', 'Communication', 'send', TRUE, NOW());

-- MODULE RAPPORTS & ANALYTICS (6 permissions)
INSERT INTO Permissions (IdPermission, Nom, Description, Categorie, Action, Statut, DateCreation) VALUES
(76, 'reports.dashboard', 'Accéder au tableau de bord', 'Rapports', 'read', TRUE, NOW()),
(77, 'reports.agents', 'Rapports sur les agents', 'Rapports', 'read', TRUE, NOW()),
(78, 'reports.affilies', 'Rapports sur les affiliés', 'Rapports', 'read', TRUE, NOW()),
(79, 'reports.financial', 'Rapports financiers', 'Rapports', 'read', TRUE, NOW()),
(80, 'reports.export', 'Exporter les rapports', 'Rapports', 'export', TRUE, NOW()),
(81, 'reports.custom', 'Créer des rapports personnalisés', 'Rapports', 'write', TRUE, NOW());

-- ═══════════════════════════════════════════════════════════════════════════════
-- 3️⃣ ATTRIBUTION DES PERMISSIONS AUX RÔLES
-- ═══════════════════════════════════════════════════════════════════════════════

-- SUPER-ADMIN (RoleId = 1) : TOUTES LES 81 PERMISSIONS
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT 1, IdPermission, NOW()
FROM Permissions
WHERE IdPermission BETWEEN 1 AND 81;

-- ADMIN (RoleId = 2) : Toutes sauf delete et system.admin (~60 permissions)
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT 2, IdPermission, NOW()
FROM Permissions
WHERE IdPermission BETWEEN 1 AND 81
  AND IdPermission NOT IN (3, 6, 9, 12, 15, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 60, 63, 66, 69, 72);

-- SUPERVISEUR (RoleId = 3) : Permissions de lecture + rapports (~35 permissions)
INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT 3, IdPermission, NOW()
FROM Permissions
WHERE IdPermission IN (
    1, 4, 7, 10, 13, 16, 18, 20, 22, 25, 28, 31, 34, 37, 40, 43, 46, 49, 52,
    55, 56, 58, 61, 64, 67, 70, 76, 77, 78, 79, 80
);

-- ═══════════════════════════════════════════════════════════════════════════════
-- 4️⃣ VÉRIFICATION FINALE
-- ═══════════════════════════════════════════════════════════════════════════════

SELECT '✅ TOTAL PERMISSIONS CRÉÉES' as Resultat, COUNT(*) as Total FROM Permissions;

SELECT '✅ PERMISSIONS PAR CATÉGORIE' as Resultat;
SELECT Categorie, COUNT(*) as NombrePermissions
FROM Permissions
GROUP BY Categorie
ORDER BY Categorie;

SELECT '✅ RÉSUMÉ PAR RÔLE' as Resultat;
SELECT 
    r.Nom as RoleName,
    COUNT(rp.IdRolePermission) as NombrePermissions
FROM Roles r
LEFT JOIN RolePermissions rp ON r.IdRole = rp.RoleId
WHERE r.IdRole IN (1, 2, 3)
GROUP BY r.IdRole, r.Nom
ORDER BY r.IdRole;

SELECT '✅ DÉTAIL SUPER-ADMIN (81 permissions)' as Resultat;
SELECT COUNT(*) as Total FROM RolePermissions WHERE RoleId = 1;

SELECT '✅ DÉTAIL ADMIN (~60 permissions)' as Resultat;
SELECT COUNT(*) as Total FROM RolePermissions WHERE RoleId = 2;

SELECT '✅ DÉTAIL SUPERVISEUR (~35 permissions)' as Resultat;
SELECT COUNT(*) as Total FROM RolePermissions WHERE RoleId = 3;

-- Afficher quelques exemples de permissions par rôle
SELECT '✅ EXEMPLES PERMISSIONS SUPER-ADMIN' as Resultat;
SELECT p.Nom, p.Categorie, p.Description
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 1
ORDER BY p.IdPermission
LIMIT 10;

SELECT '✅ EXEMPLES PERMISSIONS ADMIN' as Resultat;
SELECT p.Nom, p.Categorie, p.Description
FROM RolePermissions rp
INNER JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE rp.RoleId = 2
ORDER BY p.IdPermission
LIMIT 10;
