-- Script pour renommer la colonne Id en IdTargetAgent dans la table TargetsAgents
-- Exécuter ce script directement dans votre client MySQL

-- Renommer la colonne principale
ALTER TABLE `TargetsAgents` CHANGE COLUMN `Id` `IdTargetAgent` int NOT NULL AUTO_INCREMENT;

-- Mettre à jour la clé primaire si nécessaire
ALTER TABLE `TargetsAgents` DROP PRIMARY KEY;
ALTER TABLE `TargetsAgents` ADD PRIMARY KEY (`IdTargetAgent`);

-- Marquer la migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260302095436_RenameTargetAgentIdToIdTargetAgent', '6.0.25');

-- Vérification
SELECT 'Colonne Id renommée en IdTargetAgent avec succès!' as Result;
DESCRIBE `TargetsAgents`;
SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302095436_RenameTargetAgentIdToIdTargetAgent';
