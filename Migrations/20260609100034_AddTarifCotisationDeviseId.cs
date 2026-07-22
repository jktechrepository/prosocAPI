using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddTarifCotisationDeviseId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "WalletsAgents",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldRowVersion: true,
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<int>(
                name: "DeviseId",
                table: "TarifsCotisation",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE `TarifsCotisation` tc
SET tc.`DeviseId` = (
    SELECT d.`IdDevise` FROM `Devises` d
    WHERE d.`Code` = 'USD' AND d.`Statut` = 1
    ORDER BY d.`IdDevise` LIMIT 1
)
WHERE tc.`DeviseId` IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE `TarifsCotisation` tc
SET tc.`DeviseId` = (SELECT d.`IdDevise` FROM `Devises` d ORDER BY d.`IdDevise` LIMIT 1)
WHERE tc.`DeviseId` IS NULL;
");

            migrationBuilder.AlterColumn<int>(
                name: "DeviseId",
                table: "TarifsCotisation",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TarifsCotisation_DeviseId",
                table: "TarifsCotisation",
                column: "DeviseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TarifsCotisation_Devises_DeviseId",
                table: "TarifsCotisation",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TarifsCotisation_Devises_DeviseId",
                table: "TarifsCotisation");

            migrationBuilder.DropIndex(
                name: "IX_TarifsCotisation_DeviseId",
                table: "TarifsCotisation");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "TarifsCotisation");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RowVersion",
                table: "WalletsAgents",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }
    }
}
