-- Script pour renommer la colonne Id en IdBonEnvoi dans la table BonsEnvoi
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `BonsEnvoi` CHANGE COLUMN `Id` `IdBonEnvoi` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `BonsEnvoi` DROP PRIMARY KEY;
ALTER TABLE `BonsEnvoi` ADD PRIMARY KEY (`IdBonEnvoi`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260302094559_RenameBonEnvoiIdToIdBonEnvoi', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdBonEnvoi avec succès!' as Result;
DESCRIBE `BonsEnvoi`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302094559_RenameBonEnvoiIdToIdBonEnvoi';
