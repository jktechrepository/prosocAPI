using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddCotisationAffilieIdToCollecte : Migration
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
                name: "CotisationAffilieId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_CotisationAffilieId",
                table: "Collectes",
                column: "CotisationAffilieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_CotisationsAffilie_CotisationAffilieId",
                table: "Collectes",
                column: "CotisationAffilieId",
                principalTable: "CotisationsAffilie",
                principalColumn: "IdCotisationAffilie",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_CotisationsAffilie_CotisationAffilieId",
                table: "Collectes");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_CotisationAffilieId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "CotisationAffilieId",
                table: "Collectes");

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
