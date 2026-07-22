using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class RemoveAgentSuperviseurId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agents_Agents_SuperviseurId",
                table: "Agents");

            migrationBuilder.DropIndex(
                name: "IX_Agents_SuperviseurId",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "SuperviseurId",
                table: "Agents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SuperviseurId",
                table: "Agents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_SuperviseurId",
                table: "Agents",
                column: "SuperviseurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_Agents_SuperviseurId",
                table: "Agents",
                column: "SuperviseurId",
                principalTable: "Agents",
                principalColumn: "IdAgent");
        }
    }
}
