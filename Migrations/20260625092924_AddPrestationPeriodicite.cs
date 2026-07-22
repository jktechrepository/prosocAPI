using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddPrestationPeriodicite : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "Periodicite",
                table: "Prestations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Mensuel")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
UPDATE Prestations p
LEFT JOIN ProduitsMutuels pm ON pm.IdProduit = p.ProduitMutuelId
LEFT JOIN ProduitsAssureurs pa ON pa.IdProduit = p.ProduitAssureurId
SET p.Periodicite = COALESCE(NULLIF(pm.Periodicite, ''), NULLIF(pa.Periodicite, ''), 'Mensuel')
WHERE p.Periodicite IS NULL OR p.Periodicite = '' OR p.Periodicite = 'Mensuel';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Periodicite",
                table: "Prestations");

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
