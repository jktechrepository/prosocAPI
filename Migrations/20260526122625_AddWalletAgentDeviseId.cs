using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddWalletAgentDeviseId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviseId",
                table: "WalletsAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviseId",
                table: "WalletMouvements",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE WalletsAgents w
                SET w.DeviseId = (
                    SELECT d.IdDevise FROM Devises d
                    WHERE d.EstDevisePrincipale = 1 AND d.Statut = 1
                    ORDER BY d.IdDevise
                    LIMIT 1
                )
                WHERE w.DeviseId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE WalletMouvements m
                INNER JOIN WalletsAgents w ON m.WalletId = w.IdWalletAgent
                SET m.DeviseId = w.DeviseId
                WHERE m.DeviseId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE WalletMouvements m
                SET m.DeviseId = (
                    SELECT d.IdDevise FROM Devises d
                    WHERE d.EstDevisePrincipale = 1 AND d.Statut = 1
                    ORDER BY d.IdDevise
                    LIMIT 1
                )
                WHERE m.DeviseId IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "DeviseId",
                table: "WalletsAgents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeviseId",
                table: "WalletMouvements",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletsAgents_AgentId_DeviseId",
                table: "WalletsAgents",
                columns: new[] { "AgentId", "DeviseId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_WalletsAgents_AgentId",
                table: "WalletsAgents");

            migrationBuilder.CreateIndex(
                name: "IX_WalletsAgents_DeviseId",
                table: "WalletsAgents",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletMouvements_DeviseId",
                table: "WalletMouvements",
                column: "DeviseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletMouvements_Devises_DeviseId",
                table: "WalletMouvements",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletsAgents_Devises_DeviseId",
                table: "WalletsAgents",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletMouvements_Devises_DeviseId",
                table: "WalletMouvements");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletsAgents_Devises_DeviseId",
                table: "WalletsAgents");

            migrationBuilder.DropIndex(
                name: "IX_WalletsAgents_AgentId_DeviseId",
                table: "WalletsAgents");

            migrationBuilder.DropIndex(
                name: "IX_WalletsAgents_DeviseId",
                table: "WalletsAgents");

            migrationBuilder.DropIndex(
                name: "IX_WalletMouvements_DeviseId",
                table: "WalletMouvements");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "WalletsAgents");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "WalletMouvements");

            migrationBuilder.CreateIndex(
                name: "IX_WalletsAgents_AgentId",
                table: "WalletsAgents",
                column: "AgentId",
                unique: true);
        }
    }
}
