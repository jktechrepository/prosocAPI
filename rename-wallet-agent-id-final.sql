-- Script pour renommer la colonne Id en IdWalletAgent dans la table WalletsAgents
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `WalletsAgents` CHANGE COLUMN `Id` `IdWalletAgent` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `WalletsAgents` DROP PRIMARY KEY;
ALTER TABLE `WalletsAgents` ADD PRIMARY KEY (`IdWalletAgent`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260302095900_RenameWalletAgentIdToIdWalletAgent', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdWalletAgent avec succès!' as Result;
DESCRIBE `WalletsAgents`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302095900_RenameWalletAgentIdToIdWalletAgent';
