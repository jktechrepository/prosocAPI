-- =============================================================================
-- Déploiement UAT/prod : ParametresMetier (table + permissions + seed)
-- =============================================================================
-- Idempotent. À exécuter en UAT si la table n'existe pas encore (#1146).
-- Compatible phpMyAdmin (un seul fichier, pas de SOURCE).
--
-- Usage CLI :
--   mysql -h <host-uat> -u <user> -p uat-prosocdb < sql/DeployParametresMetierUat.idempotent.sql
-- =============================================================================

-- ─── 1/3 Migration table ─────────────────────────────────────────────────────

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
    IF NOT EXISTS(SELECT 1 FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ParametresMetier') THEN
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
    IF NOT EXISTS(SELECT 1 FROM information_schema.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ParametresMetier' AND INDEX_NAME = 'IX_ParametresMetier_Code') THEN
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
    IF NOT EXISTS(SELECT 1 FROM information_schema.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ParametresMetier' AND INDEX_NAME = 'IX_ParametresMetier_ModifieParUtilisateurId') THEN
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

-- ─── 2/3 Permissions ─────────────────────────────────────────────────────────

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'READ_PARAMETRES_METIER', 'Consulter les paramètres métier', 'PARAMETRES_METIER', 'READ', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'READ_PARAMETRES_METIER');

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT 'UPDATE_PARAMETRES_METIER', 'Modifier les paramètres métier', 'PARAMETRES_METIER', 'UPDATE', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'UPDATE_PARAMETRES_METIER');

SET @ReadId := (SELECT IdPermission FROM Permissions WHERE Nom = 'READ_PARAMETRES_METIER' AND Statut = 1 LIMIT 1);
SET @UpdateId := (SELECT IdPermission FROM Permissions WHERE Nom = 'UPDATE_PARAMETRES_METIER' AND Statut = 1 LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT r.IdRole, p.IdPermission, NOW()
FROM Roles r
CROSS JOIN (SELECT @ReadId AS IdPermission UNION ALL SELECT @UpdateId) p
WHERE r.Nom IN ('Admin', 'IT')
  AND p.IdPermission IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = r.IdRole AND rp.PermissionId = p.IdPermission);

COMMIT;

-- ─── 3/3 Seed ────────────────────────────────────────────────────────────────

START TRANSACTION;

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT 'RETRAIT_AGENT', '{"fenetre1Debut":15,"fenetre1Fin":20,"fenetre2DerniersJours":7,"montantMinimumPartiel":5}', NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'RETRAIT_AGENT');

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT 'AGENT_MAASH', '{"montantRetenueUsd":5,"deviseId":2,"codesCategoriesEligibles":["AT","AA","AP","AS","CA","FI","IT","AD"],"nomProduitMaash":"MAASH","retenueAutomatiqueActivee":true,"jourExecution":1,"heureExecution":2,"intervalleControleMinutes":60,"retenterEchecsQuotidiennement":true}', NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'AGENT_MAASH');

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT 'ARRIERES', '{"generationAutomatiqueActivee":true,"heureExecution":0,"minuteExecution":30,"intervalleControleMinutes":600,"jourEcheanceMensuelle":1}', NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'ARRIERES');

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT 'PENALITE', '{"applicationAutomatiqueActivee":true,"delaiGraceJours":3,"fraisPenaliteCode":"PENALITE_RETARD_COTISATION","retardCotisationActive":true}', NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'PENALITE');

COMMIT;

SELECT '✅ Déploiement ParametresMetier UAT terminé (table + permissions + seed).' AS Resultat;
