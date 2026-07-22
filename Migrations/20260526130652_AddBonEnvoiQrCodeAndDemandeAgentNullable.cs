using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddBonEnvoiQrCodeAndDemandeAgentNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandesBonEnvoi_Agents_AgentId",
                table: "DemandesBonEnvoi");

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
                table: "DemandesBonEnvoi",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "QrCodeImageBase64",
                table: "BonsEnvoi",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "QrCodePayload",
                table: "BonsEnvoi",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesBonEnvoi_Agents_AgentId",
                table: "DemandesBonEnvoi",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandesBonEnvoi_Agents_AgentId",
                table: "DemandesBonEnvoi");

            migrationBuilder.DropColumn(
                name: "QrCodeImageBase64",
                table: "BonsEnvoi");

            migrationBuilder.DropColumn(
                name: "QrCodePayload",
                table: "BonsEnvoi");

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
                table: "DemandesBonEnvoi",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesBonEnvoi_Agents_AgentId",
                table: "DemandesBonEnvoi",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
