using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddPenaliteAffilie : Migration
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
                name: "PenaliteAffilieId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PenalitesAffilie",
                columns: table => new
                {
                    IdPenaliteAffilie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    ArrieresAffilieId = table.Column<int>(type: "int", nullable: false),
                    FraisId = table.Column<int>(type: "int", nullable: false),
                    TypePenalite = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    JoursRetard = table.Column<int>(type: "int", nullable: false),
                    Motif = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotifAnnulation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateApplication = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateAnnulation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StatutActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenalitesAffilie", x => x.IdPenaliteAffilie);
                    table.ForeignKey(
                        name: "FK_PenalitesAffilie_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenalitesAffilie_ArrieresAffilie_ArrieresAffilieId",
                        column: x => x.ArrieresAffilieId,
                        principalTable: "ArrieresAffilie",
                        principalColumn: "IdArrieresAffilie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenalitesAffilie_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenalitesAffilie_Frais_FraisId",
                        column: x => x.FraisId,
                        principalTable: "Frais",
                        principalColumn: "IdFrais",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_PenaliteAffilieId",
                table: "Collectes",
                column: "PenaliteAffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_PenalitesAffilie_AffilieId",
                table: "PenalitesAffilie",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_PenalitesAffilie_ArrieresAffilieId_TypePenalite",
                table: "PenalitesAffilie",
                columns: new[] { "ArrieresAffilieId", "TypePenalite" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PenalitesAffilie_DeviseId",
                table: "PenalitesAffilie",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_PenalitesAffilie_FraisId",
                table: "PenalitesAffilie",
                column: "FraisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_PenalitesAffilie_PenaliteAffilieId",
                table: "Collectes",
                column: "PenaliteAffilieId",
                principalTable: "PenalitesAffilie",
                principalColumn: "IdPenaliteAffilie",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_PenalitesAffilie_PenaliteAffilieId",
                table: "Collectes");

            migrationBuilder.DropTable(
                name: "PenalitesAffilie");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_PenaliteAffilieId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "PenaliteAffilieId",
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
