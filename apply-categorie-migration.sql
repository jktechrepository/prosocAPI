-- Script pour appliquer manuellement la migration CategorieAgent
-- Exécuter ce script directement dans MySQL

-- 1. Marquer la migration problématique comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260226162138_AddCompletePermissionsSystem', '6.0.25');

-- 2. Appliquer notre migration CategorieAgent
ALTER TABLE `Agents` ADD COLUMN `CategorieAgentId` int NULL;

CREATE TABLE `CategoriesAgents` (
    `IdCategorieAgent` int NOT NULL AUTO_INCREMENT,
    `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriesAgents` PRIMARY KEY (`IdCategorieAgent`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Agents_CategorieAgentId` ON `Agents` (`CategorieAgentId`);

ALTER TABLE `Agents` ADD CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` 
FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`);

-- 3. Marquer notre migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260302103640_CategorieAgentClean', '6.0.25');

-- Vérification
SELECT * FROM `__EFMigrationsHistory` ORDER BY `MigrationId` DESC LIMIT 5;
