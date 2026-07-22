using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class WalletVirtuelMouvementEnrichi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviseId",
                table: "WalletVirtuelMouvements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperateurUtilisateurId",
                table: "WalletVirtuelMouvements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldeApres",
                table: "WalletVirtuelMouvements",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldeAvant",
                table: "WalletVirtuelMouvements",
                type: "decimal(18,2)",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_WalletVirtuelMouvements_DeviseId",
                table: "WalletVirtuelMouvements",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletVirtuelMouvements_OperateurUtilisateurId",
                table: "WalletVirtuelMouvements",
                column: "OperateurUtilisateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletVirtuelMouvements_Devises_DeviseId",
                table: "WalletVirtuelMouvements",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletVirtuelMouvements_Utilisateurs_OperateurUtilisateurId",
                table: "WalletVirtuelMouvements",
                column: "OperateurUtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletVirtuelMouvements_Devises_DeviseId",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletVirtuelMouvements_Utilisateurs_OperateurUtilisateurId",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropIndex(
                name: "IX_WalletVirtuelMouvements_DeviseId",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropIndex(
                name: "IX_WalletVirtuelMouvements_OperateurUtilisateurId",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropColumn(
                name: "OperateurUtilisateurId",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropColumn(
                name: "SoldeApres",
                table: "WalletVirtuelMouvements");

            migrationBuilder.DropColumn(
                name: "SoldeAvant",
                table: "WalletVirtuelMouvements");

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
