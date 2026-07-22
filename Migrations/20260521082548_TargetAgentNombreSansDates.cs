using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class TargetAgentNombreSansDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE `TargetsAgents` CHANGE `MontantTarget` `Nombre` int NOT NULL;");

            migrationBuilder.DropColumn(
                name: "DateDebut",
                table: "TargetsAgents");

            migrationBuilder.DropColumn(
                name: "DateFin",
                table: "TargetsAgents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDebut",
                table: "TargetsAgents",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFin",
                table: "TargetsAgents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql(
                "ALTER TABLE `TargetsAgents` CHANGE `Nombre` `MontantTarget` decimal(18,2) NOT NULL;");
        }
    }
}
