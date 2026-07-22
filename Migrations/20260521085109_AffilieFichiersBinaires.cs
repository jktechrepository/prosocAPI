using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AffilieFichiersBinaires : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarteIdentiteUrl",
                table: "Affilies");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Affilies");

            migrationBuilder.AddColumn<string>(
                name: "CarteIdentiteContentType",
                table: "Affilies",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "CarteIdentiteData",
                table: "Affilies",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                table: "Affilies",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "PhotoData",
                table: "Affilies",
                type: "longblob",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarteIdentiteContentType",
                table: "Affilies");

            migrationBuilder.DropColumn(
                name: "CarteIdentiteData",
                table: "Affilies");

            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                table: "Affilies");

            migrationBuilder.DropColumn(
                name: "PhotoData",
                table: "Affilies");

            migrationBuilder.AddColumn<string>(
                name: "CarteIdentiteUrl",
                table: "Affilies",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Affilies",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
