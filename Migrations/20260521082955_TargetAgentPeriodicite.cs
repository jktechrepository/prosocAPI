using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class TargetAgentPeriodicite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Periodicite",
                table: "TargetsAgents",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.Sql(
                "UPDATE `TargetsAgents` SET `Periodicite` = 1, `Nombre` = 5 WHERE `Nombre` = 5;" +
                "UPDATE `TargetsAgents` SET `Periodicite` = 2, `Nombre` = 25 WHERE `Nombre` = 25;" +
                "UPDATE `TargetsAgents` SET `Periodicite` = 3, `Nombre` = 100 WHERE `Nombre` NOT IN (5, 25);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Periodicite",
                table: "TargetsAgents");
        }
    }
}
