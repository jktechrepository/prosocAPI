using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddArrieresAffilieUnifie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Periodicite",
                table: "Frais",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ponctuel")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE Frais SET Periodicite = 'Ponctuel' WHERE Periodicite = '' OR Periodicite IS NULL;");

            migrationBuilder.CreateTable(
                name: "ArrieresAffilie",
                columns: table => new
                {
                    IdArrieresAffilie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    TypeObligation = table.Column<int>(type: "int", nullable: false),
                    FraisId = table.Column<int>(type: "int", nullable: true),
                    SouscriptionPrestationId = table.Column<int>(type: "int", nullable: true),
                    CotisationAffilieId = table.Column<int>(type: "int", nullable: true),
                    Mois = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Periodicite = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MontantAttendu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RestAPayer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatutPaiement = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateDernierPaiement = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrieresAffilie", x => x.IdArrieresAffilie);
                    table.ForeignKey(
                        name: "FK_ArrieresAffilie_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArrieresAffilie_CotisationsAffilie_CotisationAffilieId",
                        column: x => x.CotisationAffilieId,
                        principalTable: "CotisationsAffilie",
                        principalColumn: "IdCotisationAffilie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArrieresAffilie_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArrieresAffilie_Frais_FraisId",
                        column: x => x.FraisId,
                        principalTable: "Frais",
                        principalColumn: "IdFrais",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArrieresAffilie_SouscriptionsPrestations_SouscriptionPrestat~",
                        column: x => x.SouscriptionPrestationId,
                        principalTable: "SouscriptionsPrestations",
                        principalColumn: "IdSouscriptionPrestation",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArrieresAffilie_AffilieId_TypeObligation_Mois_Annee_FraisId_~",
                table: "ArrieresAffilie",
                columns: new[] { "AffilieId", "TypeObligation", "Mois", "Annee", "FraisId", "SouscriptionPrestationId", "CotisationAffilieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrieresAffilie_CotisationAffilieId",
                table: "ArrieresAffilie",
                column: "CotisationAffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrieresAffilie_DeviseId",
                table: "ArrieresAffilie",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrieresAffilie_FraisId",
                table: "ArrieresAffilie",
                column: "FraisId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrieresAffilie_SouscriptionPrestationId",
                table: "ArrieresAffilie",
                column: "SouscriptionPrestationId");

            migrationBuilder.Sql(@"
INSERT INTO ArrieresAffilie (
    AffilieId, TypeObligation, SouscriptionPrestationId, Mois, Annee, DateEcheance,
    Periodicite, MontantAttendu, MontantPaye, RestAPayer, DeviseId, Description,
    StatutPaiement, Statut, DateCreation, DateModification, DateDernierPaiement)
SELECT
    sa.AffilieId,
    2,
    sp.IdSouscriptionPrestation,
    CAST(SUBSTRING_INDEX(sa.Periode, '-', 1) AS UNSIGNED),
    CAST(SUBSTRING_INDEX(sa.Periode, '-', -1) AS UNSIGNED),
    STR_TO_DATE(CONCAT('01-', sa.Periode), '%d-%m-%Y'),
    'Mensuel',
    sa.MontantAttendu,
    sa.MontantPaye,
    sa.RestAPayer,
    COALESCE(p.DeviseId, 1),
    sa.Description,
    sa.StatutPaiement,
    sa.Statut,
    sa.DateCreation,
    sa.DateModification,
    sa.DateDernierPaiement
FROM SouscriptionsArrierees sa
LEFT JOIN SouscriptionsPrestations sp
    ON sp.AffilieId = sa.AffilieId AND sp.PrestationId = sa.PrestationId AND sp.Statut = 1
LEFT JOIN Prestations p ON p.IdPrestation = sa.PrestationId;
");

            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_SouscriptionsArrierees_SouscriptionsArriereesIdSou~",
                table: "Collectes");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_SouscriptionsArriereesIdSouscriptionsArrierees",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "SouscriptionsArriereesIdSouscriptionsArrierees",
                table: "Collectes");

            migrationBuilder.AddColumn<int>(
                name: "ArrieresAffilieId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_ArrieresAffilieId",
                table: "Collectes",
                column: "ArrieresAffilieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_ArrieresAffilie_ArrieresAffilieId",
                table: "Collectes",
                column: "ArrieresAffilieId",
                principalTable: "ArrieresAffilie",
                principalColumn: "IdArrieresAffilie",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropTable(
                name: "SouscriptionsArrierees");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_ArrieresAffilie_ArrieresAffilieId",
                table: "Collectes");

            migrationBuilder.DropTable(
                name: "ArrieresAffilie");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_ArrieresAffilieId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "ArrieresAffilieId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "Periodicite",
                table: "Frais");

            migrationBuilder.AddColumn<int>(
                name: "SouscriptionsArriereesIdSouscriptionsArrierees",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SouscriptionsArrierees",
                columns: table => new
                {
                    IdSouscriptionsArrierees = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    PrestationId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateDernierPaiement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MontantAttendu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Periode = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RestAPayer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatutPaiement = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SouscriptionsArrierees", x => x.IdSouscriptionsArrierees);
                    table.ForeignKey(
                        name: "FK_SouscriptionsArrierees_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SouscriptionsArrierees_Prestations_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "Prestations",
                        principalColumn: "IdPrestation",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SouscriptionsArrierees_AffilieId",
                table: "SouscriptionsArrierees",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_SouscriptionsArrierees_PrestationId",
                table: "SouscriptionsArrierees",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_SouscriptionsArriereesIdSouscriptionsArrierees",
                table: "Collectes",
                column: "SouscriptionsArriereesIdSouscriptionsArrierees");

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_SouscriptionsArrierees_SouscriptionsArriereesIdSou~",
                table: "Collectes",
                column: "SouscriptionsArriereesIdSouscriptionsArrierees",
                principalTable: "SouscriptionsArrierees",
                principalColumn: "IdSouscriptionsArrierees");
        }
    }
}
