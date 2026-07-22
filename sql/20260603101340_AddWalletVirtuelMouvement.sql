-- Migration linéaire (une seule exécution) — préférer le fichier .idempotent.sql en production
-- Voir sql/20260603101340_AddWalletVirtuelMouvement.idempotent.sql pour la procédure complète

START TRANSACTION;

ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

CREATE TABLE `WalletVirtuelMouvements` (
    `IdWalletVirtuelMouvement` int NOT NULL AUTO_INCREMENT,
    `WalletVirtuelId` int NOT NULL,
    `Montant` decimal(18,2) NOT NULL,
    `TypeOperation` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `Source` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `ReferenceExterne` int NULL,
    `DateOperation` datetime(6) NOT NULL,
    `DateCreation` datetime(6) NOT NULL,
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL,
    CONSTRAINT `PK_WalletVirtuelMouvements` PRIMARY KEY (`IdWalletVirtuelMouvement`),
    CONSTRAINT `FK_WalletVirtuelMouvements_WalletsVirtuelsAgents_WalletVirtuelId` FOREIGN KEY (`WalletVirtuelId`) REFERENCES `WalletsVirtuelsAgents` (`IdWalletVirtuelAgent`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_WalletVirtuelMouvements_WalletVirtuelId_DateOperation` ON `WalletVirtuelMouvements` (`WalletVirtuelId`, `DateOperation`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260603101340_AddWalletVirtuelMouvement', '6.0.25');

COMMIT;
