using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddHybridCommissionRates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommission",
                table: "ProduitsMutuels",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 25m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionMutuelle",
                table: "ProduitsAssureurs",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "TauxCommission",
                table: "Frais",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 25m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TauxCommission",
                table: "ProduitsMutuels");

            migrationBuilder.DropColumn(
                name: "TauxCommission",
                table: "Frais");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionMutuelle",
                table: "ProduitsAssureurs",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");
        }
    }
}
