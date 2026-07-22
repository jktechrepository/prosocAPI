using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddWalletVirtuelAgentDeviseId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviseId",
                table: "WalletsVirtuelsAgents",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE WalletsVirtuelsAgents w
                SET w.DeviseId = (
                    SELECT d.IdDevise FROM Devises d
                    WHERE d.EstDevisePrincipale = 1 AND d.Statut = 1
                    ORDER BY d.IdDevise
                    LIMIT 1
                )
                WHERE w.DeviseId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE WalletsVirtuelsAgents w
                SET w.DeviseId = (
                    SELECT d.IdDevise FROM Devises d
                    ORDER BY d.IdDevise
                    LIMIT 1
                )
                WHERE w.DeviseId IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "DeviseId",
                table: "WalletsVirtuelsAgents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletsVirtuelsAgents_DeviseId",
                table: "WalletsVirtuelsAgents",
                column: "DeviseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletsVirtuelsAgents_Devises_DeviseId",
                table: "WalletsVirtuelsAgents",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletsVirtuelsAgents_Devises_DeviseId",
                table: "WalletsVirtuelsAgents");

            migrationBuilder.DropIndex(
                name: "IX_WalletsVirtuelsAgents_DeviseId",
                table: "WalletsVirtuelsAgents");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "WalletsVirtuelsAgents");
        }
    }
}
