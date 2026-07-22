-- Apply CategorieAgent migration manually
START TRANSACTION;

-- Add column to Agents table
ALTER TABLE `Agents` ADD COLUMN `CategorieAgentId` int NULL;

-- Create CategoriesAgents table
CREATE TABLE `CategoriesAgents` (
    `IdCategorieAgent` int NOT NULL AUTO_INCREMENT,
    `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_CategoriesAgents` PRIMARY KEY (`IdCategorieAgent`)
) CHARACTER SET=utf8mb4;

-- Create index
CREATE INDEX `IX_Agents_CategorieAgentId` ON `Agents` (`CategorieAgentId`);

-- Add foreign key constraint
ALTER TABLE `Agents` ADD CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`);

-- Mark migration as applied
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260227160319_AddCategorieAgentOnly', '6.0.25');

COMMIT;
