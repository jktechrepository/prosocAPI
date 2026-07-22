-- Script pour renommer la colonne Id en IdWalletMouvement dans la table WalletMouvements
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `WalletMouvements` CHANGE COLUMN `Id` `IdWalletMouvement` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `WalletMouvements` DROP PRIMARY KEY;
ALTER TABLE `WalletMouvements` ADD PRIMARY KEY (`IdWalletMouvement`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260302100219_RenameWalletMouvementIdToIdWalletMouvement', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdWalletMouvement avec succès!' as Result;
DESCRIBE `WalletMouvements`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302100219_RenameWalletMouvementIdToIdWalletMouvement';
