using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AddMultideviseModule : Migration
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

            migrationBuilder.AddColumn<bool>(
                name: "EstDevisePrincipale",
                table: "Devises",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Symbole",
                table: "Devises",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DevisePrincipaleId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviseTarifId",
                table: "Collectes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantDevisePrincipale",
                table: "Collectes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantTarifAttendu",
                table: "Collectes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxVersDevisePrincipale",
                table: "Collectes",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TauxChangeDevises",
                columns: table => new
                {
                    IdTauxChangeDevise = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviseSourceId = table.Column<int>(type: "int", nullable: false),
                    DeviseCibleId = table.Column<int>(type: "int", nullable: false),
                    Taux = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    DateEffet = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TauxChangeDevises", x => x.IdTauxChangeDevise);
                    table.ForeignKey(
                        name: "FK_TauxChangeDevises_Devises_DeviseCibleId",
                        column: x => x.DeviseCibleId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TauxChangeDevises_Devises_DeviseSourceId",
                        column: x => x.DeviseSourceId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_DevisePrincipaleId",
                table: "Collectes",
                column: "DevisePrincipaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_DeviseTarifId",
                table: "Collectes",
                column: "DeviseTarifId");

            migrationBuilder.CreateIndex(
                name: "IX_TauxChangeDevises_DeviseCibleId",
                table: "TauxChangeDevises",
                column: "DeviseCibleId");

            migrationBuilder.CreateIndex(
                name: "IX_TauxChangeDevises_DeviseSourceId_DeviseCibleId_DateEffet",
                table: "TauxChangeDevises",
                columns: new[] { "DeviseSourceId", "DeviseCibleId", "DateEffet" });

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_Devises_DevisePrincipaleId",
                table: "Collectes",
                column: "DevisePrincipaleId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Collectes_Devises_DeviseTarifId",
                table: "Collectes",
                column: "DeviseTarifId",
                principalTable: "Devises",
                principalColumn: "IdDevise",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_Devises_DevisePrincipaleId",
                table: "Collectes");

            migrationBuilder.DropForeignKey(
                name: "FK_Collectes_Devises_DeviseTarifId",
                table: "Collectes");

            migrationBuilder.DropTable(
                name: "TauxChangeDevises");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_DevisePrincipaleId",
                table: "Collectes");

            migrationBuilder.DropIndex(
                name: "IX_Collectes_DeviseTarifId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "EstDevisePrincipale",
                table: "Devises");

            migrationBuilder.DropColumn(
                name: "Symbole",
                table: "Devises");

            migrationBuilder.DropColumn(
                name: "DevisePrincipaleId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "DeviseTarifId",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "MontantDevisePrincipale",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "MontantTarifAttendu",
                table: "Collectes");

            migrationBuilder.DropColumn(
                name: "TauxVersDevisePrincipale",
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
