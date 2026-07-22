-- Script pour renommer la colonne Id en IdAffilie dans la table Affilies
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `Affilies` CHANGE COLUMN `Id` `IdAffilie` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `Affilies` DROP PRIMARY KEY;
ALTER TABLE `Affilies` ADD PRIMARY KEY (`IdAffilie`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260302092126_RenameAffilieIdToIdAffilie', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdAffilie avec succès!' as Result;
DESCRIBE `Affilies`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302092126_RenameAffilieIdToIdAffilie';
