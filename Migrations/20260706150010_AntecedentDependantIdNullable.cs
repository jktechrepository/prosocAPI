using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AntecedentDependantIdNullable : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "DependantId",
                table: "Antecedants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Antecedants_DependantId",
                table: "Antecedants",
                column: "DependantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Antecedants_Dependants_DependantId",
                table: "Antecedants",
                column: "DependantId",
                principalTable: "Dependants",
                principalColumn: "IdDependant");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Antecedants_Dependants_DependantId",
                table: "Antecedants");

            migrationBuilder.DropIndex(
                name: "IX_Antecedants_DependantId",
                table: "Antecedants");

            migrationBuilder.DropColumn(
                name: "DependantId",
                table: "Antecedants");

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
