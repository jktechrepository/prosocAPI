-- Script pour renommer la colonne Id en IdDependant dans la table Dependants
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `Dependants` CHANGE COLUMN `Id` `IdDependant` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `Dependants` DROP PRIMARY KEY;
ALTER TABLE `Dependants` ADD PRIMARY KEY (`IdDependant`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260302093457_RenameDependantIdToIdDependant', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdDependant avec succès!' as Result;
DESCRIBE `Dependants`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302093457_RenameDependantIdToIdDependant';
