using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AdhesionAgentIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adhesions_Agents_AgentId",
                table: "Adhesions");

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

            migrationBuilder.AlterColumn<int>(
                name: "AgentId",
                table: "Adhesions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Adhesions_Agents_AgentId",
                table: "Adhesions",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adhesions_Agents_AgentId",
                table: "Adhesions");

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

            migrationBuilder.AlterColumn<int>(
                name: "AgentId",
                table: "Adhesions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Adhesions_Agents_AgentId",
                table: "Adhesions",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
