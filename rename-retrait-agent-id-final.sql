-- Script pour renommer la colonne Id en IdRetraitAgent dans la table RetraitsAgents
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `RetraitsAgents` CHANGE COLUMN `Id` `IdRetraitAgent` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `RetraitsAgents` DROP PRIMARY KEY;
ALTER TABLE `RetraitsAgents` ADD PRIMARY KEY (`IdRetraitAgent`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260302094948_RenameRetraitAgentIdToIdRetraitAgent', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdRetraitAgent avec succès!' as Result;
DESCRIBE `RetraitsAgents`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302094948_RenameRetraitAgentIdToIdRetraitAgent';
