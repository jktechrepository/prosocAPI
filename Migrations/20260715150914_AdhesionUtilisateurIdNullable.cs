using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class AdhesionUtilisateurIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adhesions_Utilisateurs_UtilisateurId",
                table: "Adhesions");

            migrationBuilder.AlterColumn<int>(
                name: "UtilisateurId",
                table: "Adhesions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Adhesions_Utilisateurs_UtilisateurId",
                table: "Adhesions",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adhesions_Utilisateurs_UtilisateurId",
                table: "Adhesions");

            migrationBuilder.AlterColumn<int>(
                name: "UtilisateurId",
                table: "Adhesions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Adhesions_Utilisateurs_UtilisateurId",
                table: "Adhesions",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "IdUtilisateur",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
