-- Marquer la migration problématique comme appliquée
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260226162138_AddCompletePermissionsSystem', '6.0.25')
ON DUPLICATE KEY UPDATE `MigrationId` = `MigrationId`;
