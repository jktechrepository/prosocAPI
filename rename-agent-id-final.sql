-- Script pour renommer la colonne Id en IdAgent dans la table Agents
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `Agents` CHANGE COLUMN `Id` `IdAgent` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire (elle devrait être automatiquement mise à jour)
-- Mais pour être sûr, vérifions qu'elle utilise bien le nouveau nom
ALTER TABLE `Agents` DROP PRIMARY KEY;
ALTER TABLE `Agents` ADD PRIMARY KEY (`IdAgent`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260302091111_RenameAgentIdToIdAgent', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdAgent avec succès!' as Result;
DESCRIBE `Agents`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302091111_RenameAgentIdToIdAgent';
