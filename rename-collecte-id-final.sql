-- Script pour renommer la colonne Id en IdCollecte dans la table Collectes
-- et ajuster la FK WalletMouvements -> Collectes
-- Exécuter ce script directement dans votre client MySQL

-- 1) Retirer la FK dépendante (si elle existe)
ALTER TABLE `WalletMouvements` DROP FOREIGN KEY `FK_WalletMouvements_Collectes_CollecteId`;

-- 2) Renommer la colonne FK côté WalletMouvements (si elle existe)
ALTER TABLE `WalletMouvements` CHANGE COLUMN `CollecteId` `CollecteIdCollecte` int NULL;

-- 3) Renommer la clé primaire côté Collectes
ALTER TABLE `Collectes` CHANGE COLUMN `Id` `IdCollecte` int NOT NULL AUTO_INCREMENT;
ALTER TABLE `Collectes` DROP PRIMARY KEY;
ALTER TABLE `Collectes` ADD PRIMARY KEY (`IdCollecte`);

-- 4) Recréer la FK WalletMouvements -> Collectes
ALTER TABLE `WalletMouvements`
    ADD CONSTRAINT `FK_WalletMouvements_Collectes_CollecteIdCollecte`
    FOREIGN KEY (`CollecteIdCollecte`) REFERENCES `Collectes` (`IdCollecte`);

-- 5) Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260302093843_RenameCollecteIdToIdCollecte', '6.0.25');

-- Vérification
SELECT 'Collecte.Id renommé en IdCollecte + FK WalletMouvements mise à jour' as Result;
DESCRIBE `Collectes`;
DESCRIBE `WalletMouvements`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302093843_RenameCollecteIdToIdCollecte';
