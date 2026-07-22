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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `Affilies` (
        `IdAffilie` int NOT NULL AUTO_INCREMENT,
        `CodeAdhesion` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Prenom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `DateNaissance` datetime(6) NOT NULL,
        `Telephone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Postnom` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ProvinceResidence` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CommuneResidence` varchar(100) CHARACTER SET utf8mb4 NULL,
        `QuartierResidence` varchar(100) CHARACTER SET utf8mb4 NULL,
        `AvenueResidence` varchar(100) CHARACTER SET utf8mb4 NULL,
        `NumeroResidence` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CommuneActivite` varchar(100) CHARACTER SET utf8mb4 NULL,
        `QuartierActivite` varchar(100) CHARACTER SET utf8mb4 NULL,
        `AvenueActivite` varchar(100) CHARACTER SET utf8mb4 NULL,
        `NumeroActivite` varchar(50) CHARACTER SET utf8mb4 NULL,
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `Roles` (
        `IdRole` int NOT NULL AUTO_INCREMENT,
        `Nom` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Code` varchar(50) CHARACTER SET utf8mb4 NULL,
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `TypeAdhesions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Libelle` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `MaxDependants` int NOT NULL,
        `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
        `Montant` decimal(18,2) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `CategorieAdhesionId` int NOT NULL,
        CONSTRAINT `PK_TypeAdhesions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TypeAdhesions_CategoriesAdhesions_CategorieAdhesionId` FOREIGN KEY (`CategorieAdhesionId`) REFERENCES `CategoriesAdhesions` (`IdCategorieAdhesion`) ON DELETE RESTRICT
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `SouscriptionsPrestations` (
        `IdSouscriptionPrestation` int NOT NULL AUTO_INCREMENT,
        `AffilieId` int NOT NULL,
        `PrestationId` int NOT NULL,
        `DateSouscription` datetime(6) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        CONSTRAINT `PK_SouscriptionsPrestations` PRIMARY KEY (`IdSouscriptionPrestation`),
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `Agents` (
        `IdAgent` int NOT NULL AUTO_INCREMENT,
        `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Matricule` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `EmailAgent` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Fonction` varchar(100) CHARACTER SET utf8mb4 NULL,
        `RoleAgent` varchar(100) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `CategorieAgentId` int NULL,
        `ZoneSocialeId` int NULL,
        `SuperviseurId` int NULL,
        CONSTRAINT `PK_Agents` PRIMARY KEY (`IdAgent`),
        CONSTRAINT `FK_Agents_Agents_SuperviseurId` FOREIGN KEY (`SuperviseurId`) REFERENCES `Agents` (`IdAgent`),
        CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`),
        CONSTRAINT `FK_Agents_ZonesSociales_ZoneSocialeId` FOREIGN KEY (`ZoneSocialeId`) REFERENCES `ZonesSociales` (`Id`) ON DELETE SET NULL
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `Adhesions` (
        `IdAdhesion` int NOT NULL AUTO_INCREMENT,
        `StatutDossier` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `AgentId` int NOT NULL,
        `AffilieId` int NOT NULL,
        `TypeAdhesionId` int NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Adhesions` PRIMARY KEY (`IdAdhesion`),
        CONSTRAINT `FK_Adhesions_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
        CONSTRAINT `FK_Adhesions_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `Collectes` (
        `IdCollecte` int NOT NULL AUTO_INCREMENT,
        `AffilieId` int NOT NULL,
        `AgentId` int NOT NULL,
        `Montant` decimal(18,2) NOT NULL,
        `ReferencePaiement` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ModePaiement` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Operateur` varchar(50) CHARACTER SET utf8mb4 NULL,
        `StatutPaiement` varchar(20) CHARACTER SET utf8mb4 NULL,
        `SouscriptionPrestationId` int NULL,
        `MontantRecu` decimal(18,2) NULL,
        `MontantAttendu` decimal(18,2) NULL,
        `DeviseId` int NOT NULL,
        `DateCollecte` datetime(6) NOT NULL,
        `Observation` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        `PrestationId` int NULL,
        `SouscriptionPrestationIdSouscriptionPrestation` int NULL,
        CONSTRAINT `PK_Collectes` PRIMARY KEY (`IdCollecte`),
        CONSTRAINT `FK_Collectes_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
        CONSTRAINT `FK_Collectes_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
        CONSTRAINT `FK_Collectes_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE CASCADE,
        CONSTRAINT `FK_Collectes_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`Id`),
        CONSTRAINT `FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationId` FOREIGN KEY (`SouscriptionPrestationId`) REFERENCES `SouscriptionsPrestations` (`IdSouscriptionPrestation`) ON DELETE SET NULL,
        CONSTRAINT `FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationIdS~` FOREIGN KEY (`SouscriptionPrestationIdSouscriptionPrestation`) REFERENCES `SouscriptionsPrestations` (`IdSouscriptionPrestation`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `Utilisateurs` (
        `IdUtilisateur` int NOT NULL AUTO_INCREMENT,
        `ReferenceUtilisateur` char(36) COLLATE ascii_general_ci NULL,
        `NomUtilisateur` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `EmailUtilisateur` varchar(200) CHARACTER SET utf8mb4 NULL,
        `PhoneUtilisateur` varchar(30) CHARACTER SET utf8mb4 NULL,
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `WalletsAgents` (
        `IdWalletAgent` int NOT NULL AUTO_INCREMENT,
        `AgentId` int NOT NULL,
        `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `WalletsVirtuelsAgents` (
        `IdWalletVirtuelAgent` int NOT NULL AUTO_INCREMENT,
        `AgentId` int NOT NULL,
        `SoldeVirtuel` decimal(18,2) NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_WalletsVirtuelsAgents` PRIMARY KEY (`IdWalletVirtuelAgent`),
        CONSTRAINT `FK_WalletsVirtuelsAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE TABLE `WalletMouvements` (
        `IdWalletMouvement` int NOT NULL AUTO_INCREMENT,
        `WalletId` int NOT NULL,
        `Montant` decimal(18,2) NOT NULL,
        `TypeOperation` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Source` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateOperation` datetime(6) NOT NULL,
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Agents_Matricule` ON `Agents` (`Matricule`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE INDEX `IX_Agents_SuperviseurId` ON `Agents` (`SuperviseurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE INDEX `IX_Collectes_SouscriptionPrestationIdSouscriptionPrestation` ON `Collectes` (`SouscriptionPrestationIdSouscriptionPrestation`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Utilisateurs_EmailUtilisateur` ON `Utilisateurs` (`EmailUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Utilisateurs_PhoneUtilisateur` ON `Utilisateurs` (`PhoneUtilisateur`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_WalletsVirtuelsAgents_AgentId` ON `WalletsVirtuelsAgents` (`AgentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309105020_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309105020_InitialCreate', '6.0.25');

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309115918_AddDeviseIdToProduitMutuel') THEN

    ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309115918_AddDeviseIdToProduitMutuel') THEN

    ALTER TABLE `ProduitsMutuels` ADD `DeviseId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309115918_AddDeviseIdToProduitMutuel') THEN

    CREATE INDEX `IX_ProduitsMutuels_DeviseId` ON `ProduitsMutuels` (`DeviseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309115918_AddDeviseIdToProduitMutuel') THEN

    ALTER TABLE `ProduitsMutuels` ADD CONSTRAINT `FK_ProduitsMutuels_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309115918_AddDeviseIdToProduitMutuel') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309115918_AddDeviseIdToProduitMutuel', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

