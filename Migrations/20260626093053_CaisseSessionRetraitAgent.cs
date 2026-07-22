using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class CaisseSessionRetraitAgent : Migration
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
                name: "OperateurUtilisateurId",
                table: "JetonsRetraits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperateurPaiementUtilisateurId",
                table: "DemandesRetraitAgents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WalletMouvementId",
                table: "DemandesRetraitAgents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SessionsCaisses",
                columns: table => new
                {
                    IdSessionCaisse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    SoldeOuverture = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOuverture = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateCloture = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ObservationCloture = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoldeTheoriqueCloture = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoldeReelCloture = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StatutActif = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionsCaisses", x => x.IdSessionCaisse);
                    table.ForeignKey(
                        name: "FK_SessionsCaisses_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionsCaisses_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MouvementsCaisses",
                columns: table => new
                {
                    IdMouvementCaisse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SessionCaisseId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    TypeOperation = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    DateOperation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CollecteId = table.Column<int>(type: "int", nullable: true),
                    DemandeRetraitId = table.Column<int>(type: "int", nullable: true),
                    JetonRetraitId = table.Column<int>(type: "int", nullable: true),
                    WalletMouvementId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MouvementsCaisses", x => x.IdMouvementCaisse);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_Collectes_CollecteId",
                        column: x => x.CollecteId,
                        principalTable: "Collectes",
                        principalColumn: "IdCollecte",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_DemandesRetraitAgents_DemandeRetraitId",
                        column: x => x.DemandeRetraitId,
                        principalTable: "DemandesRetraitAgents",
                        principalColumn: "IdDemande",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_JetonsRetraits_JetonRetraitId",
                        column: x => x.JetonRetraitId,
                        principalTable: "JetonsRetraits",
                        principalColumn: "IdJeton",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_SessionsCaisses_SessionCaisseId",
                        column: x => x.SessionCaisseId,
                        principalTable: "SessionsCaisses",
                        principalColumn: "IdSessionCaisse",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MouvementsCaisses_WalletMouvements_WalletMouvementId",
                        column: x => x.WalletMouvementId,
                        principalTable: "WalletMouvements",
                        principalColumn: "IdWalletMouvement",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JetonsRetraits_OperateurUtilisateurId",
                table: "JetonsRetraits",
                column: "OperateurUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesRetraitAgents_OperateurPaiementUtilisateurId",
                table: "DemandesRetraitAgents",
                column: "OperateurPaiementUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesRetraitAgents_WalletMouvementId",
                table: "DemandesRetraitAgents",
                column: "WalletMouvementId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_CollecteId",
                table: "MouvementsCaisses",
                column: "CollecteId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_DemandeRetraitId",
                table: "MouvementsCaisses",
                column: "DemandeRetraitId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_DeviseId",
                table: "MouvementsCaisses",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_JetonRetraitId",
                table: "MouvementsCaisses",
                column: "JetonRetraitId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_SessionCaisseId",
                table: "MouvementsCaisses",
                column: "SessionCaisseId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_UtilisateurId",
                table: "MouvementsCaisses",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsCaisses_WalletMouvementId",
                table: "MouvementsCaisses",
                column: "WalletMouvementId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionsCaisses_DeviseId",
                table: "SessionsCaisses",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionsCaisses_UtilisateurId_Statut",
                table: "SessionsCaisses",
                columns: new[] { "UtilisateurId", "Statut" });

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesRetraitAgents_Utilisateurs_OperateurPaiementUtilisat~",
                table: "DemandesRetraitAgents",
                column: "OperateurPaiementUtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesRetraitAgents_WalletMouvements_WalletMouvementId",
                table: "DemandesRetraitAgents",
                column: "WalletMouvementId",
                principalTable: "WalletMouvements",
                principalColumn: "IdWalletMouvement",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JetonsRetraits_Utilisateurs_OperateurUtilisateurId",
                table: "JetonsRetraits",
                column: "OperateurUtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandesRetraitAgents_Utilisateurs_OperateurPaiementUtilisat~",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandesRetraitAgents_WalletMouvements_WalletMouvementId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropForeignKey(
                name: "FK_JetonsRetraits_Utilisateurs_OperateurUtilisateurId",
                table: "JetonsRetraits");

            migrationBuilder.DropTable(
                name: "MouvementsCaisses");

            migrationBuilder.DropTable(
                name: "SessionsCaisses");

            migrationBuilder.DropIndex(
                name: "IX_JetonsRetraits_OperateurUtilisateurId",
                table: "JetonsRetraits");

            migrationBuilder.DropIndex(
                name: "IX_DemandesRetraitAgents_OperateurPaiementUtilisateurId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropIndex(
                name: "IX_DemandesRetraitAgents_WalletMouvementId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropColumn(
                name: "OperateurUtilisateurId",
                table: "JetonsRetraits");

            migrationBuilder.DropColumn(
                name: "OperateurPaiementUtilisateurId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropColumn(
                name: "WalletMouvementId",
                table: "DemandesRetraitAgents");

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
