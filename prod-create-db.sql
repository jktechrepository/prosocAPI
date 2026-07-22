CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Affilies` (
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

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Assureurs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_Assureurs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `CategoriesAdhesions` (
        `IdCategorieAdhesion` int NOT NULL AUTO_INCREMENT,
        `Libelle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CategoriesAdhesions` PRIMARY KEY (`IdCategorieAdhesion`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `CategoriesAgents` (
        `IdCategorieAgent` int NOT NULL AUTO_INCREMENT,
        `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_CategoriesAgents` PRIMARY KEY (`IdCategorieAgent`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Devises` (
        `IdDevise` int NOT NULL AUTO_INCREMENT,
        `Code` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `TauxChange` decimal(18,6) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_Devises` PRIMARY KEY (`IdDevise`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Notifications` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Titre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Message` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `Type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `EnvoyeurId` int NULL,
        `RecepteurId` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateLecture` datetime(6) NULL,
        `EstLu` tinyint(1) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Notifications` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Permissions` (
        `IdPermission` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Categorie` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Action` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Permissions` PRIMARY KEY (`IdPermission`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `ProduitsMutuels` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `PrixMensuel` decimal(18,2) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_ProduitsMutuels` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Provinces` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_Provinces` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Roles` (
        `IdRole` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Niveau` int NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_Roles` PRIMARY KEY (`IdRole`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `TypeAdhesions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Libelle` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `MaxDependants` int NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Montant` decimal(18,2) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_TypeAdhesions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Antecedants` (
        `IdAntecedant` int NOT NULL AUTO_INCREMENT,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `AffilieId` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Antecedants` PRIMARY KEY (`IdAntecedant`),
        CONSTRAINT `FK_Antecedants_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Dependants` (
        `IdDependant` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `LienParente` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `AffilieId` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Dependants` PRIMARY KEY (`IdDependant`),
        CONSTRAINT `FK_Dependants_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `ProduitsAssureurs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AssureurId` int NOT NULL,
        `CommissionMutuelle` decimal(18,2) NOT NULL,
        `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `PrixMensuel` decimal(18,2) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        CONSTRAINT `PK_ProduitsAssureurs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProduitsAssureurs_Assureurs_AssureurId` FOREIGN KEY (`AssureurId`) REFERENCES `Assureurs` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Communes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `ProvinceId` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Communes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Communes_Provinces_ProvinceId` FOREIGN KEY (`ProvinceId`) REFERENCES `Provinces` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `RolePermissions` (
        `IdRolePermission` int NOT NULL AUTO_INCREMENT,
        `RoleId` int NOT NULL,
        `PermissionId` int NOT NULL,
        `DateAttribution` datetime(6) NOT NULL,
        `IdUtilisateurAttribution` int NULL,
        CONSTRAINT `PK_RolePermissions` PRIMARY KEY (`IdRolePermission`),
        CONSTRAINT `FK_RolePermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`IdPermission`) ON DELETE CASCADE,
        CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`IdRole`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Prestations` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `NomPrestation` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `ProduitMutuelId` int NULL,
        `ProduitAssureurId` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Prestations` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Prestations_ProduitsAssureurs_ProduitAssureurId` FOREIGN KEY (`ProduitAssureurId`) REFERENCES `ProduitsAssureurs` (`Id`),
        CONSTRAINT `FK_Prestations_ProduitsMutuels_ProduitMutuelId` FOREIGN KEY (`ProduitMutuelId`) REFERENCES `ProduitsMutuels` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `ZonesSociales` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `CommuneId` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ZonesSociales` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ZonesSociales_Communes_CommuneId` FOREIGN KEY (`CommuneId`) REFERENCES `Communes` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `BonsEnvoi` (
        `IdBonEnvoi` int NOT NULL AUTO_INCREMENT,
        `NumeroBon` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `AffilieId` int NOT NULL,
        `PrestationId` int NOT NULL,
        `DateEmission` datetime(6) NOT NULL,
        `DateUtilisation` datetime(6) NULL,
        `EstUtilise` tinyint(1) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_BonsEnvoi` PRIMARY KEY (`IdBonEnvoi`),
        CONSTRAINT `FK_BonsEnvoi_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
        CONSTRAINT `FK_BonsEnvoi_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `SouscriptionsPrestations` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AffilieId` int NOT NULL,
        `PrestationId` int NOT NULL,
        `DateSouscription` datetime(6) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_SouscriptionsPrestations` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_SouscriptionsPrestations_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
        CONSTRAINT `FK_SouscriptionsPrestations_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Agents` (
        `IdAgent` int NOT NULL AUTO_INCREMENT,
        `CodeAT` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Matricule` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `CategorieAgentId` int NULL,
        `ZoneSocialeId` int NOT NULL,
        CONSTRAINT `PK_Agents` PRIMARY KEY (`IdAgent`),
        CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`),
        CONSTRAINT `FK_Agents_ZonesSociales_ZoneSocialeId` FOREIGN KEY (`ZoneSocialeId`) REFERENCES `ZonesSociales` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Adhesions` (
        `IdAdhesion` int NOT NULL AUTO_INCREMENT,
        `StatutDossier` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `CategorieAdhesionId` int NOT NULL,
        `TypeAdhesionId` int NOT NULL,
        `AgentId` int NOT NULL,
        `AffilieId` int NOT NULL,
        CONSTRAINT `PK_Adhesions` PRIMARY KEY (`IdAdhesion`),
        CONSTRAINT `FK_Adhesions_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
        CONSTRAINT `FK_Adhesions_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
        CONSTRAINT `FK_Adhesions_CategoriesAdhesions_CategorieAdhesionId` FOREIGN KEY (`CategorieAdhesionId`) REFERENCES `CategoriesAdhesions` (`IdCategorieAdhesion`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Adhesions_TypeAdhesions_TypeAdhesionId` FOREIGN KEY (`TypeAdhesionId`) REFERENCES `TypeAdhesions` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Collectes` (
        `IdCollecte` int NOT NULL AUTO_INCREMENT,
        `AffilieId` int NOT NULL,
        `AgentId` int NOT NULL,
        `Montant` decimal(18,2) NOT NULL,
        `DeviseId` int NOT NULL,
        `DateCollecte` datetime(6) NOT NULL,
        `Observation` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `PrestationId` int NULL,
        CONSTRAINT `PK_Collectes` PRIMARY KEY (`IdCollecte`),
        CONSTRAINT `FK_Collectes_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
        CONSTRAINT `FK_Collectes_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
        CONSTRAINT `FK_Collectes_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE CASCADE,
        CONSTRAINT `FK_Collectes_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `RetraitsAgents` (
        `IdRetraitAgent` int NOT NULL AUTO_INCREMENT,
        `AgentId` int NOT NULL,
        `Montant` decimal(18,2) NOT NULL,
        `CodeRetraitPin` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `DateDemande` datetime(6) NOT NULL,
        `EstValide` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `DeviseIdDevise` int NULL,
        CONSTRAINT `PK_RetraitsAgents` PRIMARY KEY (`IdRetraitAgent`),
        CONSTRAINT `FK_RetraitsAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
        CONSTRAINT `FK_RetraitsAgents_Devises_DeviseIdDevise` FOREIGN KEY (`DeviseIdDevise`) REFERENCES `Devises` (`IdDevise`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `TargetsAgents` (
        `IdTargetAgent` int NOT NULL AUTO_INCREMENT,
        `AgentId` int NOT NULL,
        `LibelleTarget` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `MontantTarget` decimal(18,2) NOT NULL,
        `DateDebut` datetime(6) NOT NULL,
        `DateFin` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_TargetsAgents` PRIMARY KEY (`IdTargetAgent`),
        CONSTRAINT `FK_TargetsAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `Utilisateurs` (
        `IdUtilisateur` int NOT NULL AUTO_INCREMENT,
        `ReferenceUtilisateur` char(36) COLLATE ascii_general_ci NULL,
        `NomUtilisateur` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PostNomUtilisateur` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PrenomUtilisateur` varchar(100) CHARACTER SET utf8mb4 NULL,
        `MotDePasseHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DefaultUsername` longtext CHARACTER SET utf8mb4 NULL,
        `DoitChangerMotDePasse` tinyint(1) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `RoleId` int NULL,
        `AgentId` int NULL,
        `AffilieId` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        `IsConnecte` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Utilisateurs` PRIMARY KEY (`IdUtilisateur`),
        CONSTRAINT `FK_Utilisateurs_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`),
        CONSTRAINT `FK_Utilisateurs_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_Utilisateurs_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`IdRole`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `WalletsAgents` (
        `IdWalletAgent` int NOT NULL AUTO_INCREMENT,
        `AgentId` int NOT NULL,
        `SoldeCourant` decimal(18,2) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_WalletsAgents` PRIMARY KEY (`IdWalletAgent`),
        CONSTRAINT `FK_WalletsAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `PasswordResetTokens` (
        `IdPasswordResetToken` int NOT NULL AUTO_INCREMENT,
        `UtilisateurId` int NOT NULL,
        `Token` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateExpiration` datetime(6) NOT NULL,
        `DateUtilisation` datetime(6) NULL,
        CONSTRAINT `PK_PasswordResetTokens` PRIMARY KEY (`IdPasswordResetToken`),
        CONSTRAINT `FK_PasswordResetTokens_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `RefreshTokens` (
        `IdRefreshToken` int NOT NULL AUTO_INCREMENT,
        `UtilisateurId` int NOT NULL,
        `TokenHash` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateExpiration` datetime(6) NOT NULL,
        `DateRevocation` datetime(6) NULL,
        `DeviceInfo` varchar(200) CHARACTER SET utf8mb4 NULL,
        `IpAddress` varchar(50) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`IdRefreshToken`),
        CONSTRAINT `FK_RefreshTokens_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `UserDevices` (
        `IdUserDevice` int NOT NULL AUTO_INCREMENT,
        `UtilisateurId` int NOT NULL,
        `FcmToken` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `DeviceType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `DeviceModel` varchar(100) CHARACTER SET utf8mb4 NULL,
        `OsVersion` varchar(50) CHARACTER SET utf8mb4 NULL,
        `DefaultDevice` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateEnregistrement` datetime(6) NOT NULL,
        `DateDerniereUtilisation` datetime(6) NULL,
        CONSTRAINT `PK_UserDevices` PRIMARY KEY (`IdUserDevice`),
        CONSTRAINT `FK_UserDevices_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `UserPermissions` (
        `IdUserPermission` int NOT NULL AUTO_INCREMENT,
        `UtilisateurId` int NOT NULL,
        `PermissionId` int NOT NULL,
        `IsGranted` tinyint(1) NOT NULL,
        `DateAttribution` datetime(6) NOT NULL,
        `DateExpiration` datetime(6) NULL,
        `Commentaire` varchar(500) CHARACTER SET utf8mb4 NULL,
        `AttribueParIdUtilisateur` int NULL,
        CONSTRAINT `PK_UserPermissions` PRIMARY KEY (`IdUserPermission`),
        CONSTRAINT `FK_UserPermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`IdPermission`) ON DELETE CASCADE,
        CONSTRAINT `FK_UserPermissions_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `UserRoles` (
        `IdUserRole` int NOT NULL AUTO_INCREMENT,
        `UtilisateurId` int NOT NULL,
        `RoleId` int NOT NULL,
        `IsPrimary` tinyint(1) NOT NULL,
        `DateAttribution` datetime(6) NOT NULL,
        `IdUtilisateurAttribution` int NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_UserRoles` PRIMARY KEY (`IdUserRole`),
        CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`IdRole`) ON DELETE CASCADE,
        CONSTRAINT `FK_UserRoles_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE TABLE `WalletMouvements` (
        `IdWalletMouvement` int NOT NULL AUTO_INCREMENT,
        `WalletId` int NOT NULL,
        `Montant` decimal(18,2) NOT NULL,
        `TypeMouvement` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `DateAction` datetime(6) NOT NULL,
        `ReferenceId` int NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `CollecteIdCollecte` int NULL,
        CONSTRAINT `PK_WalletMouvements` PRIMARY KEY (`IdWalletMouvement`),
        CONSTRAINT `FK_WalletMouvements_Collectes_CollecteIdCollecte` FOREIGN KEY (`CollecteIdCollecte`) REFERENCES `Collectes` (`IdCollecte`),
        CONSTRAINT `FK_WalletMouvements_WalletsAgents_WalletId` FOREIGN KEY (`WalletId`) REFERENCES `WalletsAgents` (`IdWalletAgent`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `Devises` (`IdDevise`, `Code`, `DateCreation`, `DateModification`, `Nom`, `Statut`, `TauxChange`)
    VALUES (1, 'CDF', TIMESTAMP '2026-03-02 18:28:48', NULL, 'Franc Congolais', TRUE, 1.0);
    INSERT INTO `Devises` (`IdDevise`, `Code`, `DateCreation`, `DateModification`, `Nom`, `Statut`, `TauxChange`)
    VALUES (2, 'USD', TIMESTAMP '2026-03-02 18:28:48', NULL, 'Dollar Américain', TRUE, 2500.0);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (1, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Lire les informations des utilisateurs', 'users.read', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (2, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Créer et modifier les utilisateurs', 'users.write', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (3, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Supprimer les utilisateurs', 'users.delete', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (4, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Lire les informations des rôles', 'roles.read', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (5, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Créer et modifier les rôles', 'roles.write', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (6, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Supprimer les rôles', 'roles.delete', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (7, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Lire les permissions', 'permissions.read', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (8, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Créer et modifier les permissions', 'permissions.write', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (9, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Administration système complète', 'system.admin', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (10, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Lire les rapports', 'reports.read', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (11, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Lire les informations financières', 'financial.read', TRUE);
    INSERT INTO `Permissions` (`IdPermission`, `Action`, `Categorie`, `DateCreation`, `Description`, `Nom`, `Statut`)
    VALUES (12, '', '', TIMESTAMP '2026-03-02 18:28:48', 'Créer et modifier les données financières', 'financial.write', TRUE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `Provinces` (`Id`, `DateCreation`, `DateModification`, `Nom`, `Statut`)
    VALUES (1, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Kinshasa', TRUE);
    INSERT INTO `Provinces` (`Id`, `DateCreation`, `DateModification`, `Nom`, `Statut`)
    VALUES (2, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Haut-Katanga', TRUE);
    INSERT INTO `Provinces` (`Id`, `DateCreation`, `DateModification`, `Nom`, `Statut`)
    VALUES (3, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Lualaba', TRUE);
    INSERT INTO `Provinces` (`Id`, `DateCreation`, `DateModification`, `Nom`, `Statut`)
    VALUES (4, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Kongo-Central', TRUE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `Roles` (`IdRole`, `DateCreation`, `Description`, `Niveau`, `Nom`, `Statut`)
    VALUES (1, TIMESTAMP '2026-03-02 18:28:48', 'Administrateur système avec tous les droits', 1, 'Super-Admin', TRUE);
    INSERT INTO `Roles` (`IdRole`, `DateCreation`, `Description`, `Niveau`, `Nom`, `Statut`)
    VALUES (2, TIMESTAMP '2026-03-02 18:28:48', 'Administrateur avec droits de gestion', 2, 'Admin', TRUE);
    INSERT INTO `Roles` (`IdRole`, `DateCreation`, `Description`, `Niveau`, `Nom`, `Statut`)
    VALUES (3, TIMESTAMP '2026-03-02 18:28:48', 'Superviseur d''équipe', 3, 'Superviseur', TRUE);
    INSERT INTO `Roles` (`IdRole`, `DateCreation`, `Description`, `Niveau`, `Nom`, `Statut`)
    VALUES (4, TIMESTAMP '2026-03-02 18:28:48', 'Agent de terrain', 4, 'Agent (AT)', TRUE);
    INSERT INTO `Roles` (`IdRole`, `DateCreation`, `Description`, `Niveau`, `Nom`, `Statut`)
    VALUES (5, TIMESTAMP '2026-03-02 18:28:48', 'Agent administratif', 5, 'Agent (AA)', TRUE);
    INSERT INTO `Roles` (`IdRole`, `DateCreation`, `Description`, `Niveau`, `Nom`, `Statut`)
    VALUES (6, TIMESTAMP '2026-03-02 18:28:48', 'Membre affilié', 10, 'Affilié', TRUE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `TypeAdhesions` (`Id`, `DateCreation`, `DateModification`, `Description`, `Libelle`, `MaxDependants`, `Montant`, `Statut`)
    VALUES (1, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Adhésion individuelle sans dépendants', 'Solo', 0, 5000.0, TRUE);
    INSERT INTO `TypeAdhesions` (`Id`, `DateCreation`, `DateModification`, `Description`, `Libelle`, `MaxDependants`, `Montant`, `Statut`)
    VALUES (2, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Adhésion familiale jusqu''à 3 dépendants', 'F3', 3, 15000.0, TRUE);
    INSERT INTO `TypeAdhesions` (`Id`, `DateCreation`, `DateModification`, `Description`, `Libelle`, `MaxDependants`, `Montant`, `Statut`)
    VALUES (3, TIMESTAMP '2026-03-02 18:28:48', NULL, 'Adhésion familiale jusqu''à 6 dépendants', 'F6', 6, 25000.0, TRUE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `Utilisateurs` (`IdUtilisateur`, `AffilieId`, `AgentId`, `DateCreation`, `DefaultUsername`, `DoitChangerMotDePasse`, `IsConnecte`, `MotDePasseHash`, `NomUtilisateur`, `PostNomUtilisateur`, `PrenomUtilisateur`, `ReferenceUtilisateur`, `RoleId`, `Statut`)
    VALUES (1, NULL, NULL, TIMESTAMP '2026-03-02 18:28:48', 'superadmin@prosoc.cd', FALSE, FALSE, '$2a$11$Vjo/QJssHf4wg.pVvIxF1ets5B2eawGDNydJw3eDKmvg/DEWAWVqa', 'superadmin@prosoc.cd', NULL, NULL, NULL, NULL, TRUE);
    INSERT INTO `Utilisateurs` (`IdUtilisateur`, `AffilieId`, `AgentId`, `DateCreation`, `DefaultUsername`, `DoitChangerMotDePasse`, `IsConnecte`, `MotDePasseHash`, `NomUtilisateur`, `PostNomUtilisateur`, `PrenomUtilisateur`, `ReferenceUtilisateur`, `RoleId`, `Statut`)
    VALUES (2, NULL, NULL, TIMESTAMP '2026-03-02 18:28:48', 'admin@prosoc.cd', FALSE, FALSE, '$2a$11$dLP3EhQRwBbfme1OaYoF1.lA6TyDSGJqjvRVO9fnPdydGKQutRkvK', 'admin@prosoc.cd', NULL, NULL, NULL, NULL, TRUE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (1, TIMESTAMP '2026-03-02 18:28:48', NULL, 1, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (2, TIMESTAMP '2026-03-02 18:28:48', NULL, 2, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (3, TIMESTAMP '2026-03-02 18:28:48', NULL, 3, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (4, TIMESTAMP '2026-03-02 18:28:48', NULL, 4, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (5, TIMESTAMP '2026-03-02 18:28:48', NULL, 5, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (6, TIMESTAMP '2026-03-02 18:28:48', NULL, 6, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (7, TIMESTAMP '2026-03-02 18:28:48', NULL, 7, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (8, TIMESTAMP '2026-03-02 18:28:48', NULL, 8, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (9, TIMESTAMP '2026-03-02 18:28:48', NULL, 9, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (10, TIMESTAMP '2026-03-02 18:28:48', NULL, 10, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (11, TIMESTAMP '2026-03-02 18:28:48', NULL, 11, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (12, TIMESTAMP '2026-03-02 18:28:48', NULL, 12, 1);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (13, TIMESTAMP '2026-03-02 18:28:48', NULL, 1, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (14, TIMESTAMP '2026-03-02 18:28:48', NULL, 2, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (15, TIMESTAMP '2026-03-02 18:28:48', NULL, 4, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (16, TIMESTAMP '2026-03-02 18:28:48', NULL, 5, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (17, TIMESTAMP '2026-03-02 18:28:48', NULL, 7, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (18, TIMESTAMP '2026-03-02 18:28:48', NULL, 10, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (19, TIMESTAMP '2026-03-02 18:28:48', NULL, 11, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (20, TIMESTAMP '2026-03-02 18:28:48', NULL, 12, 2);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (21, TIMESTAMP '2026-03-02 18:28:48', NULL, 1, 3);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (22, TIMESTAMP '2026-03-02 18:28:48', NULL, 4, 3);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (23, TIMESTAMP '2026-03-02 18:28:48', NULL, 7, 3);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (24, TIMESTAMP '2026-03-02 18:28:48', NULL, 10, 3);
    INSERT INTO `RolePermissions` (`IdRolePermission`, `DateAttribution`, `IdUtilisateurAttribution`, `PermissionId`, `RoleId`)
    VALUES (25, TIMESTAMP '2026-03-02 18:28:48', NULL, 11, 3);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `UserRoles` (`IdUserRole`, `DateAttribution`, `IdUtilisateurAttribution`, `IsPrimary`, `RoleId`, `Statut`, `UtilisateurId`)
    VALUES (1, TIMESTAMP '2026-03-02 18:28:48', NULL, TRUE, 1, TRUE, 1);
    INSERT INTO `UserRoles` (`IdUserRole`, `DateAttribution`, `IdUtilisateurAttribution`, `IsPrimary`, `RoleId`, `Statut`, `UtilisateurId`)
    VALUES (2, TIMESTAMP '2026-03-02 18:28:48', NULL, TRUE, 2, TRUE, 2);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE UNIQUE INDEX `IX_Adhesions_AffilieId` ON `Adhesions` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Adhesions_AgentId` ON `Adhesions` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Adhesions_CategorieAdhesionId` ON `Adhesions` (`CategorieAdhesionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Adhesions_TypeAdhesionId` ON `Adhesions` (`TypeAdhesionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Agents_CategorieAgentId` ON `Agents` (`CategorieAgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Agents_ZoneSocialeId` ON `Agents` (`ZoneSocialeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Antecedants_AffilieId` ON `Antecedants` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_BonsEnvoi_AffilieId` ON `BonsEnvoi` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_BonsEnvoi_PrestationId` ON `BonsEnvoi` (`PrestationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Collectes_AffilieId` ON `Collectes` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Collectes_AgentId` ON `Collectes` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Collectes_DeviseId` ON `Collectes` (`DeviseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Collectes_PrestationId` ON `Collectes` (`PrestationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Communes_ProvinceId` ON `Communes` (`ProvinceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Dependants_AffilieId` ON `Dependants` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_PasswordResetTokens_UtilisateurId` ON `PasswordResetTokens` (`UtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Prestations_ProduitAssureurId` ON `Prestations` (`ProduitAssureurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Prestations_ProduitMutuelId` ON `Prestations` (`ProduitMutuelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_ProduitsAssureurs_AssureurId` ON `ProduitsAssureurs` (`AssureurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_RefreshTokens_UtilisateurId` ON `RefreshTokens` (`UtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_RetraitsAgents_AgentId` ON `RetraitsAgents` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_RetraitsAgents_DeviseIdDevise` ON `RetraitsAgents` (`DeviseIdDevise`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_RolePermissions_PermissionId` ON `RolePermissions` (`PermissionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_RolePermissions_RoleId` ON `RolePermissions` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_SouscriptionsPrestations_AffilieId` ON `SouscriptionsPrestations` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_SouscriptionsPrestations_PrestationId` ON `SouscriptionsPrestations` (`PrestationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_TargetsAgents_AgentId` ON `TargetsAgents` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_UserDevices_UtilisateurId` ON `UserDevices` (`UtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_UserPermissions_PermissionId` ON `UserPermissions` (`PermissionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_UserPermissions_UtilisateurId` ON `UserPermissions` (`UtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_UserRoles_RoleId` ON `UserRoles` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_UserRoles_UtilisateurId` ON `UserRoles` (`UtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Utilisateurs_AffilieId` ON `Utilisateurs` (`AffilieId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Utilisateurs_AgentId` ON `Utilisateurs` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_Utilisateurs_RoleId` ON `Utilisateurs` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_WalletMouvements_CollecteIdCollecte` ON `WalletMouvements` (`CollecteIdCollecte`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_WalletMouvements_WalletId` ON `WalletMouvements` (`WalletId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE UNIQUE INDEX `IX_WalletsAgents_AgentId` ON `WalletsAgents` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    CREATE INDEX `IX_ZonesSociales_CommuneId` ON `ZonesSociales` (`CommuneId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302162849_InitialCreateDb') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260302162849_InitialCreateDb', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    SET @col_exists := (
        SELECT COUNT(*)
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'TypeAdhesions'
          AND COLUMN_NAME = 'CategorieAdhesionId'
    );
    SET @sql := IF(@col_exists = 0,
        'ALTER TABLE `TypeAdhesions` ADD COLUMN `CategorieAdhesionId` int NOT NULL DEFAULT 0',
        'SELECT 1'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    INSERT INTO `CategoriesAdhesions`
    (`IdCategorieAdhesion`, `DateCreation`, `DateModification`, `Description`, `Libelle`, `Statut`)
    SELECT 1, NOW(6), NULL, 'Catégorie par défaut', 'Par défaut', TRUE
    WHERE NOT EXISTS (SELECT 1 FROM `CategoriesAdhesions` WHERE `IdCategorieAdhesion` = 1);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdDevise` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdDevise` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdPermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 13;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 14;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 15;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 16;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 17;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 18;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 19;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 20;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 21;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 22;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 23;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 24;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRolePermission` = 25;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRole` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRole` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRole` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdRole` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdUserRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `IdUserRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26', `MotDePasseHash` = '$2a$11$6wNoyWcqaD8xfatoRthTbOVUCO8mgVShhYZOfZ7SvN7lFWPtiAmJa'
    WHERE `IdUtilisateur` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-02 19:22:26', `MotDePasseHash` = '$2a$11$UuemcFsD8fY.sgfHj5OslOULtYDtMzf1QYxz0FtGV01QqZa1.0kTi'
    WHERE `IdUtilisateur` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `TypeAdhesions` SET `CategorieAdhesionId` = 1, `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `TypeAdhesions` SET `CategorieAdhesionId` = 1, `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `TypeAdhesions` SET `CategorieAdhesionId` = 1, `DateCreation` = TIMESTAMP '2026-03-02 19:22:26'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    UPDATE `TypeAdhesions` SET `CategorieAdhesionId` = 1 WHERE `CategorieAdhesionId` = 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    CREATE INDEX `IX_TypeAdhesions_CategorieAdhesionId` ON `TypeAdhesions` (`CategorieAdhesionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    ALTER TABLE `TypeAdhesions` ADD CONSTRAINT `FK_TypeAdhesions_CategoriesAdhesions_CategorieAdhesionId` FOREIGN KEY (`CategorieAdhesionId`) REFERENCES `CategoriesAdhesions` (`IdCategorieAdhesion`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302172228_AddCategorieAdhesionToTypeAdhesion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260302172228_AddCategorieAdhesionToTypeAdhesion', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    ALTER TABLE `Adhesions` DROP FOREIGN KEY `FK_Adhesions_CategoriesAdhesions_CategorieAdhesionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    ALTER TABLE `Adhesions` DROP INDEX `IX_Adhesions_CategorieAdhesionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    ALTER TABLE `Adhesions` DROP COLUMN `CategorieAdhesionId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `CategoriesAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdCategorieAdhesion` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdDevise` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdDevise` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdPermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 13;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 14;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 15;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 16;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 17;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 18;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 19;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 20;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 21;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 22;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 23;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 24;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRolePermission` = 25;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRole` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRole` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRole` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdRole` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdUserRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:13:54'
    WHERE `IdUserRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54', `MotDePasseHash` = '$2a$11$V2U.s6KIfomFJtksdBph1uFYKF/lWPMREjfdgAPU.y4cwMoaLPTS.'
    WHERE `IdUtilisateur` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-02 21:13:54', `MotDePasseHash` = '$2a$11$KrnrSS.FvRIy/6ubquPzzeNlQm69/PW6n93MsM6cNMWtOu7fw0vW6'
    WHERE `IdUtilisateur` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302191355_RemoveCategorieAdhesionFromAdhesion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260302191355_RemoveCategorieAdhesionFromAdhesion', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `AvenueActivite` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `AvenueResidence` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `CommuneActivite` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `CommuneResidence` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `NumeroActivite` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `NumeroResidence` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `Postnom` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `ProvinceResidence` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `QuartierActivite` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    ALTER TABLE `Affilies` ADD `QuartierResidence` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `CategoriesAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdCategorieAdhesion` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdDevise` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdDevise` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdPermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 13;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 14;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 15;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 16;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 17;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 18;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 19;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 20;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 21;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 22;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 23;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 24;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRolePermission` = 25;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRole` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRole` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRole` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdRole` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdUserRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-02 21:37:13'
    WHERE `IdUserRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13', `MotDePasseHash` = '$2a$11$6OiCqd8sJtswY9Az4wu0HOwBiUGZpU/A9zMsD2cHfz5OpILDmOepS'
    WHERE `IdUtilisateur` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-02 21:37:13', `MotDePasseHash` = '$2a$11$4SInQgpJ/IZI/928opMKkuZyeXLrw2JFP1YGtK7aoUoNfbHHiUmHK'
    WHERE `IdUtilisateur` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302193714_AddAffilieResidenceAndActivityFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260302193714_AddAffilieResidenceAndActivityFields', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303101956_AddCodesAdhesionSequences') THEN

    CREATE TABLE `CodesAdhesionSequences` (
        `Prefix` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `NextValue` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_CodesAdhesionSequences` PRIMARY KEY (`Prefix`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303101956_AddCodesAdhesionSequences') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260303101956_AddCodesAdhesionSequences', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `ModePaiement` varchar(20) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `MontantAttendu` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `MontantRecu` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `Operateur` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `ReferencePaiement` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `SouscriptionPrestationId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD `StatutPaiement` varchar(20) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    CREATE UNIQUE INDEX `IX_Collectes_ReferencePaiement` ON `Collectes` (`ReferencePaiement`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    CREATE INDEX `IX_Collectes_SouscriptionPrestationId` ON `Collectes` (`SouscriptionPrestationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationId` FOREIGN KEY (`SouscriptionPrestationId`) REFERENCES `SouscriptionsPrestations` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303113429_AddCollectePaymentFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260303113429_AddCollectePaymentFields', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    ALTER TABLE `Agents` DROP FOREIGN KEY `FK_Agents_ZonesSociales_ZoneSocialeId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    ALTER TABLE `Agents` MODIFY COLUMN `ZoneSocialeId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    ALTER TABLE `Agents` MODIFY COLUMN `CodeAT` varchar(20) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `CategoriesAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdCategorieAdhesion` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdDevise` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Devises` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdDevise` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Permissions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdPermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Provinces` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 10;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 11;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 12;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 13;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 14;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 15;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 16;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 17;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 18;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 19;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 20;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 21;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 22;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 23;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 24;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `RolePermissions` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdRolePermission` = 25;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdRole` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdRole` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdRole` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Roles` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `IdRole` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `TypeAdhesions` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:00'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdUserRole` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `UserRoles` SET `DateAttribution` = TIMESTAMP '2026-03-03 16:16:01'
    WHERE `IdUserRole` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01', `MotDePasseHash` = '$2a$11$KlhUvt0MtjpsZ3sPHGgt..PYKFzPm.pkcvPO0PfmDRVlJA90FDU36'
    WHERE `IdUtilisateur` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    UPDATE `Utilisateurs` SET `DateCreation` = TIMESTAMP '2026-03-03 16:16:01', `MotDePasseHash` = '$2a$11$Yz/F2m5l3.SIGp9DR0JdnuphR4en9tUi6I6E8rf.tUFHkQMrIWx3i'
    WHERE `IdUtilisateur` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    CREATE UNIQUE INDEX `IX_Agents_CodeAT` ON `Agents` (`CodeAT`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    ALTER TABLE `Agents` ADD CONSTRAINT `FK_Agents_ZonesSociales_ZoneSocialeId` FOREIGN KEY (`ZoneSocialeId`) REFERENCES `ZonesSociales` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303141603_MakeAgentCodeATAndZoneNullable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260303141603_MakeAgentCodeATAndZoneNullable', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

