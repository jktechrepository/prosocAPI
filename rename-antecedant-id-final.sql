-- Script pour renommer la colonne Id en IdAntecedant dans la table Antecedants
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `Antecedants` CHANGE COLUMN `Id` `IdAntecedant` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `Antecedants` DROP PRIMARY KEY;
ALTER TABLE `Antecedants` ADD PRIMARY KEY (`IdAntecedant`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260302093013_RenameAntecedantIdToIdAntecedant', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdAntecedant avec succès!' as Result;
DESCRIBE `Antecedants`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302093013_RenameAntecedantIdToIdAntecedant';
