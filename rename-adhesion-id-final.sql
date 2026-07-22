-- Script pour renommer la colonne Id en IdAdhesion dans la table Adhesions
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `Adhesions` CHANGE COLUMN `Id` `IdAdhesion` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `Adhesions` DROP PRIMARY KEY;
ALTER TABLE `Adhesions` ADD PRIMARY KEY (`IdAdhesion`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260302091714_RenameAdhesionIdToIdAdhesion', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdAdhesion avec succès!' as Result;
DESCRIBE `Adhesions`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302091714_RenameAdhesionIdToIdAdhesion';
