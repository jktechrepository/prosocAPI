START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    ALTER TABLE `JetonsRetraits` ADD `OperateurUtilisateurId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    ALTER TABLE `DemandesRetraitAgents` ADD `OperateurPaiementUtilisateurId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    ALTER TABLE `DemandesRetraitAgents` ADD `WalletMouvementId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE TABLE `SessionsCaisses` (
        `IdSessionCaisse` int NOT NULL AUTO_INCREMENT,
        `UtilisateurId` int NOT NULL,
        `SoldeOuverture` decimal(18,2) NOT NULL,
        `DeviseId` int NOT NULL,
        `Statut` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `DateOuverture` datetime(6) NOT NULL,
        `DateCloture` datetime(6) NULL,
        `ObservationCloture` varchar(500) CHARACTER SET utf8mb4 NULL,
        `SoldeTheoriqueCloture` decimal(18,2) NULL,
        `SoldeReelCloture` decimal(18,2) NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `StatutActif` tinyint(1) NOT NULL,
        CONSTRAINT `PK_SessionsCaisses` PRIMARY KEY (`IdSessionCaisse`),
        CONSTRAINT `FK_SessionsCaisses_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE RESTRICT,
        CONSTRAINT `FK_SessionsCaisses_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE TABLE `MouvementsCaisses` (
        `IdMouvementCaisse` int NOT NULL AUTO_INCREMENT,
        `SessionCaisseId` int NOT NULL,
        `UtilisateurId` int NOT NULL,
        `TypeOperation` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `Source` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Montant` decimal(18,2) NOT NULL,
        `DeviseId` int NOT NULL,
        `DateOperation` datetime(6) NOT NULL,
        `CollecteId` int NULL,
        `DemandeRetraitId` int NULL,
        `JetonRetraitId` int NULL,
        `WalletMouvementId` int NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DateCreation` datetime(6) NOT NULL,
        `Statut` tinyint(1) NOT NULL,
        CONSTRAINT `PK_MouvementsCaisses` PRIMARY KEY (`IdMouvementCaisse`),
        CONSTRAINT `FK_MouvementsCaisses_Collectes_CollecteId` FOREIGN KEY (`CollecteId`) REFERENCES `Collectes` (`IdCollecte`) ON DELETE SET NULL,
        CONSTRAINT `FK_MouvementsCaisses_DemandesRetraitAgents_DemandeRetraitId` FOREIGN KEY (`DemandeRetraitId`) REFERENCES `DemandesRetraitAgents` (`IdDemande`) ON DELETE SET NULL,
        CONSTRAINT `FK_MouvementsCaisses_Devises_DeviseId` FOREIGN KEY (`DeviseId`) REFERENCES `Devises` (`IdDevise`) ON DELETE CASCADE,
        CONSTRAINT `FK_MouvementsCaisses_JetonsRetraits_JetonRetraitId` FOREIGN KEY (`JetonRetraitId`) REFERENCES `JetonsRetraits` (`IdJeton`) ON DELETE SET NULL,
        CONSTRAINT `FK_MouvementsCaisses_SessionsCaisses_SessionCaisseId` FOREIGN KEY (`SessionCaisseId`) REFERENCES `SessionsCaisses` (`IdSessionCaisse`) ON DELETE CASCADE,
        CONSTRAINT `FK_MouvementsCaisses_Utilisateurs_UtilisateurId` FOREIGN KEY (`UtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE RESTRICT,
        CONSTRAINT `FK_MouvementsCaisses_WalletMouvements_WalletMouvementId` FOREIGN KEY (`WalletMouvementId`) REFERENCES `WalletMouvements` (`IdWalletMouvement`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_JetonsRetraits_OperateurUtilisateurId` ON `JetonsRetraits` (`OperateurUtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_DemandesRetraitAgents_OperateurPaiementUtilisateurId` ON `DemandesRetraitAgents` (`OperateurPaiementUtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_DemandesRetraitAgents_WalletMouvementId` ON `DemandesRetraitAgents` (`WalletMouvementId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_CollecteId` ON `MouvementsCaisses` (`CollecteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_DemandeRetraitId` ON `MouvementsCaisses` (`DemandeRetraitId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_DeviseId` ON `MouvementsCaisses` (`DeviseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_JetonRetraitId` ON `MouvementsCaisses` (`JetonRetraitId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_SessionCaisseId` ON `MouvementsCaisses` (`SessionCaisseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_UtilisateurId` ON `MouvementsCaisses` (`UtilisateurId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_MouvementsCaisses_WalletMouvementId` ON `MouvementsCaisses` (`WalletMouvementId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_SessionsCaisses_DeviseId` ON `SessionsCaisses` (`DeviseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    CREATE INDEX `IX_SessionsCaisses_UtilisateurId_Statut` ON `SessionsCaisses` (`UtilisateurId`, `Statut`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    ALTER TABLE `DemandesRetraitAgents` ADD CONSTRAINT `FK_DemandesRetraitAgents_Utilisateurs_OperateurPaiementUtilisat~` FOREIGN KEY (`OperateurPaiementUtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    ALTER TABLE `DemandesRetraitAgents` ADD CONSTRAINT `FK_DemandesRetraitAgents_WalletMouvements_WalletMouvementId` FOREIGN KEY (`WalletMouvementId`) REFERENCES `WalletMouvements` (`IdWalletMouvement`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    ALTER TABLE `JetonsRetraits` ADD CONSTRAINT `FK_JetonsRetraits_Utilisateurs_OperateurUtilisateurId` FOREIGN KEY (`OperateurUtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260626093053_CaisseSessionRetraitAgent') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260626093053_CaisseSessionRetraitAgent', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

