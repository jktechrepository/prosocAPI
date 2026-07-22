using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddCollecteOperateurUtilisateurId : Migration
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
                name: "OperateurUtilisateurId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_OperateurUtilisateurId",
                table: "Collectes",
                column: "OperateurUtilisateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_Utilisateurs_OperateurUtilisateurId",
                table: "Collectes",
                column: "OperateurUtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_Utilisateurs_OperateurUtilisateurId",
                table: "Collectes");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_OperateurUtilisateurId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "OperateurUtilisateurId",
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
