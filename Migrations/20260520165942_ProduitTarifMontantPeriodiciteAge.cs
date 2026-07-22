using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class ProduitTarifMontantPeriodiciteAge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrixMensuel",
                table: "ProduitsMutuels",
                newName: "Montant");

            migrationBuilder.RenameColumn(
                name: "PrixMensuel",
                table: "ProduitsAssureurs",
                newName: "Montant");

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
                name: "AgeMax",
                table: "ProduitsMutuels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AgeMin",
                table: "ProduitsMutuels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Periodicite",
                table: "ProduitsMutuels",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Mensuel")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AgeMax",
                table: "ProduitsAssureurs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AgeMin",
                table: "ProduitsAssureurs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Periodicite",
                table: "ProduitsAssureurs",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Mensuel")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeMax",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "AgeMin",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "Periodicite",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "AgeMax",
                table: "ProduitsAssureurs");

            migrationBuilder.DropColumn(
                name: "AgeMin",
                table: "ProduitsAssureurs");

            migrationBuilder.DropColumn(
                name: "Periodicite",
                table: "ProduitsAssureurs");

            migrationBuilder.RenameColumn(
                name: "Montant",
                table: "ProduitsMutuels",
                newName: "PrixMensuel");

            migrationBuilder.RenameColumn(
                name: "Montant",
                table: "ProduitsAssureurs",
                newName: "PrixMensuel");

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
