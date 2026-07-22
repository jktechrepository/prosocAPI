using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class ProduitQuatreTauxCommission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TauxCommission",
                table: "ProduitsMutuels",
                newName: "TauxCommissionAT");

            migrationBuilder.RenameColumn(
                name: "CommissionMutuelle",
                table: "ProduitsAssureurs",
                newName: "TauxCommissionAT");

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

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommissionAA",
                table: "ProduitsMutuels",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommissionAAMash",
                table: "ProduitsMutuels",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommissionAAStructure",
                table: "ProduitsMutuels",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommissionAA",
                table: "ProduitsAssureurs",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommissionAAMash",
                table: "ProduitsAssureurs",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommissionAAStructure",
                table: "ProduitsAssureurs",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TauxCommissionAA",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "TauxCommissionAAMash",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "TauxCommissionAAStructure",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "TauxCommissionAA",
                table: "ProduitsAssureurs");

            migrationBuilder.DropColumn(
                name: "TauxCommissionAAMash",
                table: "ProduitsAssureurs");

            migrationBuilder.DropColumn(
                name: "TauxCommissionAAStructure",
                table: "ProduitsAssureurs");

            migrationBuilder.RenameColumn(
                name: "TauxCommissionAT",
                table: "ProduitsMutuels",
                newName: "TauxCommission");

            migrationBuilder.RenameColumn(
                name: "TauxCommissionAT",
                table: "ProduitsAssureurs",
                newName: "CommissionMutuelle");

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
