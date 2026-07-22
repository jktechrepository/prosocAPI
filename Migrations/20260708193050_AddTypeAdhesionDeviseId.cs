using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddTypeAdhesionDeviseId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviseId",
                table: "TypeAdhesions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE TypeAdhesions
SET DeviseId = (
    SELECT d.IdDevise
    FROM Devises d
    WHERE d.EstDevisePrincipale = TRUE AND d.Statut = TRUE
    LIMIT 1
)
WHERE DeviseId IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE TypeAdhesions
SET DeviseId = (
    SELECT d.IdDevise
    FROM Devises d
    WHERE d.Statut = TRUE
    ORDER BY d.IdDevise
    LIMIT 1
)
WHERE DeviseId IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE TypeAdhesions
SET DeviseId = (
    SELECT d.IdDevise
    FROM Devises d
    ORDER BY d.IdDevise
    LIMIT 1
)
WHERE DeviseId IS NULL;
");

            migrationBuilder.AlterColumn<int>(
                name: "DeviseId",
                table: "TypeAdhesions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TypeAdhesions_DeviseId",
                table: "TypeAdhesions",
                column: "DeviseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TypeAdhesions_Devises_DeviseId",
                table: "TypeAdhesions",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TypeAdhesions_Devises_DeviseId",
                table: "TypeAdhesions");

            migrationBuilder.DropIndex(
                name: "IX_TypeAdhesions_DeviseId",
                table: "TypeAdhesions");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "TypeAdhesions");
        }
    }
}
