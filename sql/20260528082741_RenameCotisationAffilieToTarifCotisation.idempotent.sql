START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `ArrieresAffilie` DROP FOREIGN KEY `FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `Collectes` DROP FOREIGN KEY `FK_Collectes_CotisationsAffilie_CotisationAffilieId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `Collectes` RENAME COLUMN `CotisationAffilieId` TO `TarifCotisationId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `Collectes` DROP INDEX `IX_Collectes_CotisationAffilieId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    CREATE INDEX `IX_Collectes_TarifCotisationId` ON `Collectes` (`TarifCotisationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `ArrieresAffilie` RENAME COLUMN `CotisationAffilieId` TO `TarifCotisationId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `ArrieresAffilie` DROP INDEX `IX_ArrieresAffilie_CotisationAffilieId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    CREATE INDEX `IX_ArrieresAffilie_TarifCotisationId` ON `ArrieresAffilie` (`TarifCotisationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `CotisationsAffilie` RENAME `TarifsCotisation`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `TarifsCotisation` DROP INDEX `IX_CotisationsAffilie_TypeAdhesionId_Periodicite`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    CREATE UNIQUE INDEX `IX_TarifsCotisation_TypeAdhesionId_Periodicite` ON `TarifsCotisation` (`TypeAdhesionId`, `Periodicite`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `ArrieresAffilie` ADD CONSTRAINT `FK_ArrieresAffilie_TarifsCotisation_TarifCotisationId` FOREIGN KEY (`TarifCotisationId`) REFERENCES `TarifsCotisation` (`IdCotisationAffilie`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_TarifsCotisation_TarifCotisationId` FOREIGN KEY (`TarifCotisationId`) REFERENCES `TarifsCotisation` (`IdCotisationAffilie`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260528082741_RenameCotisationAffilieToTarifCotisation') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260528082741_RenameCotisationAffilieToTarifCotisation', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

