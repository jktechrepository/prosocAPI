-- =============================================================================
-- Migration : 20260603101340_AddWalletVirtuelMouvement
-- Objectif  : Journal des mouvements wallet virtuel (table WalletVirtuelMouvements)
-- Idempotent: vérifie __EFMigrationsHistory — réexécutable sans erreur
-- =============================================================================
--
-- Prérequis :
--   - MySQL / MariaDB (base ProsocAPI)
--   - Table WalletsVirtuelsAgents existante
--   - Sauvegarde recommandée avant exécution en production
--
-- Vérifier l'état actuel :
--   SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 10;
--   Si '20260603101340_AddWalletVirtuelMouvement' est déjà présent, ne pas réexécuter.
--
-- Exécution (exemple) :
--   mysql -h HOST -u USER -p NOM_BASE < sql/20260603101340_AddWalletVirtuelMouvement.idempotent.sql
--
-- Contrôles post-déploiement :
--   SHOW CREATE TABLE WalletVirtuelMouvements;
--   SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260603101340_AddWalletVirtuelMouvement';
--
-- Référence EF : Migrations/20260603101340_AddWalletVirtuelMouvement.cs
-- =============================================================================

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260603101340_AddWalletVirtuelMouvement') THEN

    ALTER TABLE `WalletsAgents` MODIFY COLUMN `RowVersion` timestamp(6) NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260603101340_AddWalletVirtuelMouvement') THEN
    IF NOT EXISTS(
        SELECT 1 FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WalletVirtuelMouvements'
    ) THEN

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

    END IF;
    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260603101340_AddWalletVirtuelMouvement') THEN
    IF NOT EXISTS(
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'WalletVirtuelMouvements'
          AND INDEX_NAME = 'IX_WalletVirtuelMouvements_WalletVirtuelId_DateOperation'
    ) THEN

    CREATE INDEX `IX_WalletVirtuelMouvements_WalletVirtuelId_DateOperation` ON `WalletVirtuelMouvements` (`WalletVirtuelId`, `DateOperation`);

    END IF;
    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260603101340_AddWalletVirtuelMouvement') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260603101340_AddWalletVirtuelMouvement', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;
