-- =============================================================================
-- Migration : 20260711102938_AddParametresMetier
-- Objectif  : Table ParametresMetier (config métier éditable Admin/IT)
-- Idempotent: vérifie __EFMigrationsHistory — réexécutable sans erreur
-- =============================================================================
--
-- Prérequis :
--   - MySQL / MariaDB (base ProsocAPI)
--   - Table Utilisateurs existante (FK ModifieParUtilisateurId)
--
-- Ordre UAT recommandé :
--   1. sql/MigrateParametresMetier.idempotent.sql          (ce fichier — crée la table)
--   2. sql/MigrateParametresMetierPermissions.idempotent.sql
--   3. sql/SeedParametresMetierRetraitAgent.idempotent.sql
--
-- Alternative tout-en-un : sql/DeployParametresMetierUat.idempotent.sql
--
-- Référence EF : Migrations/20260711102938_AddParametresMetier.cs
-- =============================================================================

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260711102938_AddParametresMetier') THEN

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260711102938_AddParametresMetier') THEN
    IF NOT EXISTS(
        SELECT 1 FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ParametresMetier'
    ) THEN

    CREATE TABLE `ParametresMetier` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Code` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ValeurJson` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DateCreation` datetime(6) NOT NULL,
        `DateModification` datetime(6) NULL,
        `ModifieParUtilisateurId` int NULL,
        CONSTRAINT `PK_ParametresMetier` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ParametresMetier_Utilisateurs_ModifieParUtilisateurId`
            FOREIGN KEY (`ModifieParUtilisateurId`) REFERENCES `Utilisateurs` (`IdUtilisateur`) ON DELETE SET NULL
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260711102938_AddParametresMetier') THEN
    IF NOT EXISTS(
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'ParametresMetier'
          AND INDEX_NAME = 'IX_ParametresMetier_Code'
    ) THEN

    CREATE UNIQUE INDEX `IX_ParametresMetier_Code` ON `ParametresMetier` (`Code`);

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260711102938_AddParametresMetier') THEN
    IF NOT EXISTS(
        SELECT 1 FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'ParametresMetier'
          AND INDEX_NAME = 'IX_ParametresMetier_ModifieParUtilisateurId'
    ) THEN

    CREATE INDEX `IX_ParametresMetier_ModifieParUtilisateurId` ON `ParametresMetier` (`ModifieParUtilisateurId`);

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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260711102938_AddParametresMetier') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260711102938_AddParametresMetier', '6.0.25');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

SELECT '✅ Table ParametresMetier créée (migration 20260711102938). Exécuter ensuite le seed et les permissions.' AS Resultat;
