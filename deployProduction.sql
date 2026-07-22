CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Affilies` (
    `IdAffilie` int NOT NULL AUTO_INCREMENT,
    `CodeAdhesion` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Prenom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `DateNaissance` datetime(6) NOT NULL,
    `Telephone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `EmailAffilie` varchar(150) CHARACTER SET utf8mb4 NULL,
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
    `PhotoUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Affilies` PRIMARY KEY (`IdAffilie`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Assureurs` (
    `IdAssureur` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Assureurs` PRIMARY KEY (`IdAssureur`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `CategoriesAdhesions` (
    `IdCategorieAdhesion` int NOT NULL AUTO_INCREMENT,
    `Libelle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriesAdhesions` PRIMARY KEY (`IdCategorieAdhesion`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `CategoriesAgents` (
    `IdCategorieAgent` int NOT NULL AUTO_INCREMENT,
    `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_CategoriesAgents` PRIMARY KEY (`IdCategorieAgent`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `CodesAdhesionSequences` (
    `Prefix` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `NextValue` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_CodesAdhesionSequences` PRIMARY KEY (`Prefix`)
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `HopitalPartenaires` (
    `IdHopital` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Adresse` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Telephone` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(200) CHARACTER SET utf8mb4 NULL,
    `ContactPersonne` varchar(100) CHARACTER SET utf8mb4 NULL,
    `CodeAcces` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Niveau` varchar(20) CHARACTER SET utf8mb4 NULL,
    `EstActif` tinyint(1) NOT NULL,
    `ServicesOfferts` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `PlafondJournalier` decimal(18,2) NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_HopitalPartenaires` PRIMARY KEY (`IdHopital`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `MobileAppConfigs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AppName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Platform` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Version` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `BuildNumber` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `AppStoreUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `PlayStoreUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `UpdateMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `IsForceUpdateRequired` tinyint(1) NOT NULL,
    `IsMaintenanceMode` tinyint(1) NOT NULL,
    `MaintenanceStart` datetime(6) NULL,
    `MaintenanceEnd` datetime(6) NULL,
    `MaintenanceMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `MinSupportedVersion` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_MobileAppConfigs` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `NotificationTypes` (
    `IdNotificationType` int NOT NULL AUTO_INCREMENT,
    `Code` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Categorie` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Couleur` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Icône` longtext CHARACTER SET utf8mb4 NOT NULL,
    `EstActif` tinyint(1) NOT NULL,
    `Priorite` int NOT NULL,
    `EmailParDefaut` tinyint(1) NOT NULL,
    `SmsParDefaut` tinyint(1) NOT NULL,
    `PushParDefaut` tinyint(1) NOT NULL,
    `InAppParDefaut` tinyint(1) NOT NULL,
    `TemplateMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_NotificationTypes` PRIMARY KEY (`IdNotificationType`)
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `Provinces` (
    `IdProvince` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_Provinces` PRIMARY KEY (`IdProvince`)
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `UserNotificationPreferences` (
    `IdUserNotificationPreference` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `EmailNotification` tinyint(1) NOT NULL,
    `SmsNotification` tinyint(1) NOT NULL,
    `PushNotification` tinyint(1) NOT NULL,
    `InAppNotification` tinyint(1) NOT NULL,
    `CommissionEmail` tinyint(1) NOT NULL,
    `CommissionSms` tinyint(1) NOT NULL,
    `CommissionPush` tinyint(1) NOT NULL,
    `CommissionInApp` tinyint(1) NOT NULL,
    `MinCommissionAmount` decimal(18,2) NOT NULL,
    `CommissionCurrency` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `CommissionMessageTemplate` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Language` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Timezone` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `QuietHoursEnabled` tinyint(1) NOT NULL,
    `QuietHoursStart` int NOT NULL,
    `QuietHoursEnd` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_UserNotificationPreferences` PRIMARY KEY (`IdUserNotificationPreference`)
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `Dependants` (
    `IdDependant` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `LienParente` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `DateNaissance` datetime(6) NULL,
    `Telephone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `AffilieId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Dependants` PRIMARY KEY (`IdDependant`),
    CONSTRAINT `FK_Dependants_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `TypeAdhesions` (
    `IdTypeAdhesion` int NOT NULL AUTO_INCREMENT,
    `Libelle` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `MaxDependants` int NOT NULL,
    `Description` varchar(255) CHARACTER SET utf8mb4 NULL,
    `Montant` decimal(18,2) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `CategorieAdhesionId` int NOT NULL,
    CONSTRAINT `PK_TypeAdhesions` PRIMARY KEY (`IdTypeAdhesion`),
    CONSTRAINT `FK_TypeAdhesions_CategoriesAdhesions_CategorieAdhesionId` FOREIGN KEY (`CategorieAdhesionId`) REFERENCES `CategoriesAdhesions` (`IdCategorieAdhesion`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `ProduitsAssureurs` (
    `IdProduit` int NOT NULL AUTO_INCREMENT,
    `AssureurId` int NOT NULL,
    `DeviseId` int NOT NULL,
    `CommissionMutuelle` decimal(18,2) NOT NULL,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `PrixMensuel` decimal(18,2) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    CONSTRAINT `PK_ProduitsAssureurs` PRIMARY KEY (`IdProduit`),
    CONSTRAINT `FK_ProduitsAssureurs_Assureurs_AssureurId` FOREIGN KEY (`AssureurId`) REFERENCES `Assureurs` (`IdAssureur`) ON DELETE CASCADE,
    CONSTRAINT `FK_ProduitsAssureurs_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `ProduitsMutuels` (
    `IdProduit` int NOT NULL AUTO_INCREMENT,
    `DeviseId` int NOT NULL,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `PrixMensuel` decimal(18,2) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    CONSTRAINT `PK_ProduitsMutuels` PRIMARY KEY (`IdProduit`),
    CONSTRAINT `FK_ProduitsMutuels_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `JetonsMedicaux` (
    `IdJeton` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `CodeJeton` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `DateEmission` datetime(6) NOT NULL,
    `DateUtilisation` datetime(6) NULL,
    `DateExpiration` datetime(6) NULL,
    `EstValide` tinyint(1) NOT NULL,
    `EstUtilise` tinyint(1) NOT NULL,
    `HopitalPartenaireId` int NULL,
    `Observation` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_JetonsMedicaux` PRIMARY KEY (`IdJeton`),
    CONSTRAINT `FK_JetonsMedicaux_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_JetonsMedicaux_HopitalPartenaires_HopitalPartenaireId` FOREIGN KEY (`HopitalPartenaireId`) REFERENCES `HopitalPartenaires` (`IdHopital`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Notifications` (
    `IdNotification` int NOT NULL AUTO_INCREMENT,
    `Titre` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Message` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
    `Type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `TypeNotificationId` int NULL,
    `EnvoyeurId` int NULL,
    `RecepteurId` int NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateLecture` datetime(6) NULL,
    `EstLu` tinyint(1) NOT NULL,
    `Priorite` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Categorie` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Couleur` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Icône` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Métadonnées` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `DateEnvoiEmail` datetime(6) NULL,
    `DateEnvoiSms` datetime(6) NULL,
    `DateEnvoiPush` datetime(6) NULL,
    `EmailEnvoyé` tinyint(1) NOT NULL,
    `SmsEnvoyé` tinyint(1) NOT NULL,
    `PushEnvoyé` tinyint(1) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Notifications` PRIMARY KEY (`IdNotification`),
    CONSTRAINT `FK_Notifications_NotificationTypes_TypeNotificationId` FOREIGN KEY (`TypeNotificationId`) REFERENCES `NotificationTypes` (`IdNotificationType`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Communes` (
    `IdCommune` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `ProvinceId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Communes` PRIMARY KEY (`IdCommune`),
    CONSTRAINT `FK_Communes_Provinces_ProvinceId` FOREIGN KEY (`ProvinceId`) REFERENCES `Provinces` (`IdProvince`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `Prestations` (
    `IdPrestation` int NOT NULL AUTO_INCREMENT,
    `NomPrestation` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `ProduitMutuelId` int NULL,
    `ProduitAssureurId` int NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    `HopitalPartenaireIdHopital` int NULL,
    CONSTRAINT `PK_Prestations` PRIMARY KEY (`IdPrestation`),
    CONSTRAINT `FK_Prestations_HopitalPartenaires_HopitalPartenaireIdHopital` FOREIGN KEY (`HopitalPartenaireIdHopital`) REFERENCES `HopitalPartenaires` (`IdHopital`),
    CONSTRAINT `FK_Prestations_ProduitsAssureurs_ProduitAssureurId` FOREIGN KEY (`ProduitAssureurId`) REFERENCES `ProduitsAssureurs` (`IdProduit`),
    CONSTRAINT `FK_Prestations_ProduitsMutuels_ProduitMutuelId` FOREIGN KEY (`ProduitMutuelId`) REFERENCES `ProduitsMutuels` (`IdProduit`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ZonesSociales` (
    `IdZoneSociale` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `CommuneId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_ZonesSociales` PRIMARY KEY (`IdZoneSociale`),
    CONSTRAINT `FK_ZonesSociales_Communes_CommuneId` FOREIGN KEY (`CommuneId`) REFERENCES `Communes` (`IdCommune`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

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
    CONSTRAINT `FK_BonsEnvoi_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`IdPrestation`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `SouscriptionsArrierees` (
    `IdSouscriptionsArrierees` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `PrestationId` int NOT NULL,
    `MontantAttendu` decimal(18,2) NOT NULL,
    `MontantPaye` decimal(18,2) NOT NULL,
    `RestAPayer` decimal(18,2) NOT NULL,
    `Periode` varchar(7) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `DateDernierPaiement` datetime(6) NULL,
    `StatutPaiement` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_SouscriptionsArrierees` PRIMARY KEY (`IdSouscriptionsArrierees`),
    CONSTRAINT `FK_SouscriptionsArrierees_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_SouscriptionsArrierees_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`IdPrestation`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

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
    CONSTRAINT `FK_SouscriptionsPrestations_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`IdPrestation`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Agents` (
    `IdAgent` int NOT NULL AUTO_INCREMENT,
    `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Matricule` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Phone` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `EmailAgent` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Fonction` varchar(100) CHARACTER SET utf8mb4 NULL,
    `RoleAgent` varchar(100) CHARACTER SET utf8mb4 NULL,
    `PhotoUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    `CategorieAgentId` int NULL,
    `ZoneSocialeId` int NULL,
    `SuperviseurId` int NULL,
    CONSTRAINT `PK_Agents` PRIMARY KEY (`IdAgent`),
    CONSTRAINT `FK_Agents_Agents_SuperviseurId` FOREIGN KEY (`SuperviseurId`) REFERENCES `Agents` (`IdAgent`),
    CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`),
    CONSTRAINT `FK_Agents_ZonesSociales_ZoneSocialeId` FOREIGN KEY (`ZoneSocialeId`) REFERENCES `ZonesSociales` (`IdZoneSociale`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `DemandesBonEnvoi` (
    `IdDemande` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `PrestationId` int NOT NULL,
    `TypeDemande` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MotifDemande` varchar(500) CHARACTER SET utf8mb4 NULL,
    `AgentId` int NOT NULL,
    `ObservationAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateDemande` datetime(6) NOT NULL,
    `DateValidation` datetime(6) NULL,
    `StatutDemande` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `BonEnvoiId` int NULL,
    `JetonMedicalId` int NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_DemandesBonEnvoi` PRIMARY KEY (`IdDemande`),
    CONSTRAINT `FK_DemandesBonEnvoi_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_DemandesBonEnvoi_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
    CONSTRAINT `FK_DemandesBonEnvoi_BonsEnvoi_BonEnvoiId` FOREIGN KEY (`BonEnvoiId`) REFERENCES `BonsEnvoi` (`IdBonEnvoi`),
    CONSTRAINT `FK_DemandesBonEnvoi_JetonsMedicaux_JetonMedicalId` FOREIGN KEY (`JetonMedicalId`) REFERENCES `JetonsMedicaux` (`IdJeton`),
    CONSTRAINT `FK_DemandesBonEnvoi_Prestations_PrestationId` FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`IdPrestation`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `WalletsAgents` (
    `IdWalletAgent` int NOT NULL AUTO_INCREMENT,
    `AgentId` int NOT NULL,
    `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    `SoldeCourant` decimal(18,2) NOT NULL,
    `SoldeDisponible` decimal(18,2) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_WalletsAgents` PRIMARY KEY (`IdWalletAgent`),
    CONSTRAINT `FK_WalletsAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `Adhesions` (
    `IdAdhesion` int NOT NULL AUTO_INCREMENT,
    `StatutDossier` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `AgentId` int NOT NULL,
    `AffilieId` int NOT NULL,
    `TypeAdhesionId` int NOT NULL,
    `UtilisateurId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    `AffilieIdAffilie` int NULL,
    CONSTRAINT `PK_Adhesions` PRIMARY KEY (`IdAdhesion`),
    CONSTRAINT `FK_Adhesions_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_Adhesions_Affilies_AffilieIdAffilie` FOREIGN KEY (`AffilieIdAffilie`) REFERENCES `Affilies` (`IdAffilie`),
    CONSTRAINT `FK_Adhesions_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
    CONSTRAINT `FK_Adhesions_TypeAdhesions_TypeAdhesionId` FOREIGN KEY (`TypeAdhesionId`) REFERENCES `TypeAdhesions` (`IdTypeAdhesion`) ON DELETE CASCADE,
    CONSTRAINT `FK_Adhesions_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Frais` (
    `IdFrais` int NOT NULL AUTO_INCREMENT,
    `Libelle` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Montant` decimal(18,2) NOT NULL,
    `DeviseId` int NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    `CreeParId` int NULL,
    `ModifieParId` int NULL,
    `DateSuppression` datetime(6) NULL,
    `EstSupprime` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Frais` PRIMARY KEY (`IdFrais`),
    CONSTRAINT `FK_Frais_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Frais_Utilisateurs_CreeParId` FOREIGN KEY (`CreeParId`) REFERENCES `Utilisateurs` (`IdUtilisateur`),
    CONSTRAINT `FK_Frais_Utilisateurs_ModifieParId` FOREIGN KEY (`ModifieParId`) REFERENCES `Utilisateurs` (`IdUtilisateur`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `MobileSyncData` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UtilisateurId` int NOT NULL,
    `EntityType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `EntityId` int NOT NULL,
    `Operation` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Data` json NOT NULL,
    `SyncStatus` varchar(50) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateSynchronisation` datetime(6) NULL,
    `DateDerniereTentative` datetime(6) NULL,
    `NombreTentatives` int NOT NULL,
    `ErreurMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `EstSynchronise` tinyint(1) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_MobileSyncData` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_MobileSyncData_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `MobileUserSessions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UtilisateurId` int NOT NULL,
    `SessionToken` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `DeviceId` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Platform` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `AppVersion` varchar(100) CHARACTER SET utf8mb4 NULL,
    `OsVersion` varchar(100) CHARACTER SET utf8mb4 NULL,
    `IpAddress` varchar(50) CHARACTER SET utf8mb4 NULL,
    `UserAgent` varchar(100) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateDerniereActivite` datetime(6) NOT NULL,
    `DateExpiration` datetime(6) NOT NULL,
    `EstActive` tinyint(1) NOT NULL,
    `EstBiometricAuth` tinyint(1) NOT NULL,
    `NombreRequetes` int NOT NULL,
    `Metadata` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `DateDerniereSynchronisation` datetime(6) NULL,
    `EstModeHorsLigne` tinyint(1) NOT NULL,
    CONSTRAINT `PK_MobileUserSessions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_MobileUserSessions_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `Collectes` (
    `IdCollecte` int NOT NULL AUTO_INCREMENT,
    `TypeCollecte` int NOT NULL,
    `FraisId` int NULL,
    `AffilieId` int NOT NULL,
    `AgentId` int NOT NULL,
    `Montant` decimal(18,2) NOT NULL,
    `Mois` int NOT NULL,
    `Annee` int NOT NULL,
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
    `PrestationIdPrestation` int NULL,
    `SouscriptionPrestationIdSouscriptionPrestation` int NULL,
    `SouscriptionsArriereesIdSouscriptionsArrierees` int NULL,
    CONSTRAINT `PK_Collectes` PRIMARY KEY (`IdCollecte`),
    CONSTRAINT `FK_Collectes_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_Collectes_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
    CONSTRAINT `FK_Collectes_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE CASCADE,
    CONSTRAINT `FK_Collectes_Frais_FraisId` FOREIGN KEY (`FraisId`) REFERENCES `Frais` (`IdFrais`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Collectes_Prestations_PrestationIdPrestation` FOREIGN KEY (`PrestationIdPrestation`) REFERENCES `Prestations` (`IdPrestation`),
    CONSTRAINT `FK_Collectes_SouscriptionsArrierees_SouscriptionsArriereesIdSou~` FOREIGN KEY (`SouscriptionsArriereesIdSouscriptionsArrierees`) REFERENCES `SouscriptionsArrierees` (`IdSouscriptionsArrierees`),
    CONSTRAINT `FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationId` FOREIGN KEY (`SouscriptionPrestationId`) REFERENCES `SouscriptionsPrestations` (`IdSouscriptionPrestation`) ON DELETE SET NULL,
    CONSTRAINT `FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationIdS~` FOREIGN KEY (`SouscriptionPrestationIdSouscriptionPrestation`) REFERENCES `SouscriptionsPrestations` (`IdSouscriptionPrestation`)
) CHARACTER SET=utf8mb4;

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

CREATE TABLE `DemandesRetraitAgents` (
    `IdDemande` int NOT NULL AUTO_INCREMENT,
    `AgentId` int NOT NULL,
    `MontantDemande` decimal(18,2) NOT NULL,
    `TypeRetrait` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `StatutDemande` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MotifRetrait` varchar(500) CHARACTER SET utf8mb4 NULL,
    `MotifRejet` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateDemande` datetime(6) NOT NULL,
    `DateValidation` datetime(6) NULL,
    `DateTraitement` datetime(6) NULL,
    `AgentValidationId` int NULL,
    `JetonRetraitId` int NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_DemandesRetraitAgents` PRIMARY KEY (`IdDemande`),
    CONSTRAINT `FK_DemandesRetraitAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
    CONSTRAINT `FK_DemandesRetraitAgents_Agents_AgentValidationId` FOREIGN KEY (`AgentValidationId`) REFERENCES `Agents` (`IdAgent`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `JetonsRetraits` (
    `IdJeton` int NOT NULL AUTO_INCREMENT,
    `AgentId` int NOT NULL,
    `DemandeRetraitId` int NOT NULL,
    `CodeJeton` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MontantRetrait` decimal(18,2) NOT NULL,
    `DateEmission` datetime(6) NOT NULL,
    `DateUtilisation` datetime(6) NULL,
    `DateExpiration` datetime(6) NOT NULL,
    `EstValide` tinyint(1) NOT NULL,
    `EstUtilise` tinyint(1) NOT NULL,
    `ObservationUtilisation` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_JetonsRetraits` PRIMARY KEY (`IdJeton`),
    CONSTRAINT `FK_JetonsRetraits_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE,
    CONSTRAINT `FK_JetonsRetraits_DemandesRetraitAgents_DemandeRetraitId` FOREIGN KEY (`DemandeRetraitId`) REFERENCES `DemandesRetraitAgents` (`IdDemande`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Adhesions_AffilieId` ON `Adhesions` (`AffilieId`);

CREATE INDEX `IX_Adhesions_AffilieIdAffilie` ON `Adhesions` (`AffilieIdAffilie`);

CREATE INDEX `IX_Adhesions_AgentId` ON `Adhesions` (`AgentId`);

CREATE INDEX `IX_Adhesions_TypeAdhesionId` ON `Adhesions` (`TypeAdhesionId`);

CREATE INDEX `IX_Adhesions_UtilisateurId` ON `Adhesions` (`UtilisateurId`);

CREATE INDEX `IX_Agents_CategorieAgentId` ON `Agents` (`CategorieAgentId`);

CREATE UNIQUE INDEX `IX_Agents_Matricule` ON `Agents` (`Matricule`);

CREATE INDEX `IX_Agents_SuperviseurId` ON `Agents` (`SuperviseurId`);

CREATE INDEX `IX_Agents_ZoneSocialeId` ON `Agents` (`ZoneSocialeId`);

CREATE INDEX `IX_Antecedants_AffilieId` ON `Antecedants` (`AffilieId`);

CREATE INDEX `IX_BonsEnvoi_AffilieId` ON `BonsEnvoi` (`AffilieId`);

CREATE INDEX `IX_BonsEnvoi_PrestationId` ON `BonsEnvoi` (`PrestationId`);

CREATE INDEX `IX_Collectes_AffilieId` ON `Collectes` (`AffilieId`);

CREATE INDEX `IX_Collectes_AgentId` ON `Collectes` (`AgentId`);

CREATE INDEX `IX_Collectes_DeviseId` ON `Collectes` (`DeviseId`);

CREATE INDEX `IX_Collectes_FraisId` ON `Collectes` (`FraisId`);

CREATE INDEX `IX_Collectes_PrestationIdPrestation` ON `Collectes` (`PrestationIdPrestation`);

CREATE UNIQUE INDEX `IX_Collectes_ReferencePaiement` ON `Collectes` (`ReferencePaiement`);

CREATE INDEX `IX_Collectes_SouscriptionPrestationId` ON `Collectes` (`SouscriptionPrestationId`);

CREATE INDEX `IX_Collectes_SouscriptionPrestationIdSouscriptionPrestation` ON `Collectes` (`SouscriptionPrestationIdSouscriptionPrestation`);

CREATE INDEX `IX_Collectes_SouscriptionsArriereesIdSouscriptionsArrierees` ON `Collectes` (`SouscriptionsArriereesIdSouscriptionsArrierees`);

CREATE INDEX `IX_Communes_ProvinceId` ON `Communes` (`ProvinceId`);

CREATE INDEX `IX_DemandesBonEnvoi_AffilieId` ON `DemandesBonEnvoi` (`AffilieId`);

CREATE INDEX `IX_DemandesBonEnvoi_AgentId` ON `DemandesBonEnvoi` (`AgentId`);

CREATE INDEX `IX_DemandesBonEnvoi_BonEnvoiId` ON `DemandesBonEnvoi` (`BonEnvoiId`);

CREATE INDEX `IX_DemandesBonEnvoi_JetonMedicalId` ON `DemandesBonEnvoi` (`JetonMedicalId`);

CREATE INDEX `IX_DemandesBonEnvoi_PrestationId` ON `DemandesBonEnvoi` (`PrestationId`);

CREATE INDEX `IX_DemandesRetraitAgents_AgentId` ON `DemandesRetraitAgents` (`AgentId`);

CREATE INDEX `IX_DemandesRetraitAgents_AgentValidationId` ON `DemandesRetraitAgents` (`AgentValidationId`);

CREATE INDEX `IX_DemandesRetraitAgents_JetonRetraitId` ON `DemandesRetraitAgents` (`JetonRetraitId`);

CREATE INDEX `IX_Dependants_AffilieId` ON `Dependants` (`AffilieId`);

CREATE INDEX `IX_Frais_CreeParId` ON `Frais` (`CreeParId`);

CREATE INDEX `IX_Frais_DeviseId` ON `Frais` (`DeviseId`);

CREATE INDEX `IX_Frais_ModifieParId` ON `Frais` (`ModifieParId`);

CREATE INDEX `IX_JetonsMedicaux_AffilieId` ON `JetonsMedicaux` (`AffilieId`);

CREATE INDEX `IX_JetonsMedicaux_HopitalPartenaireId` ON `JetonsMedicaux` (`HopitalPartenaireId`);

CREATE INDEX `IX_JetonsRetraits_AgentId` ON `JetonsRetraits` (`AgentId`);

CREATE INDEX `IX_JetonsRetraits_DemandeRetraitId` ON `JetonsRetraits` (`DemandeRetraitId`);

CREATE INDEX `IX_MobileSyncData_UtilisateurId` ON `MobileSyncData` (`UtilisateurId`);

CREATE INDEX `IX_MobileUserSessions_UtilisateurId` ON `MobileUserSessions` (`UtilisateurId`);

CREATE INDEX `IX_Notifications_TypeNotificationId` ON `Notifications` (`TypeNotificationId`);

CREATE INDEX `IX_PasswordResetTokens_UtilisateurId` ON `PasswordResetTokens` (`UtilisateurId`);

CREATE INDEX `IX_Prestations_HopitalPartenaireIdHopital` ON `Prestations` (`HopitalPartenaireIdHopital`);

CREATE INDEX `IX_Prestations_ProduitAssureurId` ON `Prestations` (`ProduitAssureurId`);

CREATE INDEX `IX_Prestations_ProduitMutuelId` ON `Prestations` (`ProduitMutuelId`);

CREATE INDEX `IX_ProduitsAssureurs_AssureurId` ON `ProduitsAssureurs` (`AssureurId`);

CREATE INDEX `IX_ProduitsAssureurs_DeviseId` ON `ProduitsAssureurs` (`DeviseId`);

CREATE INDEX `IX_ProduitsMutuels_DeviseId` ON `ProduitsMutuels` (`DeviseId`);

CREATE INDEX `IX_RefreshTokens_UtilisateurId` ON `RefreshTokens` (`UtilisateurId`);

CREATE INDEX `IX_RetraitsAgents_AgentId` ON `RetraitsAgents` (`AgentId`);

CREATE INDEX `IX_RetraitsAgents_DeviseIdDevise` ON `RetraitsAgents` (`DeviseIdDevise`);

CREATE INDEX `IX_RolePermissions_PermissionId` ON `RolePermissions` (`PermissionId`);

CREATE INDEX `IX_RolePermissions_RoleId` ON `RolePermissions` (`RoleId`);

CREATE INDEX `IX_SouscriptionsArrierees_AffilieId` ON `SouscriptionsArrierees` (`AffilieId`);

CREATE INDEX `IX_SouscriptionsArrierees_PrestationId` ON `SouscriptionsArrierees` (`PrestationId`);

CREATE INDEX `IX_SouscriptionsPrestations_AffilieId` ON `SouscriptionsPrestations` (`AffilieId`);

CREATE INDEX `IX_SouscriptionsPrestations_PrestationId` ON `SouscriptionsPrestations` (`PrestationId`);

CREATE INDEX `IX_TargetsAgents_AgentId` ON `TargetsAgents` (`AgentId`);

CREATE INDEX `IX_TypeAdhesions_CategorieAdhesionId` ON `TypeAdhesions` (`CategorieAdhesionId`);

CREATE INDEX `IX_UserDevices_UtilisateurId` ON `UserDevices` (`UtilisateurId`);

CREATE INDEX `IX_UserPermissions_PermissionId` ON `UserPermissions` (`PermissionId`);

CREATE INDEX `IX_UserPermissions_UtilisateurId` ON `UserPermissions` (`UtilisateurId`);

CREATE INDEX `IX_UserRoles_RoleId` ON `UserRoles` (`RoleId`);

CREATE INDEX `IX_UserRoles_UtilisateurId` ON `UserRoles` (`UtilisateurId`);

CREATE INDEX `IX_Utilisateurs_AffilieId` ON `Utilisateurs` (`AffilieId`);

CREATE INDEX `IX_Utilisateurs_AgentId` ON `Utilisateurs` (`AgentId`);

CREATE UNIQUE INDEX `IX_Utilisateurs_EmailUtilisateur` ON `Utilisateurs` (`EmailUtilisateur`);

CREATE UNIQUE INDEX `IX_Utilisateurs_PhoneUtilisateur` ON `Utilisateurs` (`PhoneUtilisateur`);

CREATE INDEX `IX_Utilisateurs_RoleId` ON `Utilisateurs` (`RoleId`);

CREATE INDEX `IX_WalletMouvements_CollecteIdCollecte` ON `WalletMouvements` (`CollecteIdCollecte`);

CREATE INDEX `IX_WalletMouvements_WalletId` ON `WalletMouvements` (`WalletId`);

CREATE UNIQUE INDEX `IX_WalletsAgents_AgentId` ON `WalletsAgents` (`AgentId`);

CREATE UNIQUE INDEX `IX_WalletsVirtuelsAgents_AgentId` ON `WalletsVirtuelsAgents` (`AgentId`);

CREATE INDEX `IX_ZonesSociales_CommuneId` ON `ZonesSociales` (`CommuneId`);

ALTER TABLE `DemandesRetraitAgents` ADD CONSTRAINT `FK_DemandesRetraitAgents_JetonsRetraits_JetonRetraitId` FOREIGN KEY (`JetonRetraitId`) REFERENCES `JetonsRetraits` (`IdJeton`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260318155922_InitialCreate', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Prestations` ADD `DeviseId` int NOT NULL DEFAULT 2;

ALTER TABLE `Prestations` ADD `Montant` decimal(18,2) NOT NULL DEFAULT 0.0;

CREATE INDEX `IX_Prestations_DeviseId` ON `Prestations` (`DeviseId`);

ALTER TABLE `Prestations` ADD CONSTRAINT `FK_Prestations_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260319094244_AddMontantAndDeviseIdToPrestation', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `ProduitsMutuels` ADD `TauxCommission` decimal(5,2) NOT NULL DEFAULT 25.0;

ALTER TABLE `ProduitsAssureurs` MODIFY COLUMN `CommissionMutuelle` decimal(5,2) NOT NULL;

ALTER TABLE `Frais` ADD `TauxCommission` decimal(5,2) NOT NULL DEFAULT 25.0;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260511095020_AddHybridCommissionRates', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

CREATE TABLE `CotisationsAffilie` (
    `IdCotisationAffilie` int NOT NULL AUTO_INCREMENT,
    `Montant` decimal(18,2) NOT NULL,
    `Periodicite` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `TypeAdhesionId` int NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_CotisationsAffilie` PRIMARY KEY (`IdCotisationAffilie`),
    CONSTRAINT `FK_CotisationsAffilie_TypeAdhesions_TypeAdhesionId` FOREIGN KEY (`TypeAdhesionId`) REFERENCES `TypeAdhesions` (`IdTypeAdhesion`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_CotisationsAffilie_TypeAdhesionId_Periodicite` ON `CotisationsAffilie` (`TypeAdhesionId`, `Periodicite`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260520141902_AddCotisationAffilie', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Collectes` ADD `CotisationAffilieId` int NULL;

CREATE INDEX `IX_Collectes_CotisationAffilieId` ON `Collectes` (`CotisationAffilieId`);

ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_CotisationsAffilie_CotisationAffilieId` FOREIGN KEY (`CotisationAffilieId`) REFERENCES `CotisationsAffilie` (`IdCotisationAffilie`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260520161402_AddCotisationAffilieIdToCollecte', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `ProduitsMutuels` RENAME COLUMN `PrixMensuel` TO `Montant`;

ALTER TABLE `ProduitsAssureurs` RENAME COLUMN `PrixMensuel` TO `Montant`;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `ProduitsMutuels` ADD `AgeMax` int NOT NULL DEFAULT 0;

ALTER TABLE `ProduitsMutuels` ADD `AgeMin` int NOT NULL DEFAULT 0;

ALTER TABLE `ProduitsMutuels` ADD `Periodicite` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Mensuel';

ALTER TABLE `ProduitsAssureurs` ADD `AgeMax` int NOT NULL DEFAULT 0;

ALTER TABLE `ProduitsAssureurs` ADD `AgeMin` int NOT NULL DEFAULT 0;

ALTER TABLE `ProduitsAssureurs` ADD `Periodicite` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Mensuel';

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260520165942_ProduitTarifMontantPeriodiciteAge', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `ProduitsMutuels` RENAME COLUMN `TauxCommission` TO `TauxCommissionAT`;

ALTER TABLE `ProduitsAssureurs` RENAME COLUMN `CommissionMutuelle` TO `TauxCommissionAT`;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `ProduitsMutuels` ADD `TauxCommissionAA` decimal(5,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `ProduitsMutuels` ADD `TauxCommissionAAMash` decimal(5,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `ProduitsMutuels` ADD `TauxCommissionAAStructure` decimal(5,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `ProduitsAssureurs` ADD `TauxCommissionAA` decimal(5,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `ProduitsAssureurs` ADD `TauxCommissionAAMash` decimal(5,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `ProduitsAssureurs` ADD `TauxCommissionAAStructure` decimal(5,2) NOT NULL DEFAULT 0.0;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260520170926_ProduitQuatreTauxCommission', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `ProduitsMutuels` ADD `EstGratuit` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `ProduitsAssureurs` ADD `EstGratuit` tinyint(1) NOT NULL DEFAULT FALSE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260520171424_ProduitEstGratuit', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `TargetsAgents` CHANGE `MontantTarget` `Nombre` int NOT NULL;

ALTER TABLE `TargetsAgents` DROP COLUMN `DateDebut`;

ALTER TABLE `TargetsAgents` DROP COLUMN `DateFin`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521082548_TargetAgentNombreSansDates', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `TargetsAgents` ADD `Periodicite` int NOT NULL DEFAULT 3;

UPDATE `TargetsAgents` SET `Periodicite` = 1, `Nombre` = 5 WHERE `Nombre` = 5;UPDATE `TargetsAgents` SET `Periodicite` = 2, `Nombre` = 25 WHERE `Nombre` = 25;UPDATE `TargetsAgents` SET `Periodicite` = 3, `Nombre` = 100 WHERE `Nombre` NOT IN (5, 25);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521082955_TargetAgentPeriodicite', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Affilies` ADD `CarteIdentiteUrl` varchar(500) CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521084222_AffilieCarteIdentiteUrl', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `Affilies` DROP COLUMN `CarteIdentiteUrl`;

ALTER TABLE `Affilies` DROP COLUMN `PhotoUrl`;

ALTER TABLE `Affilies` ADD `CarteIdentiteContentType` varchar(100) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Affilies` ADD `CarteIdentiteData` longblob NULL;

ALTER TABLE `Affilies` ADD `PhotoContentType` varchar(100) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Affilies` ADD `PhotoData` longblob NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521085109_AffilieFichiersBinaires', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `Dependants` MODIFY COLUMN `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `Dependants` ADD `Adresse` varchar(500) CHARACTER SET utf8mb4 NULL;

CREATE TABLE `PersonnesContact` (
    `IdPersonneContact` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `LienParente` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Adresse` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_PersonnesContact` PRIMARY KEY (`IdPersonneContact`),
    CONSTRAINT `FK_PersonnesContact_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_PersonnesContact_AffilieId` ON `PersonnesContact` (`AffilieId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521090655_AdhesionNiveau2Encodeur', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

CREATE TABLE `AgentBeneficiairesMaash` (
    `IdAgentBeneficiaireMaash` int NOT NULL AUTO_INCREMENT,
    `AgentId` int NOT NULL,
    `NomComplet` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `LienParente` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Adresse` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_AgentBeneficiairesMaash` PRIMARY KEY (`IdAgentBeneficiaireMaash`),
    CONSTRAINT `FK_AgentBeneficiairesMaash_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `RetenuesMaashAgents` (
    `IdRetenueMaashAgent` int NOT NULL AUTO_INCREMENT,
    `AgentId` int NOT NULL,
    `Annee` int NOT NULL,
    `Mois` int NOT NULL,
    `Montant` decimal(18,2) NOT NULL,
    `DeviseId` int NOT NULL,
    `WalletMouvementId` int NULL,
    `DatePaiement` datetime(6) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_RetenuesMaashAgents` PRIMARY KEY (`IdRetenueMaashAgent`),
    CONSTRAINT `FK_RetenuesMaashAgents_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE RESTRICT,
    CONSTRAINT `FK_RetenuesMaashAgents_WalletMouvements_WalletMouvementId` FOREIGN KEY (`WalletMouvementId`) REFERENCES `WalletMouvements` (`IdWalletMouvement`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_AgentBeneficiairesMaash_AgentId` ON `AgentBeneficiairesMaash` (`AgentId`);

CREATE UNIQUE INDEX `IX_RetenuesMaashAgents_AgentId_Annee_Mois` ON `RetenuesMaashAgents` (`AgentId`, `Annee`, `Mois`);

CREATE INDEX `IX_RetenuesMaashAgents_WalletMouvementId` ON `RetenuesMaashAgents` (`WalletMouvementId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521101009_AgentMaashRetenue', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Dependants` ADD `CertificatScolariteContentType` varchar(100) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Dependants` ADD `CertificatScolariteData` longblob NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521101823_PersonneEnChargeRegles', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `Frais` ADD `Periodicite` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Ponctuel';

UPDATE Frais SET Periodicite = 'Ponctuel' WHERE Periodicite = '' OR Periodicite IS NULL;

CREATE TABLE `ArrieresAffilie` (
    `IdArrieresAffilie` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `TypeObligation` int NOT NULL,
    `FraisId` int NULL,
    `SouscriptionPrestationId` int NULL,
    `CotisationAffilieId` int NULL,
    `Mois` int NOT NULL,
    `Annee` int NOT NULL,
    `DateEcheance` datetime(6) NOT NULL,
    `Periodicite` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MontantAttendu` decimal(18,2) NOT NULL,
    `MontantPaye` decimal(18,2) NOT NULL,
    `RestAPayer` decimal(18,2) NOT NULL,
    `DeviseId` int NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `StatutPaiement` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `DateDernierPaiement` datetime(6) NULL,
    CONSTRAINT `PK_ArrieresAffilie` PRIMARY KEY (`IdArrieresAffilie`),
    CONSTRAINT `FK_ArrieresAffilie_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE,
    CONSTRAINT `FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId` FOREIGN KEY (`CotisationAffilieId`) REFERENCES `CotisationsAffilie` (`IdCotisationAffilie`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ArrieresAffilie_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ArrieresAffilie_Frais_FraisId` FOREIGN KEY (`FraisId`) REFERENCES `Frais` (`IdFrais`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ArrieresAffilie_SouscriptionsPrestations_SouscriptionPrestat~` FOREIGN KEY (`SouscriptionPrestationId`) REFERENCES `SouscriptionsPrestations` (`IdSouscriptionPrestation`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_ArrieresAffilie_AffilieId_TypeObligation_Mois_Annee_FraisId_~` ON `ArrieresAffilie` (`AffilieId`, `TypeObligation`, `Mois`, `Annee`, `FraisId`, `SouscriptionPrestationId`, `CotisationAffilieId`);

CREATE INDEX `IX_ArrieresAffilie_CotisationAffilieId` ON `ArrieresAffilie` (`CotisationAffilieId`);

CREATE INDEX `IX_ArrieresAffilie_DeviseId` ON `ArrieresAffilie` (`DeviseId`);

CREATE INDEX `IX_ArrieresAffilie_FraisId` ON `ArrieresAffilie` (`FraisId`);

CREATE INDEX `IX_ArrieresAffilie_SouscriptionPrestationId` ON `ArrieresAffilie` (`SouscriptionPrestationId`);


INSERT INTO ArrieresAffilie (
    AffilieId, TypeObligation, SouscriptionPrestationId, Mois, Annee, DateEcheance,
    Periodicite, MontantAttendu, MontantPaye, RestAPayer, DeviseId, Description,
    StatutPaiement, Statut, DateCreation, DateModification, DateDernierPaiement)
SELECT
    sa.AffilieId,
    2,
    sp.IdSouscriptionPrestation,
    CAST(SUBSTRING_INDEX(sa.Periode, '-', 1) AS UNSIGNED),
    CAST(SUBSTRING_INDEX(sa.Periode, '-', -1) AS UNSIGNED),
    STR_TO_DATE(CONCAT('01-', sa.Periode), '%d-%m-%Y'),
    'Mensuel',
    sa.MontantAttendu,
    sa.MontantPaye,
    sa.RestAPayer,
    COALESCE(p.DeviseId, 1),
    sa.Description,
    sa.StatutPaiement,
    sa.Statut,
    sa.DateCreation,
    sa.DateModification,
    sa.DateDernierPaiement
FROM SouscriptionsArrierees sa
LEFT JOIN SouscriptionsPrestations sp
    ON sp.AffilieId = sa.AffilieId AND sp.PrestationId = sa.PrestationId AND sp.Statut = 1
LEFT JOIN Prestations p ON p.IdPrestation = sa.PrestationId;


ALTER TABLE `Collectes` DROP FOREIGN KEY `FK_Collectes_SouscriptionsArrierees_SouscriptionsArriereesIdSou~`;

ALTER TABLE `Collectes` DROP INDEX `IX_Collectes_SouscriptionsArriereesIdSouscriptionsArrierees`;

ALTER TABLE `Collectes` DROP COLUMN `SouscriptionsArriereesIdSouscriptionsArrierees`;

ALTER TABLE `Collectes` ADD `ArrieresAffilieId` int NULL;

CREATE INDEX `IX_Collectes_ArrieresAffilieId` ON `Collectes` (`ArrieresAffilieId`);

ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_ArrieresAffilie_ArrieresAffilieId` FOREIGN KEY (`ArrieresAffilieId`) REFERENCES `ArrieresAffilie` (`IdArrieresAffilie`) ON DELETE SET NULL;

DROP TABLE `SouscriptionsArrierees`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260524210114_AddArrieresAffilieUnifie', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Collectes` ADD `PenaliteAffilieId` int NULL;

CREATE TABLE `PenalitesAffilie` (
    `IdPenaliteAffilie` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `ArrieresAffilieId` int NOT NULL,
    `FraisId` int NOT NULL,
    `TypePenalite` int NOT NULL,
    `Montant` decimal(18,2) NOT NULL,
    `DeviseId` int NOT NULL,
    `JoursRetard` int NOT NULL,
    `Motif` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Statut` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MotifAnnulation` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateApplication` datetime(6) NOT NULL,
    `DatePaiement` datetime(6) NULL,
    `DateAnnulation` datetime(6) NULL,
    `StatutActif` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_PenalitesAffilie` PRIMARY KEY (`IdPenaliteAffilie`),
    CONSTRAINT `FK_PenalitesAffilie_Affilies_AffilieId` FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenalitesAffilie_ArrieresAffilie_ArrieresAffilieId` FOREIGN KEY (`ArrieresAffilieId`) REFERENCES `ArrieresAffilie` (`IdArrieresAffilie`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenalitesAffilie_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenalitesAffilie_Frais_FraisId` FOREIGN KEY (`FraisId`) REFERENCES `Frais` (`IdFrais`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Collectes_PenaliteAffilieId` ON `Collectes` (`PenaliteAffilieId`);

CREATE INDEX `IX_PenalitesAffilie_AffilieId` ON `PenalitesAffilie` (`AffilieId`);

CREATE UNIQUE INDEX `IX_PenalitesAffilie_ArrieresAffilieId_TypePenalite` ON `PenalitesAffilie` (`ArrieresAffilieId`, `TypePenalite`);

CREATE INDEX `IX_PenalitesAffilie_DeviseId` ON `PenalitesAffilie` (`DeviseId`);

CREATE INDEX `IX_PenalitesAffilie_FraisId` ON `PenalitesAffilie` (`FraisId`);

ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_PenalitesAffilie_PenaliteAffilieId` FOREIGN KEY (`PenaliteAffilieId`) REFERENCES `PenalitesAffilie` (`IdPenaliteAffilie`) ON DELETE SET NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260524212719_AddPenaliteAffilie', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Devises` ADD `EstDevisePrincipale` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `Devises` ADD `Symbole` varchar(10) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Collectes` ADD `DevisePrincipaleId` int NULL;

ALTER TABLE `Collectes` ADD `DeviseTarifId` int NULL;

ALTER TABLE `Collectes` ADD `MontantDevisePrincipale` decimal(18,2) NULL;

ALTER TABLE `Collectes` ADD `MontantTarifAttendu` decimal(18,2) NULL;

ALTER TABLE `Collectes` ADD `TauxVersDevisePrincipale` decimal(18,6) NULL;

CREATE TABLE `TauxChangeDevises` (
    `IdTauxChangeDevise` int NOT NULL AUTO_INCREMENT,
    `DeviseSourceId` int NOT NULL,
    `DeviseCibleId` int NOT NULL,
    `Taux` decimal(18,6) NOT NULL,
    `DateEffet` datetime(6) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_TauxChangeDevises` PRIMARY KEY (`IdTauxChangeDevise`),
    CONSTRAINT `FK_TauxChangeDevises_Devises_DeviseCibleId` FOREIGN KEY (`DeviseCibleId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TauxChangeDevises_Devises_DeviseSourceId` FOREIGN KEY (`DeviseSourceId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Collectes_DevisePrincipaleId` ON `Collectes` (`DevisePrincipaleId`);

CREATE INDEX `IX_Collectes_DeviseTarifId` ON `Collectes` (`DeviseTarifId`);

CREATE INDEX `IX_TauxChangeDevises_DeviseCibleId` ON `TauxChangeDevises` (`DeviseCibleId`);

CREATE INDEX `IX_TauxChangeDevises_DeviseSourceId_DeviseCibleId_DateEffet` ON `TauxChangeDevises` (`DeviseSourceId`, `DeviseCibleId`, `DateEffet`);

ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_Devises_DevisePrincipaleId` FOREIGN KEY (`DevisePrincipaleId`) REFERENCES `Devises` (`IdDevise`) ON DELETE SET NULL;

ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_Devises_DeviseTarifId` FOREIGN KEY (`DeviseTarifId`) REFERENCES `Devises` (`IdDevise`) ON DELETE SET NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260524220719_AddMultideviseModule', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Collectes` ADD `OrderNumberFlexPay` varchar(100) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Collectes` ADD `ProviderReferenceFlexPay` varchar(100) CHARACTER SET utf8mb4 NULL;

CREATE TABLE `CollectesEnAttente` (
    `IdCollecteEnAttente` char(36) COLLATE ascii_general_ci NOT NULL,
    `SourceFlux` int NOT NULL,
    `StatutEnAttente` int NOT NULL,
    `AffilieId` int NULL,
    `AgentId` int NULL,
    `IdUtilisateur` int NULL,
    `TypeCollecte` int NOT NULL,
    `FraisId` int NULL,
    `CotisationAffilieId` int NULL,
    `SouscriptionPrestationId` int NULL,
    `Mois` int NOT NULL,
    `Annee` int NOT NULL,
    `MethodePaiement` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
    `MontantTarif` decimal(18,2) NOT NULL,
    `DeviseTarifId` int NOT NULL,
    `MontantFlexPay` decimal(18,2) NOT NULL,
    `CodeDevisePaiement` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `TauxVersDevisePaiement` decimal(18,6) NULL,
    `OrderNumberFlexPay` varchar(100) CHARACTER SET utf8mb4 NULL,
    `ReferenceFlexPay` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Phone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `TelephoneAffilie` varchar(100) CHARACTER SET utf8mb4 NULL,
    `PayloadMetierJson` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DateExpiration` datetime(6) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `IdCollecteFinalisee` int NULL,
    CONSTRAINT `PK_CollectesEnAttente` PRIMARY KEY (`IdCollecteEnAttente`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `InfoPaiementsMarchand` (
    `IdInfoPaiementMarchand` int NOT NULL AUTO_INCREMENT,
    `CodeMarchand` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ApiToken` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `ActifMobileMoney` tinyint(1) NOT NULL,
    `ActifCarteBancaire` tinyint(1) NOT NULL,
    `Statut` tinyint(1) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    CONSTRAINT `PK_InfoPaiementsMarchand` PRIMARY KEY (`IdInfoPaiementMarchand`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `TransactionsFlexPay` (
    `IdTransaction` char(36) COLLATE ascii_general_ci NOT NULL,
    `OrderNumber` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Reference` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderReference` varchar(100) CHARACTER SET utf8mb4 NULL,
    `TypePaiement` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `Channel` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Amount` decimal(18,2) NOT NULL,
    `AmountCustomer` decimal(18,2) NULL,
    `Currency` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `Phone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `CodeFlexPay` varchar(10) CHARACTER SET utf8mb4 NULL,
    `MessageFlexPay` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Merchant` varchar(100) CHARACTER SET utf8mb4 NULL,
    `CallbackUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `PaymentUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateCallback` datetime(6) NULL,
    `DateDerniereVerification` datetime(6) NULL,
    `IdCollecteEnAttente` char(36) COLLATE ascii_general_ci NULL,
    `IdCollecte` int NULL,
    `SourceFlux` int NOT NULL,
    `MessageErreur` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `ReponseBruteFlexPay` longtext CHARACTER SET utf8mb4 NULL,
    `NombreCallbacks` int NOT NULL,
    `NombreVerifications` int NOT NULL,
    CONSTRAINT `PK_TransactionsFlexPay` PRIMARY KEY (`IdTransaction`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `PaiementHolds` (
    `IdPaiementHold` int NOT NULL AUTO_INCREMENT,
    `IdCollecteEnAttente` char(36) COLLATE ascii_general_ci NOT NULL,
    `AffilieId` int NULL,
    `TypeCollecte` int NOT NULL,
    `Mois` int NOT NULL,
    `Annee` int NOT NULL,
    `FraisId` int NULL,
    `SouscriptionPrestationId` int NULL,
    `CotisationAffilieId` int NULL,
    `TelephoneAffilie` varchar(30) CHARACTER SET utf8mb4 NULL,
    `ExpireAt` datetime(6) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    CONSTRAINT `PK_PaiementHolds` PRIMARY KEY (`IdPaiementHold`),
    CONSTRAINT `FK_PaiementHolds_CollectesEnAttente_IdCollecteEnAttente` FOREIGN KEY (`IdCollecteEnAttente`) REFERENCES `CollectesEnAttente` (`IdCollecteEnAttente`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `CallbacksFlexPay` (
    `IdCallback` char(36) COLLATE ascii_general_ci NOT NULL,
    `IdTransaction` char(36) COLLATE ascii_general_ci NULL,
    `OrderNumber` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Code` varchar(10) CHARACTER SET utf8mb4 NULL,
    `Reference` varchar(100) CHARACTER SET utf8mb4 NULL,
    `ProviderReference` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Amount` varchar(50) CHARACTER SET utf8mb4 NULL,
    `AmountCustomer` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Phone` varchar(20) CHARACTER SET utf8mb4 NULL,
    `Currency` varchar(10) CHARACTER SET utf8mb4 NULL,
    `Channel` varchar(50) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` varchar(50) CHARACTER SET utf8mb4 NULL,
    `PayloadComplet` longtext CHARACTER SET utf8mb4 NULL,
    `Headers` longtext CHARACTER SET utf8mb4 NULL,
    `IpSource` varchar(50) CHARACTER SET utf8mb4 NULL,
    `DateReception` datetime(6) NOT NULL,
    `TraiteAvecSucces` tinyint(1) NOT NULL,
    `MessageErreur` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `DetailsTraitement` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_CallbacksFlexPay` PRIMARY KEY (`IdCallback`),
    CONSTRAINT `FK_CallbacksFlexPay_TransactionsFlexPay_IdTransaction` FOREIGN KEY (`IdTransaction`) REFERENCES `TransactionsFlexPay` (`IdTransaction`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_CallbacksFlexPay_IdTransaction` ON `CallbacksFlexPay` (`IdTransaction`);

CREATE INDEX `IX_PaiementHolds_IdCollecteEnAttente` ON `PaiementHolds` (`IdCollecteEnAttente`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260524224948_AddFlexPayModule', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `CollectesEnAttente` ADD `IdAdhesionFinalisee` int NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260524230232_AddFlexPayAdhesionFinalisee', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` ADD `DeviseId` int NULL;

ALTER TABLE `WalletMouvements` ADD `DeviseId` int NULL;


                UPDATE WalletsAgents w
                SET w.DeviseId = (
                    SELECT d.IdDevise FROM Devises d
                    WHERE d.EstDevisePrincipale = 1 AND d.Statut = 1
                    ORDER BY d.IdDevise
                    LIMIT 1
                )
                WHERE w.DeviseId IS NULL;
            


                UPDATE WalletMouvements m
                INNER JOIN WalletsAgents w ON m.WalletId = w.IdWalletAgent
                SET m.DeviseId = w.DeviseId
                WHERE m.DeviseId IS NULL;
            


                UPDATE WalletMouvements m
                SET m.DeviseId = (
                    SELECT d.IdDevise FROM Devises d
                    WHERE d.EstDevisePrincipale = 1 AND d.Statut = 1
                    ORDER BY d.IdDevise
                    LIMIT 1
                )
                WHERE m.DeviseId IS NULL;
            

ALTER TABLE `WalletsAgents` MODIFY COLUMN `DeviseId` int NOT NULL;

ALTER TABLE `WalletMouvements` MODIFY COLUMN `DeviseId` int NOT NULL;

CREATE UNIQUE INDEX `IX_WalletsAgents_AgentId_DeviseId` ON `WalletsAgents` (`AgentId`, `DeviseId`);

ALTER TABLE `WalletsAgents` DROP INDEX `IX_WalletsAgents_AgentId`;

CREATE INDEX `IX_WalletsAgents_DeviseId` ON `WalletsAgents` (`DeviseId`);

CREATE INDEX `IX_WalletMouvements_DeviseId` ON `WalletMouvements` (`DeviseId`);

ALTER TABLE `WalletMouvements` ADD CONSTRAINT `FK_WalletMouvements_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT;

ALTER TABLE `WalletsAgents` ADD CONSTRAINT `FK_WalletsAgents_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260526122625_AddWalletAgentDeviseId', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `DemandesBonEnvoi` DROP FOREIGN KEY `FK_DemandesBonEnvoi_Agents_AgentId`;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `DemandesBonEnvoi` MODIFY COLUMN `AgentId` int NULL;

ALTER TABLE `BonsEnvoi` ADD `QrCodeImageBase64` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `BonsEnvoi` ADD `QrCodePayload` varchar(2000) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `DemandesBonEnvoi` ADD CONSTRAINT `FK_DemandesBonEnvoi_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE SET NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260526130652_AddBonEnvoiQrCodeAndDemandeAgentNullable', '6.0.25');

COMMIT;

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

ALTER TABLE `Frais` ADD `Code` varchar(50) CHARACTER SET utf8mb4 NULL;

CREATE UNIQUE INDEX `IX_Frais_Code` ON `Frais` (`Code`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260526134513_AddFraisCode', '6.0.25');

COMMIT;

