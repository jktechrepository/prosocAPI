using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddBonEnvoiJetonMedicalLinkR1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JetonMedicalId",
                table: "BonsEnvoi",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE BonsEnvoi b
INNER JOIN DemandesBonEnvoi d ON d.BonEnvoiId = b.IdBonEnvoi
SET b.JetonMedicalId = d.JetonMedicalId
WHERE b.JetonMedicalId IS NULL
  AND d.JetonMedicalId IS NOT NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_BonsEnvoi_JetonMedicalId",
                table: "BonsEnvoi",
                column: "JetonMedicalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BonsEnvoi_JetonsMedicaux_JetonMedicalId",
                table: "BonsEnvoi",
                column: "JetonMedicalId",
                principalTable: "JetonsMedicaux",
                principalColumn: "IdJeton",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsEnvoi_JetonsMedicaux_JetonMedicalId",
                table: "BonsEnvoi");

            migrationBuilder.DropIndex(
                name: "IX_BonsEnvoi_JetonMedicalId",
                table: "BonsEnvoi");

            migrationBuilder.DropColumn(
                name: "JetonMedicalId",
                table: "BonsEnvoi");
        }
    }
}
