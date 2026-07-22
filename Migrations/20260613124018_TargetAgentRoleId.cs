using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class TargetAgentRoleId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TargetsAgents_Agents_AgentId",
                table: "TargetsAgents");

            migrationBuilder.DropIndex(
                name: "IX_TargetsAgents_AgentId",
                table: "TargetsAgents");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "TargetsAgents",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE TargetsAgents t
LEFT JOIN Utilisateurs u ON u.AgentId = t.AgentId AND u.RoleId IS NOT NULL
LEFT JOIN Agents a ON a.IdAgent = t.AgentId
LEFT JOIN Roles r ON r.Nom = COALESCE(
    (SELECT r2.Nom FROM Roles r2 WHERE r2.IdRole = u.RoleId LIMIT 1),
    a.RoleAgent
)
SET t.RoleId = r.IdRole
WHERE r.IdRole IS NOT NULL;
");

            migrationBuilder.Sql(@"
UPDATE TargetsAgents t
SET t.RoleId = (SELECT IdRole FROM Roles WHERE Nom = 'Agent (AT)' LIMIT 1)
WHERE t.RoleId IS NULL;
");

            migrationBuilder.Sql(@"
DELETE t1 FROM TargetsAgents t1
INNER JOIN TargetsAgents t2
    ON t1.RoleId = t2.RoleId
    AND t1.Periodicite = t2.Periodicite
    AND t1.IdTargetAgent < t2.IdTargetAgent;
");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "TargetsAgents");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "TargetsAgents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_TargetsAgents_RoleId_Periodicite",
                table: "TargetsAgents",
                columns: new[] { "RoleId", "Periodicite" });

            migrationBuilder.AddForeignKey(
                name: "FK_TargetsAgents_Roles_RoleId",
                table: "TargetsAgents",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "IdRole",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TargetsAgents_Roles_RoleId",
                table: "TargetsAgents");

            migrationBuilder.DropIndex(
                name: "IX_TargetsAgents_RoleId_Periodicite",
                table: "TargetsAgents");

            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "TargetsAgents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "TargetsAgents");

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

            migrationBuilder.CreateIndex(
                name: "IX_TargetsAgents_AgentId",
                table: "TargetsAgents",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TargetsAgents_Agents_AgentId",
                table: "TargetsAgents",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "IdAgent",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
