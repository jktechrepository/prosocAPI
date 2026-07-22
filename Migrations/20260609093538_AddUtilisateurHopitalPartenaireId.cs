using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddUtilisateurHopitalPartenaireId : Migration
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
                name: "HopitalPartenaireId",
                table: "Utilisateurs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_HopitalPartenaireId",
                table: "Utilisateurs",
                column: "HopitalPartenaireId");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_HopitalPartenaires_HopitalPartenaireId",
                table: "Utilisateurs",
                column: "HopitalPartenaireId",
                principalTable: "HopitalPartenaires",
                principalColumn: "IdHopital",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_HopitalPartenaires_HopitalPartenaireId",
                table: "Utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_HopitalPartenaireId",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "HopitalPartenaireId",
                table: "Utilisateurs");

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
