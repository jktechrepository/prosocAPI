START TRANSACTION;

ALTER TABLE `ArrieresAffilie` DROP FOREIGN KEY `FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId`;

ALTER TABLE `Collectes` DROP FOREIGN KEY `FK_Collectes_CotisationsAffilie_CotisationAffilieId`;

ALTER TABLE `Collectes` RENAME COLUMN `CotisationAffilieId` TO `TarifCotisationId`;

ALTER TABLE `Collectes` DROP INDEX `IX_Collectes_CotisationAffilieId`;

CREATE INDEX `IX_Collectes_TarifCotisationId` ON `Collectes` (`TarifCotisationId`);

ALTER TABLE `ArrieresAffilie` RENAME COLUMN `CotisationAffilieId` TO `TarifCotisationId`;

ALTER TABLE `ArrieresAffilie` DROP INDEX `IX_ArrieresAffilie_CotisationAffilieId`;

CREATE INDEX `IX_ArrieresAffilie_TarifCotisationId` ON `ArrieresAffilie` (`TarifCotisationId`);

ALTER TABLE `CotisationsAffilie` RENAME `TarifsCotisation`;

ALTER TABLE `TarifsCotisation` DROP INDEX `IX_CotisationsAffilie_TypeAdhesionId_Periodicite`;

CREATE UNIQUE INDEX `IX_TarifsCotisation_TypeAdhesionId_Periodicite` ON `TarifsCotisation` (`TypeAdhesionId`, `Periodicite`);

ALTER TABLE `ArrieresAffilie` ADD CONSTRAINT `FK_ArrieresAffilie_TarifsCotisation_TarifCotisationId` FOREIGN KEY (`TarifCotisationId`) REFERENCES `TarifsCotisation` (`IdCotisationAffilie`) ON DELETE RESTRICT;

ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_TarifsCotisation_TarifCotisationId` FOREIGN KEY (`TarifCotisationId`) REFERENCES `TarifsCotisation` (`IdCotisationAffilie`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260528082741_RenameCotisationAffilieToTarifCotisation', '6.0.25');

COMMIT;

