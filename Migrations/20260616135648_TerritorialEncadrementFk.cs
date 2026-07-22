using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class TerritorialEncadrementFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChefEquipeAgentId",
                table: "ZonesSociales",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "SuperviseurAgentId",
                table: "Communes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZonesSociales_ChefEquipeAgentId",
                table: "ZonesSociales",
                column: "ChefEquipeAgentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communes_SuperviseurAgentId",
                table: "Communes",
                column: "SuperviseurAgentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Communes_Agents_SuperviseurAgentId",
                table: "Communes",
                column: "SuperviseurAgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ZonesSociales_Agents_ChefEquipeAgentId",
                table: "ZonesSociales",
                column: "ChefEquipeAgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communes_Agents_SuperviseurAgentId",
                table: "Communes");

            migrationBuilder.DropForeignKey(
                name: "FK_ZonesSociales_Agents_ChefEquipeAgentId",
                table: "ZonesSociales");

            migrationBuilder.DropIndex(
                name: "IX_ZonesSociales_ChefEquipeAgentId",
                table: "ZonesSociales");

            migrationBuilder.DropIndex(
                name: "IX_Communes_SuperviseurAgentId",
                table: "Communes");

            migrationBuilder.DropColumn(
                name: "ChefEquipeAgentId",
                table: "ZonesSociales");

            migrationBuilder.DropColumn(
                name: "SuperviseurAgentId",
                table: "Communes");

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
