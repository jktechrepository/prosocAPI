-- ========================================
-- Script de création de la base de données Prosoc (Production)
-- Généré par EF Core --idempotent
-- Peut être exécuté plusieurs fois sans erreur
-- ========================================

-- Créer la table de suivi des migrations si elle n'existe pas
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

-- Appliquer la migration initiale si elle n'a pas déjà été appliquée
-- Si vous partez d'une base vide, vous pouvez ignorer les vérifications IF EXISTS et exécuter directement le SQL ci-dessous

-- ========================================
-- 1. CRÉATION DES TABLES
-- ========================================

-- Affilies
CREATE TABLE IF NOT EXISTS `Affilies` (
    `IdAffilie` int NOT NULL AUTO_INCREMENT,
    `CodeAdhesion` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Prenom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `DateNaissance` datetime(6) NOT NULL,
    `Telephone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Affilies` PRIMARY KEY (`IdAffilie`)
) CHARACTER SET=utf8mb4;

-- Assureurs
CREATE TABLE IF NOT EXISTS `Assureurs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Assureurs` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- CategoriesAdhesions
CREATE TABLE IF NOT EXISTS `CategoriesAdhesions` (
    `IdCategorieAdhesion` int NOT NULL AUTO_INCREMENT,
    `Libelle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriesAdhesions` PRIMARY KEY (`IdCategorieAdhesion`)
) CHARACTER SET=utf8mb4;

-- CategoriesAgents
CREATE TABLE IF NOT EXISTS `CategoriesAgents` (
    `IdCategorieAgent` int NOT NULL AUTO_INCREMENT,
    `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DescriptionCategorie` longtext CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriesAgents` PRIMARY KEY (`IdCategorieAgent`)
) CHARACTER SET=utf8mb4;

-- CategoriesPrestations
CREATE TABLE IF NOT EXISTS `CategoriesPrestations` (
    `IdCategoriePrestation` int NOT NULL AUTO_INCREMENT,
    `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DescriptionCategorie` longtext CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriesPrestations` PRIMARY KEY (`IdCategoriePrestation`)
) CHARACTER SET=utf8mb4;

-- CodesAdhesionSequences
CREATE TABLE IF NOT EXISTS `CodesAdhesionSequences` (
    `Prefix` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `NextValue` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_CodesAdhesionSequences` PRIMARY KEY (`Prefix`)
) CHARACTER SET=utf8mb4;

-- Collectes
CREATE TABLE IF NOT EXISTS `Collectes` (
    `IdCollecte` int NOT NULL AUTO_INCREMENT,
    `ReferencePaiement` varchar(100) CHARACTER SET utf8mb4 NULL,
    `ModePaiement` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Operateur` varchar(100) CHARACTER SET utf8mb4 NULL,
    `StatutPaiement` varchar(50) CHARACTER SET utf8mb4 NULL,
    `MontantRecu` decimal(10,2) NULL,
    `MontantAttendu` decimal(10,2) NULL,
    `DateCollecte` datetime(6) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `AffilieId` int NULL,
    `AgentId` int NULL,
    `DeviseId` int NULL,
    `SouscriptionPrestationId` int NULL,
    CONSTRAINT `PK_Collectes` PRIMARY KEY (`IdCollecte`),
    CONSTRAINT `FK_Collectes_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_Collectes_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Collectes_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationId` FOREIGN KEY (`SouscriptionPrestationId`) REFERENCES `SouscriptionsPrestations` (`IdSouscription`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

-- Communes
CREATE TABLE IF NOT EXISTS `Communes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ProvinceId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Communes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Communes_Provinces_ProvinceId` FOREIGN KEY (`ProvinceId`) REFERENCES `Provinces` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- Devises
CREATE TABLE IF NOT EXISTS `Devises` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Code` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `Symbole` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Devises` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- Agents
CREATE TABLE IF NOT EXISTS `Agents` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CodeAT` varchar(50) CHARACTER SET utf8mb4 NULL,
    `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Matricule` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Phone` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `ZoneSocialeId` int NULL,
    `CategorieAgentId` int NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Agents` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`) ON DELETE CASCADE,
    CONSTRAINT `FK_Agents_ZonesSociales_ZoneSocialeId` FOREIGN KEY (`ZoneSocialeId`) REFERENCES `ZonesSociales` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

-- Adhesions
CREATE TABLE IF NOT EXISTS `Adhesions` (
    `IdAdhesion` int NOT NULL AUTO_INCREMENT,
    `StatutDossier` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `AgentId` int NOT NULL,
    `AffilieId` int NOT NULL,
    `TypeAdhesionId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Adhesions` PRIMARY KEY (`IdAdhesion`),
    CONSTRAINT `FK_Adhesions_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Adhesions_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_Adhesions_TypeAdhesions_TypeAdhesionId` FOREIGN KEY (`TypeAdhesionId`) REFERENCES `TypeAdhesions` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- Permissions
CREATE TABLE IF NOT EXISTS `Permissions` (
    `IdPermission` int NOT NULL AUTO_INCREMENT,
    `NomPermission` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `DescriptionPermission` longtext CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Permissions` PRIMARY KEY (`IdPermission`)
) CHARACTER SET=utf8mb4;

-- Provinces
CREATE TABLE IF NOT EXISTS `Provinces` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Provinces` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- RolePermissions
CREATE TABLE IF NOT EXISTS `RolePermissions` (
    `IdRolePermission` int NOT NULL AUTO_INCREMENT,
    `RoleId` int NOT NULL,
    `PermissionId` int NOT NULL,
    `DateAttribution` datetime(6) NOT NULL,
    CONSTRAINT `PK_RolePermissions` PRIMARY KEY (`IdRolePermission`),
    CONSTRAINT `FK_RolePermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`IdPermission`) ON DELETE CASCADE,
    CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`IdRole`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- Roles
CREATE TABLE IF NOT EXISTS `Roles` (
    `IdRole` int NOT NULL AUTO_INCREMENT,
    `NomRole` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `DescriptionRole` longtext CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`IdRole`)
) CHARACTER SET=utf8mb4;

-- SouscriptionsPrestations
CREATE TABLE IF NOT EXISTS `SouscriptionsPrestations` (
    `IdSouscription` int NOT NULL AUTO_INCREMENT,
    `Montant` decimal(10,2) NOT NULL,
    `DateSouscription` datetime(6) NOT NULL,
    `DateFin` datetime(6) NULL,
    `AffilieId` int NOT NULL,
    `PrestationId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_SouscriptionsPrestations` PRIMARY KEY (`IdSouscription`),
    CONSTRAINT `FK_SouscriptionsPrestations_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_SouscriptionsPrestations_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`IdPrestation`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- TypeAdhesions
CREATE TABLE IF NOT EXISTS `TypeAdhesions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Libelle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_TypeAdhesions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- Prestations
CREATE TABLE IF NOT EXISTS `Prestations` (
    `IdPrestation` int NOT NULL AUTO_INCREMENT,
    `Libelle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Cotisation` decimal(10,2) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `CategoriePrestationId` int NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Prestations` PRIMARY KEY (`IdPrestation`),
    CONSTRAINT `FK_Prestations_CategoriesPrestations_CategoriePrestationId` FOREIGN KEY (`CategoriePrestationId`) REFERENCES `CategoriesPrestations` (`IdCategoriePrestation`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- UserRoles
CREATE TABLE IF NOT EXISTS `UserRoles` (
    `IdUserRole` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `RoleId` int NOT NULL,
    `DateAttribution` datetime(6) NOT NULL,
    CONSTRAINT `PK_UserRoles` PRIMARY KEY (`IdUserRole`),
    CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`IdRole`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoles_Utilisateurs_UserId` FOREIGN KEY (`UserId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- Utilisateurs
CREATE TABLE IF NOT EXISTS `Utilisateurs` (
    `IdUtilisateur` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Prenom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `MotDePasseHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Utilisateurs` PRIMARY KEY (`IdUtilisateur`)
) CHARACTER SET=utf8mb4;

-- ZonesSociales
CREATE TABLE IF NOT EXISTS `ZonesSociales` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CommuneId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_ZonesSociales` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ZonesSociales_Communes_CommuneId` FOREIGN KEY (`CommuneId`) REFERENCES `Communes` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- ========================================
-- 2. CRÉATION DES INDEX
-- ========================================

-- Index unique sur Agents.CodeAT (nullable)
CREATE UNIQUE INDEX `IX_Agents_CodeAT` ON `Agents` (`CodeAT`);

-- Index sur Affilies pour recherche rapide
CREATE INDEX `IX_Affilies_Nom` ON `Affilies` (`Nom`);
CREATE INDEX `IX_Affilies_Prenom` ON `Affilies` (`Prenom`);
CREATE INDEX `IX_Affilies_CodeAdhesion` ON `Affilies` (`CodeAdhesion`);

-- Index sur Adhesions
CREATE INDEX `IX_Adhesions_AffilieId` ON `Adhesions` (`AffilieId`);
CREATE INDEX `IX_Adhesions_AgentId` ON `Adhesions` (`AgentId`);

-- Index sur Collectes
CREATE INDEX `IX_Collectes_AffilieId` ON `Collectes` (`AffilieId`);
CREATE INDEX `IX_Collectes_AgentId` ON `Collectes` (`AgentId`);
CREATE INDEX `IX_Collectes_DateCollecte` ON `Collectes` (`DateCollecte`);

-- Index sur SouscriptionsPrestations
CREATE INDEX `IX_SouscriptionsPrestations_AffilieId` ON `SouscriptionsPrestations` (`AffilieId`);
CREATE INDEX `IX_SouscriptionsPrestations_PrestationId` ON `SouscriptionsPrestations` (`PrestationId`);

-- Index géographiques
CREATE INDEX `IX_Communes_ProvinceId` ON `Communes` (`ProvinceId`);
CREATE INDEX `IX_ZonesSociales_CommuneId` ON `ZonesSociales` (`CommuneId`);

-- ========================================
-- 3. DONNÉES DE RÉFÉRENCE (SEED DATA)
-- ========================================

-- Provinces
INSERT IGNORE INTO `Provinces` (`Id`, `Nom`, `Statut`, `DateCreation`) VALUES
(1, 'Kinshasa', 1, NOW()),
(2, 'Bas-Congo', 1, NOW()),
(3, 'Bandundu', 1, NOW()),
(4, 'Katanga', 1, NOW());

-- Devises
INSERT IGNORE INTO `Devises` (`Id`, `Nom`, `Code`, `Symbole`, `Statut`, `DateCreation`) VALUES
(1, 'Franc Congolais', 'CDF', 'FC', 1, NOW()),
(2, 'Dollar Américain', 'USD', '$', 1, NOW());

-- TypeAdhesions
INSERT IGNORE INTO `TypeAdhesions` (`Id`, `Libelle`, `Description`, `DateCreation`) VALUES
(1, 'Individuel', 'Adhésion individuelle', NOW()),
(2, 'Familial', 'Adhésion familiale', NOW()),
(3, 'Groupé', 'Adhésion groupée', NOW());

-- CategoriesAgents
INSERT IGNORE INTO `CategoriesAgents` (`IdCategorieAgent`, `LibelleCategorie`, `DescriptionCategorie`, `Statut`, `DateCreation`) VALUES
(1, 'Agent Principal', 'Agent principal avec droits complets', 1, NOW()),
(2, 'Agent Secondaire', 'Agent avec droits limités', 1, NOW());

-- CategoriesPrestations
INSERT IGNORE INTO `CategoriesPrestations` (`IdCategoriePrestation`, `LibelleCategorie`, `DescriptionCategorie`, `Statut`, `DateCreation`) VALUES
(1, 'Santé', 'Prestations de santé', 1, NOW()),
(2, 'Éducation', 'Prestations éducatives', 1, NOW()),
(3, 'Logement', 'Prestations de logement', 1, NOW());

-- Permissions
INSERT IGNORE INTO `Permissions` (`IdPermission`, `NomPermission`, `DescriptionPermission`, `DateCreation`) VALUES
(1, 'CREATE_USER', 'Créer un utilisateur', NOW()),
(2, 'READ_USER', 'Voir les utilisateurs', NOW()),
(3, 'UPDATE_USER', 'Modifier un utilisateur', NOW()),
(4, 'DELETE_USER', 'Supprimer un utilisateur', NOW()),
(5, 'CREATE_AGENT', 'Créer un agent', NOW()),
(6, 'READ_AGENT', 'Voir les agents', NOW()),
(7, 'UPDATE_AGENT', 'Modifier un agent', NOW()),
(8, 'DELETE_AGENT', 'Supprimer un agent', NOW()),
(9, 'CREATE_ADHESION', 'Créer une adhésion', NOW()),
(10, 'READ_ADHESION', 'Voir les adhésions', NOW()),
(11, 'UPDATE_ADHESION', 'Modifier une adhésion', NOW()),
(12, 'DELETE_ADHESION', 'Supprimer une adhésion', NOW());

-- Roles
INSERT IGNORE INTO `Roles` (`IdRole`, `NomRole`, `DescriptionRole`, `DateCreation`) VALUES
(1, 'Admin', 'Administrateur système', NOW()),
(2, 'Agent', 'Agent d\'adhésion', NOW()),
(3, 'Superviseur', 'Superviseur d\'équipe', NOW()),
(4, 'Visiteur', 'Utilisateur en lecture seule', NOW()),
(5, 'SuperAdmin', 'Super administrateur', NOW()),
(6, 'Manager', 'Manager d\'opérations', NOW());

-- Attribution des permissions aux rôles
INSERT IGNORE INTO `RolePermissions` (`RoleId`, `PermissionId`, `DateAttribution`) VALUES
-- Admin (toutes les permissions)
(1, 1, NOW()), (1, 2, NOW()), (1, 3, NOW()), (1, 4, NOW()),
(1, 5, NOW()), (1, 6, NOW()), (1, 7, NOW()), (1, 8, NOW()),
(1, 9, NOW()), (1, 10, NOW()), (1, 11, NOW()), (1, 12, NOW()),
-- Agent (création et lecture)
(2, 5, NOW()), (2, 6, NOW()), (2, 9, NOW()), (2, 10, NOW()),
-- Superviseur (lecture et modification)
(3, 2, NOW()), (3, 3, NOW()), (3, 6, NOW()), (3, 7, NOW()),
(3, 10, NOW()), (3, 11, NOW()),
-- Visiteur (lecture seule)
(4, 2, NOW()), (4, 6, NOW()), (4, 10, NOW()),
-- SuperAdmin (toutes les permissions)
(5, 1, NOW()), (5, 2, NOW()), (5, 3, NOW()), (5, 4, NOW()),
(5, 5, NOW()), (5, 6, NOW()), (5, 7, NOW()), (5, 8, NOW()),
(5, 9, NOW()), (5, 10, NOW()), (5, 11, NOW()), (5, 12, NOW()),
-- Manager (toutes sauf suppression)
(6, 1, NOW()), (6, 2, NOW()), (6, 3, NOW()),
(6, 5, NOW()), (6, 6, NOW()), (6, 7, NOW()),
(6, 9, NOW()), (6, 10, NOW()), (6, 11, NOW());

-- Utilisateurs par défaut
-- Mot de passe: "admin" (hashé avec BCrypt)
INSERT IGNORE INTO `Utilisateurs` (`IdUtilisateur`, `Nom`, `Prenom`, `Email`, `MotDePasseHash`, `Statut`, `DateCreation`) VALUES
(1, 'Admin', 'System', 'admin@prosoc.cd', '$2a$11$KlhUvt0MtjpsZ3sPHGgt..PYKFzPm.pkcvPO0PfmDRVlJA90FDU36', 1, NOW()),
(2, 'Super', 'Admin', 'superadmin@prosoc.cd', '$2a$11$Yz/F2m5l3.SIGp9DR0JdnuphR4en9tUi6I6E8rf.tUFHkQMrIWx3i', 1, NOW());

-- Attribution des rôles aux utilisateurs
INSERT IGNORE INTO `UserRoles` (`UserId`, `RoleId`, `DateAttribution`) VALUES
(1, 1, NOW()),  -- admin -> Admin
(2, 5, NOW());  -- superadmin -> SuperAdmin

-- ========================================
-- 4. ENREGISTREMENT DES MIGRATIONS
-- ========================================

-- Marquer les migrations comme appliquées
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES
('20260302162849_InitialCreateDb', '6.0.25'),
('20260303141603_MakeAgentCodeATAndZoneNullable', '6.0.25');

COMMIT;

-- ========================================
-- 5. VÉRIFICATION
-- ========================================

-- Afficher un résumé des tables créées
SELECT 
    TABLE_NAME as 'Table',
    TABLE_ROWS as 'Lignes (estimé)',
    DATA_LENGTH as 'Taille (octets)'
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = DATABASE() 
    AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- ========================================
-- FIN DU SCRIPT
-- ========================================
