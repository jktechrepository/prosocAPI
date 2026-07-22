using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddUniqueActiveLibelleTarifCotisation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LibelleTarifCotisationNormalized",
                table: "TarifsCotisation",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
UPDATE TarifsCotisation
SET LibelleTarifCotisationNormalized = CASE
    WHEN Statut = TRUE
         AND LibelleTarifCotisation IS NOT NULL
         AND TRIM(LibelleTarifCotisation) <> ''
    THEN LOWER(TRIM(LibelleTarifCotisation))
    ELSE NULL
END;
");

            migrationBuilder.CreateIndex(
                name: "IX_TarifsCotisation_LibelleTarifCotisationNormalized",
                table: "TarifsCotisation",
                column: "LibelleTarifCotisationNormalized",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TarifsCotisation_LibelleTarifCotisationNormalized",
                table: "TarifsCotisation");

            migrationBuilder.DropColumn(
                name: "LibelleTarifCotisationNormalized",
                table: "TarifsCotisation");
        }
    }
}
