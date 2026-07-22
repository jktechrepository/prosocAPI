using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddCategorieAgentCode : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "LibelleCategorie",
                table: "CategoriesAgents",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CategoriesAgents",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "CategoriesAgents",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"
UPDATE CategoriesAgents
SET Code = UPPER(TRIM(
    CASE
        WHEN LibelleCategorie LIKE '%(%)%'
            THEN SUBSTRING_INDEX(SUBSTRING_INDEX(LibelleCategorie, '(', -1), ')', 1)
        WHEN LibelleCategorie NOT LIKE '% %' AND CHAR_LENGTH(TRIM(LibelleCategorie)) <= 10
            THEN LibelleCategorie
        ELSE LibelleCategorie
    END
))
WHERE Code IS NULL OR TRIM(Code) = '';
");

            migrationBuilder.Sql(@"
UPDATE CategoriesAgents
SET Description = CASE UPPER(TRIM(Code))
        WHEN 'AT' THEN 'Agent de Terrain'
        WHEN 'AA' THEN 'Agent Administratif'
        WHEN 'AP' THEN 'Agent Percepteur'
        WHEN 'AS' THEN 'Agent Superviseur'
        WHEN 'CA' THEN 'Caissier'
        WHEN 'AH' THEN 'Agent Hôpital'
        WHEN 'FI' THEN 'Financier'
        WHEN 'IT' THEN 'Technicien'
        WHEN 'AD' THEN 'Admin'
        ELSE COALESCE(NULLIF(TRIM(Description), ''), UPPER(TRIM(Code)))
    END,
    LibelleCategorie = CONCAT(
        CASE UPPER(TRIM(Code))
            WHEN 'AT' THEN 'Agent de Terrain'
            WHEN 'AA' THEN 'Agent Administratif'
            WHEN 'AP' THEN 'Agent Percepteur'
            WHEN 'AS' THEN 'Agent Superviseur'
            WHEN 'CA' THEN 'Caissier'
            WHEN 'AH' THEN 'Agent Hôpital'
            WHEN 'FI' THEN 'Financier'
            WHEN 'IT' THEN 'Technicien'
            WHEN 'AD' THEN 'Admin'
            ELSE COALESCE(NULLIF(TRIM(Description), ''), UPPER(TRIM(Code)))
        END,
        ' (',
        UPPER(TRIM(Code)),
        ')'
    ),
    DateModification = NOW()
WHERE Code IS NOT NULL AND TRIM(Code) <> '';
");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CategoriesAgents",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "CategoriesAgents");

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

            migrationBuilder.AlterColumn<string>(
                name: "LibelleCategorie",
                table: "CategoriesAgents",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CategoriesAgents",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
