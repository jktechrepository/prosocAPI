using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class RenameCotisationAffilieToTarifCotisation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent : tolère un schéma déjà renommé sans ligne dans __EFMigrationsHistory.
            migrationBuilder.Sql(@"
SET @db = DATABASE();

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'ArrieresAffilie'
       AND CONSTRAINT_NAME = 'FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId'
       AND CONSTRAINT_TYPE = 'FOREIGN KEY') > 0,
    'ALTER TABLE `ArrieresAffilie` DROP FOREIGN KEY `FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes'
       AND CONSTRAINT_NAME = 'FK_Collectes_CotisationsAffilie_CotisationAffilieId'
       AND CONSTRAINT_TYPE = 'FOREIGN KEY') > 0,
    'ALTER TABLE `Collectes` DROP FOREIGN KEY `FK_Collectes_CotisationsAffilie_CotisationAffilieId`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'CotisationAffilieId') > 0,
    'ALTER TABLE `Collectes` RENAME COLUMN `CotisationAffilieId` TO `TarifCotisationId`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes'
       AND INDEX_NAME = 'IX_Collectes_CotisationAffilieId') > 0,
    'ALTER TABLE `Collectes` RENAME INDEX `IX_Collectes_CotisationAffilieId` TO `IX_Collectes_TarifCotisationId`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'ArrieresAffilie' AND COLUMN_NAME = 'CotisationAffilieId') > 0,
    'ALTER TABLE `ArrieresAffilie` RENAME COLUMN `CotisationAffilieId` TO `TarifCotisationId`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'ArrieresAffilie'
       AND INDEX_NAME = 'IX_ArrieresAffilie_CotisationAffilieId') > 0,
    'ALTER TABLE `ArrieresAffilie` RENAME INDEX `IX_ArrieresAffilie_CotisationAffilieId` TO `IX_ArrieresAffilie_TarifCotisationId`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'CotisationsAffilie') > 0
    AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
         WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation') = 0,
    'RENAME TABLE `CotisationsAffilie` TO `TarifsCotisation`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation'
       AND INDEX_NAME = 'IX_CotisationsAffilie_TypeAdhesionId_Periodicite') > 0,
    'ALTER TABLE `TarifsCotisation` RENAME INDEX `IX_CotisationsAffilie_TypeAdhesionId_Periodicite` TO `IX_TarifsCotisation_TypeAdhesionId_Periodicite`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'ArrieresAffilie'
       AND CONSTRAINT_NAME = 'FK_ArrieresAffilie_TarifsCotisation_TarifCotisationId') = 0
    AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
         WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'ArrieresAffilie' AND COLUMN_NAME = 'TarifCotisationId') > 0
    AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
         WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation') > 0,
    'ALTER TABLE `ArrieresAffilie` ADD CONSTRAINT `FK_ArrieresAffilie_TarifsCotisation_TarifCotisationId` FOREIGN KEY (`TarifCotisationId`) REFERENCES `TarifsCotisation` (`IdCotisationAffilie`) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
     WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes'
       AND CONSTRAINT_NAME = 'FK_Collectes_TarifsCotisation_TarifCotisationId') = 0
    AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
         WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'TarifCotisationId') > 0
    AND (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
         WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TarifsCotisation') > 0,
    'ALTER TABLE `Collectes` ADD CONSTRAINT `FK_Collectes_TarifsCotisation_TarifCotisationId` FOREIGN KEY (`TarifCotisationId`) REFERENCES `TarifsCotisation` (`IdCotisationAffilie`) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArrieresAffilie_TarifsCotisation_TarifCotisationId",
                table: "ArrieresAffilie");

            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_TarifsCotisation_TarifCotisationId",
                table: "Collectes");

            migrationBuilder.RenameColumn(
                name: "TarifCotisationId",
                table: "Collectes",
                newName: "CotisationAffilieId");

            migrationBuilder.RenameIndex(
                name: "IX_Collectes_TarifCotisationId",
                table: "Collectes",
                newName: "IX_Collectes_CotisationAffilieId");

            migrationBuilder.RenameColumn(
                name: "TarifCotisationId",
                table: "ArrieresAffilie",
                newName: "CotisationAffilieId");

            migrationBuilder.RenameIndex(
                name: "IX_ArrieresAffilie_TarifCotisationId",
                table: "ArrieresAffilie",
                newName: "IX_ArrieresAffilie_CotisationAffilieId");

            migrationBuilder.RenameTable(
                name: "TarifsCotisation",
                newName: "CotisationsAffilie");

            migrationBuilder.RenameIndex(
                name: "IX_TarifsCotisation_TypeAdhesionId_Periodicite",
                table: "CotisationsAffilie",
                newName: "IX_CotisationsAffilie_TypeAdhesionId_Periodicite");

            migrationBuilder.AddForeignKey(
                name: "FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId",
                table: "ArrieresAffilie",
                column: "CotisationAffilieId",
                principalTable: "CotisationsAffilie",
                principalColumn: "IdCotisationAffilie",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_CotisationsAffilie_CotisationAffilieId",
                table: "Collectes",
                column: "CotisationAffilieId",
                principalTable: "CotisationsAffilie",
                principalColumn: "IdCotisationAffilie",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
