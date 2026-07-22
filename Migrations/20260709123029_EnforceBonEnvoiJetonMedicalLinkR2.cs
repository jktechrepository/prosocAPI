using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class EnforceBonEnvoiJetonMedicalLinkR2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE BonsEnvoi b
INNER JOIN DemandesBonEnvoi d ON d.BonEnvoiId = b.IdBonEnvoi
SET b.JetonMedicalId = d.JetonMedicalId
WHERE b.JetonMedicalId IS NULL
  AND d.JetonMedicalId IS NOT NULL;
");

            migrationBuilder.Sql(@"
SET @nb_unlinked_bons := (SELECT COUNT(*) FROM BonsEnvoi WHERE JetonMedicalId IS NULL);
SET @sql := IF(@nb_unlinked_bons > 0,
    'SIGNAL SQLSTATE ''45000'' SET MESSAGE_TEXT = ''R2 aborted: BonsEnvoi sans JetonMedicalId'';',
    'SELECT 1;');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.AlterColumn<int>(
                name: "JetonMedicalId",
                table: "BonsEnvoi",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "JetonMedicalId",
                table: "BonsEnvoi",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
