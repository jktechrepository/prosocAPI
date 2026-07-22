using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class PerceptionVirtuelleCollecte : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePerception",
                table: "Collectes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PercepteurUtilisateurId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerceptionVirtuelleId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatutPerception",
                table: "Collectes",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerceptionsVirtuelles",
                columns: table => new
                {
                    IdPerceptionVirtuelle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    PercepteurUtilisateurId = table.Column<int>(type: "int", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    NombreCollectes = table.Column<int>(type: "int", nullable: false),
                    DatePerception = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerceptionsVirtuelles", x => x.IdPerceptionVirtuelle);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuelles_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuelles_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuelles_Utilisateurs_PercepteurUtilisateurId",
                        column: x => x.PercepteurUtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerceptionsVirtuellesLignes",
                columns: table => new
                {
                    IdLigne = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PerceptionVirtuelleId = table.Column<int>(type: "int", nullable: false),
                    CollecteId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletVirtuelMouvementId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerceptionsVirtuellesLignes", x => x.IdLigne);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuellesLignes_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuellesLignes_Collectes_CollecteId",
                        column: x => x.CollecteId,
                        principalTable: "Collectes",
                        principalColumn: "IdCollecte",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuellesLignes_PerceptionsVirtuelles_Perception~",
                        column: x => x.PerceptionVirtuelleId,
                        principalTable: "PerceptionsVirtuelles",
                        principalColumn: "IdPerceptionVirtuelle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerceptionsVirtuellesLignes_WalletVirtuelMouvements_WalletVi~",
                        column: x => x.WalletVirtuelMouvementId,
                        principalTable: "WalletVirtuelMouvements",
                        principalColumn: "IdWalletVirtuelMouvement",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_PercepteurUtilisateurId",
                table: "Collectes",
                column: "PercepteurUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_PerceptionVirtuelleId",
                table: "Collectes",
                column: "PerceptionVirtuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuelles_AgentId",
                table: "PerceptionsVirtuelles",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuelles_DeviseId",
                table: "PerceptionsVirtuelles",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuelles_PercepteurUtilisateurId",
                table: "PerceptionsVirtuelles",
                column: "PercepteurUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuellesLignes_AgentId",
                table: "PerceptionsVirtuellesLignes",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuellesLignes_CollecteId",
                table: "PerceptionsVirtuellesLignes",
                column: "CollecteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuellesLignes_PerceptionVirtuelleId",
                table: "PerceptionsVirtuellesLignes",
                column: "PerceptionVirtuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_PerceptionsVirtuellesLignes_WalletVirtuelMouvementId",
                table: "PerceptionsVirtuellesLignes",
                column: "WalletVirtuelMouvementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_PerceptionsVirtuelles_PerceptionVirtuelleId",
                table: "Collectes",
                column: "PerceptionVirtuelleId",
                principalTable: "PerceptionsVirtuelles",
                principalColumn: "IdPerceptionVirtuelle",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_Utilisateurs_PercepteurUtilisateurId",
                table: "Collectes",
                column: "PercepteurUtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_PerceptionsVirtuelles_PerceptionVirtuelleId",
                table: "Collectes");

            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_Utilisateurs_PercepteurUtilisateurId",
                table: "Collectes");

            migrationBuilder.DropTable(
                name: "PerceptionsVirtuellesLignes");

            migrationBuilder.DropTable(
                name: "PerceptionsVirtuelles");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_PercepteurUtilisateurId",
                table: "Collectes");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_PerceptionVirtuelleId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "DatePerception",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "PercepteurUtilisateurId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "PerceptionVirtuelleId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "StatutPerception",
                table: "Collectes");

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
