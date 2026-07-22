using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prosoc.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Affilies",
                columns: table => new
                {
                    IdAffilie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodeAdhesion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prenom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomComplet = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNaissance = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAffilie = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Postnom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProvinceResidence = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommuneResidence = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuartierResidence = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvenueResidence = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroResidence = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommuneActivite = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuartierActivite = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvenueActivite = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroActivite = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affilies", x => x.IdAffilie);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Assureurs",
                columns: table => new
                {
                    IdAssureur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assureurs", x => x.IdAssureur);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoriesAdhesions",
                columns: table => new
                {
                    IdCategorieAdhesion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Libelle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesAdhesions", x => x.IdCategorieAdhesion);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoriesAgents",
                columns: table => new
                {
                    IdCategorieAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LibelleCategorie = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesAgents", x => x.IdCategorieAgent);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CodesAdhesionSequences",
                columns: table => new
                {
                    Prefix = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NextValue = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodesAdhesionSequences", x => x.Prefix);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Devises",
                columns: table => new
                {
                    IdDevise = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TauxChange = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devises", x => x.IdDevise);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HopitalPartenaires",
                columns: table => new
                {
                    IdHopital = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adresse = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telephone = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactPersonne = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodeAcces = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Niveau = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ServicesOfferts = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlafondJournalier = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopitalPartenaires", x => x.IdHopital);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MobileAppConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AppName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppStoreUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayStoreUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdateMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsForceUpdateRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsMaintenanceMode = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaintenanceStart = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MaintenanceEnd = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MaintenanceMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinSupportedVersion = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileAppConfigs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    IdNotificationType = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categorie = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Couleur = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icône = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstActif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priorite = table.Column<int>(type: "int", nullable: false),
                    EmailParDefaut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SmsParDefaut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PushParDefaut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InAppParDefaut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TemplateMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypes", x => x.IdNotificationType);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    IdPermission = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categorie = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.IdPermission);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    IdProvince = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.IdProvince);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Niveau = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRole);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    IdUserNotificationPreference = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmailNotification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SmsNotification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PushNotification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InAppNotification = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommissionEmail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommissionSms = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommissionPush = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommissionInApp = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MinCommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionCurrency = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommissionMessageTemplate = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timezone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuietHoursEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuietHoursStart = table.Column<int>(type: "int", nullable: false),
                    QuietHoursEnd = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.IdUserNotificationPreference);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Antecedants",
                columns: table => new
                {
                    IdAntecedant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antecedants", x => x.IdAntecedant);
                    table.ForeignKey(
                        name: "FK_Antecedants_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Dependants",
                columns: table => new
                {
                    IdDependant = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LienParente = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNaissance = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependants", x => x.IdDependant);
                    table.ForeignKey(
                        name: "FK_Dependants_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TypeAdhesions",
                columns: table => new
                {
                    IdTypeAdhesion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Libelle = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxDependants = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CategorieAdhesionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeAdhesions", x => x.IdTypeAdhesion);
                    table.ForeignKey(
                        name: "FK_TypeAdhesions_CategoriesAdhesions_CategorieAdhesionId",
                        column: x => x.CategorieAdhesionId,
                        principalTable: "CategoriesAdhesions",
                        principalColumn: "IdCategorieAdhesion",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProduitsAssureurs",
                columns: table => new
                {
                    IdProduit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AssureurId = table.Column<int>(type: "int", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    CommissionMutuelle = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrixMensuel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduitsAssureurs", x => x.IdProduit);
                    table.ForeignKey(
                        name: "FK_ProduitsAssureurs_Assureurs_AssureurId",
                        column: x => x.AssureurId,
                        principalTable: "Assureurs",
                        principalColumn: "IdAssureur",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProduitsAssureurs_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProduitsMutuels",
                columns: table => new
                {
                    IdProduit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrixMensuel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduitsMutuels", x => x.IdProduit);
                    table.ForeignKey(
                        name: "FK_ProduitsMutuels_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JetonsMedicaux",
                columns: table => new
                {
                    IdJeton = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    CodeJeton = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEmission = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstValide = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EstUtilise = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HopitalPartenaireId = table.Column<int>(type: "int", nullable: true),
                    Observation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JetonsMedicaux", x => x.IdJeton);
                    table.ForeignKey(
                        name: "FK_JetonsMedicaux_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JetonsMedicaux_HopitalPartenaires_HopitalPartenaireId",
                        column: x => x.HopitalPartenaireId,
                        principalTable: "HopitalPartenaires",
                        principalColumn: "IdHopital");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    IdNotification = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeNotificationId = table.Column<int>(type: "int", nullable: true),
                    EnvoyeurId = table.Column<int>(type: "int", nullable: true),
                    RecepteurId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateLecture = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstLu = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priorite = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categorie = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Couleur = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icône = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Métadonnées = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateEnvoiEmail = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateEnvoiSms = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateEnvoiPush = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EmailEnvoyé = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SmsEnvoyé = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PushEnvoyé = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.IdNotification);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTypes_TypeNotificationId",
                        column: x => x.TypeNotificationId,
                        principalTable: "NotificationTypes",
                        principalColumn: "IdNotificationType");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Communes",
                columns: table => new
                {
                    IdCommune = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communes", x => x.IdCommune);
                    table.ForeignKey(
                        name: "FK_Communes_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "IdProvince",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    IdRolePermission = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IdUtilisateurAttribution = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.IdRolePermission);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "IdPermission",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Prestations",
                columns: table => new
                {
                    IdPrestation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomPrestation = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProduitMutuelId = table.Column<int>(type: "int", nullable: true),
                    ProduitAssureurId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HopitalPartenaireIdHopital = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestations", x => x.IdPrestation);
                    table.ForeignKey(
                        name: "FK_Prestations_HopitalPartenaires_HopitalPartenaireIdHopital",
                        column: x => x.HopitalPartenaireIdHopital,
                        principalTable: "HopitalPartenaires",
                        principalColumn: "IdHopital");
                    table.ForeignKey(
                        name: "FK_Prestations_ProduitsAssureurs_ProduitAssureurId",
                        column: x => x.ProduitAssureurId,
                        principalTable: "ProduitsAssureurs",
                        principalColumn: "IdProduit");
                    table.ForeignKey(
                        name: "FK_Prestations_ProduitsMutuels_ProduitMutuelId",
                        column: x => x.ProduitMutuelId,
                        principalTable: "ProduitsMutuels",
                        principalColumn: "IdProduit");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ZonesSociales",
                columns: table => new
                {
                    IdZoneSociale = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommuneId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonesSociales", x => x.IdZoneSociale);
                    table.ForeignKey(
                        name: "FK_ZonesSociales_Communes_CommuneId",
                        column: x => x.CommuneId,
                        principalTable: "Communes",
                        principalColumn: "IdCommune",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BonsEnvoi",
                columns: table => new
                {
                    IdBonEnvoi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroBon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    PrestationId = table.Column<int>(type: "int", nullable: false),
                    DateEmission = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstUtilise = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsEnvoi", x => x.IdBonEnvoi);
                    table.ForeignKey(
                        name: "FK_BonsEnvoi_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonsEnvoi_Prestations_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "Prestations",
                        principalColumn: "IdPrestation",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SouscriptionsArrierees",
                columns: table => new
                {
                    IdSouscriptionsArrierees = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    PrestationId = table.Column<int>(type: "int", nullable: false),
                    MontantAttendu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RestAPayer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Periode = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateDernierPaiement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "SouscriptionsPrestations",
                columns: table => new
                {
                    IdSouscriptionPrestation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    PrestationId = table.Column<int>(type: "int", nullable: false),
                    DateSouscription = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SouscriptionsPrestations", x => x.IdSouscriptionPrestation);
                    table.ForeignKey(
                        name: "FK_SouscriptionsPrestations_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SouscriptionsPrestations_Prestations_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "Prestations",
                        principalColumn: "IdPrestation",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    IdAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomComplet = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Matricule = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAgent = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fonction = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleAgent = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CategorieAgentId = table.Column<int>(type: "int", nullable: true),
                    ZoneSocialeId = table.Column<int>(type: "int", nullable: true),
                    SuperviseurId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.IdAgent);
                    table.ForeignKey(
                        name: "FK_Agents_Agents_SuperviseurId",
                        column: x => x.SuperviseurId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_Agents_CategoriesAgents_CategorieAgentId",
                        column: x => x.CategorieAgentId,
                        principalTable: "CategoriesAgents",
                        principalColumn: "IdCategorieAgent");
                    table.ForeignKey(
                        name: "FK_Agents_ZonesSociales_ZoneSocialeId",
                        column: x => x.ZoneSocialeId,
                        principalTable: "ZonesSociales",
                        principalColumn: "IdZoneSociale",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DemandesBonEnvoi",
                columns: table => new
                {
                    IdDemande = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    PrestationId = table.Column<int>(type: "int", nullable: false),
                    TypeDemande = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotifDemande = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    ObservationAgent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDemande = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StatutDemande = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BonEnvoiId = table.Column<int>(type: "int", nullable: true),
                    JetonMedicalId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesBonEnvoi", x => x.IdDemande);
                    table.ForeignKey(
                        name: "FK_DemandesBonEnvoi_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandesBonEnvoi_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandesBonEnvoi_BonsEnvoi_BonEnvoiId",
                        column: x => x.BonEnvoiId,
                        principalTable: "BonsEnvoi",
                        principalColumn: "IdBonEnvoi");
                    table.ForeignKey(
                        name: "FK_DemandesBonEnvoi_JetonsMedicaux_JetonMedicalId",
                        column: x => x.JetonMedicalId,
                        principalTable: "JetonsMedicaux",
                        principalColumn: "IdJeton");
                    table.ForeignKey(
                        name: "FK_DemandesBonEnvoi_Prestations_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "Prestations",
                        principalColumn: "IdPrestation",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RetraitsAgents",
                columns: table => new
                {
                    IdRetraitAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodeRetraitPin = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDemande = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstValide = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeviseIdDevise = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetraitsAgents", x => x.IdRetraitAgent);
                    table.ForeignKey(
                        name: "FK_RetraitsAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RetraitsAgents_Devises_DeviseIdDevise",
                        column: x => x.DeviseIdDevise,
                        principalTable: "Devises",
                        principalColumn: "IdDevise");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TargetsAgents",
                columns: table => new
                {
                    IdTargetAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    LibelleTarget = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MontantTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetsAgents", x => x.IdTargetAgent);
                    table.ForeignKey(
                        name: "FK_TargetsAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IdUtilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReferenceUtilisateur = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NomUtilisateur = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailUtilisateur = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneUtilisateur = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotDePasseHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultUsername = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoitChangerMotDePasse = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    AgentId = table.Column<int>(type: "int", nullable: true),
                    AffilieId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsConnecte = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IdUtilisateur);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie");
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "IdRole");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WalletsAgents",
                columns: table => new
                {
                    IdWalletAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    SoldeCourant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoldeDisponible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletsAgents", x => x.IdWalletAgent);
                    table.ForeignKey(
                        name: "FK_WalletsAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WalletsVirtuelsAgents",
                columns: table => new
                {
                    IdWalletVirtuelAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    SoldeVirtuel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletsVirtuelsAgents", x => x.IdWalletVirtuelAgent);
                    table.ForeignKey(
                        name: "FK_WalletsVirtuelsAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Adhesions",
                columns: table => new
                {
                    IdAdhesion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StatutDossier = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    TypeAdhesionId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AffilieIdAffilie = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adhesions", x => x.IdAdhesion);
                    table.ForeignKey(
                        name: "FK_Adhesions_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adhesions_Affilies_AffilieIdAffilie",
                        column: x => x.AffilieIdAffilie,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie");
                    table.ForeignKey(
                        name: "FK_Adhesions_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adhesions_TypeAdhesions_TypeAdhesionId",
                        column: x => x.TypeAdhesionId,
                        principalTable: "TypeAdhesions",
                        principalColumn: "IdTypeAdhesion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adhesions_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Frais",
                columns: table => new
                {
                    IdFrais = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Libelle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreeParId = table.Column<int>(type: "int", nullable: true),
                    ModifieParId = table.Column<int>(type: "int", nullable: true),
                    DateSuppression = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstSupprime = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frais", x => x.IdFrais);
                    table.ForeignKey(
                        name: "FK_Frais_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Frais_Utilisateurs_CreeParId",
                        column: x => x.CreeParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                    table.ForeignKey(
                        name: "FK_Frais_Utilisateurs_ModifieParId",
                        column: x => x.ModifieParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MobileSyncData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Data = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SyncStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateSynchronisation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateDerniereTentative = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NombreTentatives = table.Column<int>(type: "int", nullable: false),
                    ErreurMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstSynchronise = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileSyncData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileSyncData_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MobileUserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    SessionToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppVersion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OsVersion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateDerniereActivite = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EstBiometricAuth = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NombreRequetes = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDerniereSynchronisation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstModeHorsLigne = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileUserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileUserSessions_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    IdPasswordResetToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.IdPasswordResetToken);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    IdRefreshToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateRevocation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeviceInfo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.IdRefreshToken);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    IdUserDevice = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    FcmToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceModel = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OsVersion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultDevice = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateEnregistrement = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateDerniereUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.IdUserDevice);
                    table.ForeignKey(
                        name: "FK_UserDevices_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    IdUserPermission = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsGranted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Commentaire = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttribueParIdUtilisateur = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.IdUserPermission);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "IdPermission",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    IdUserRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IdUtilisateurAttribution = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.IdUserRole);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "IdRole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "IdUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Collectes",
                columns: table => new
                {
                    IdCollecte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TypeCollecte = table.Column<int>(type: "int", nullable: false),
                    FraisId = table.Column<int>(type: "int", nullable: true),
                    AffilieId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mois = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    ReferencePaiement = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModePaiement = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Operateur = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatutPaiement = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SouscriptionPrestationId = table.Column<int>(type: "int", nullable: true),
                    MontantRecu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontantAttendu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeviseId = table.Column<int>(type: "int", nullable: false),
                    DateCollecte = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PrestationIdPrestation = table.Column<int>(type: "int", nullable: true),
                    SouscriptionPrestationIdSouscriptionPrestation = table.Column<int>(type: "int", nullable: true),
                    SouscriptionsArriereesIdSouscriptionsArrierees = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collectes", x => x.IdCollecte);
                    table.ForeignKey(
                        name: "FK_Collectes_Affilies_AffilieId",
                        column: x => x.AffilieId,
                        principalTable: "Affilies",
                        principalColumn: "IdAffilie",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Collectes_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Collectes_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "IdDevise",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Collectes_Frais_FraisId",
                        column: x => x.FraisId,
                        principalTable: "Frais",
                        principalColumn: "IdFrais",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collectes_Prestations_PrestationIdPrestation",
                        column: x => x.PrestationIdPrestation,
                        principalTable: "Prestations",
                        principalColumn: "IdPrestation");
                    table.ForeignKey(
                        name: "FK_Collectes_SouscriptionsArrierees_SouscriptionsArriereesIdSou~",
                        column: x => x.SouscriptionsArriereesIdSouscriptionsArrierees,
                        principalTable: "SouscriptionsArrierees",
                        principalColumn: "IdSouscriptionsArrierees");
                    table.ForeignKey(
                        name: "FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationId",
                        column: x => x.SouscriptionPrestationId,
                        principalTable: "SouscriptionsPrestations",
                        principalColumn: "IdSouscriptionPrestation",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Collectes_SouscriptionsPrestations_SouscriptionPrestationIdS~",
                        column: x => x.SouscriptionPrestationIdSouscriptionPrestation,
                        principalTable: "SouscriptionsPrestations",
                        principalColumn: "IdSouscriptionPrestation");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WalletMouvements",
                columns: table => new
                {
                    IdWalletMouvement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TypeOperation = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOperation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CollecteIdCollecte = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletMouvements", x => x.IdWalletMouvement);
                    table.ForeignKey(
                        name: "FK_WalletMouvements_Collectes_CollecteIdCollecte",
                        column: x => x.CollecteIdCollecte,
                        principalTable: "Collectes",
                        principalColumn: "IdCollecte");
                    table.ForeignKey(
                        name: "FK_WalletMouvements_WalletsAgents_WalletId",
                        column: x => x.WalletId,
                        principalTable: "WalletsAgents",
                        principalColumn: "IdWalletAgent",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DemandesRetraitAgents",
                columns: table => new
                {
                    IdDemande = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    MontantDemande = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TypeRetrait = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatutDemande = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotifRetrait = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotifRejet = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateDemande = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateTraitement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AgentValidationId = table.Column<int>(type: "int", nullable: true),
                    JetonRetraitId = table.Column<int>(type: "int", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesRetraitAgents", x => x.IdDemande);
                    table.ForeignKey(
                        name: "FK_DemandesRetraitAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandesRetraitAgents_Agents_AgentValidationId",
                        column: x => x.AgentValidationId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JetonsRetraits",
                columns: table => new
                {
                    IdJeton = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    DemandeRetraitId = table.Column<int>(type: "int", nullable: false),
                    CodeJeton = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MontantRetrait = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateEmission = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateUtilisation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateExpiration = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstValide = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EstUtilise = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ObservationUtilisation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateCreation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Statut = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JetonsRetraits", x => x.IdJeton);
                    table.ForeignKey(
                        name: "FK_JetonsRetraits_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JetonsRetraits_DemandesRetraitAgents_DemandeRetraitId",
                        column: x => x.DemandeRetraitId,
                        principalTable: "DemandesRetraitAgents",
                        principalColumn: "IdDemande",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Adhesions_AffilieId",
                table: "Adhesions",
                column: "AffilieId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adhesions_AffilieIdAffilie",
                table: "Adhesions",
                column: "AffilieIdAffilie");

            migrationBuilder.CreateIndex(
                name: "IX_Adhesions_AgentId",
                table: "Adhesions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Adhesions_TypeAdhesionId",
                table: "Adhesions",
                column: "TypeAdhesionId");

            migrationBuilder.CreateIndex(
                name: "IX_Adhesions_UtilisateurId",
                table: "Adhesions",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CategorieAgentId",
                table: "Agents",
                column: "CategorieAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Matricule",
                table: "Agents",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_SuperviseurId",
                table: "Agents",
                column: "SuperviseurId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ZoneSocialeId",
                table: "Agents",
                column: "ZoneSocialeId");

            migrationBuilder.CreateIndex(
                name: "IX_Antecedants_AffilieId",
                table: "Antecedants",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsEnvoi_AffilieId",
                table: "BonsEnvoi",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsEnvoi_PrestationId",
                table: "BonsEnvoi",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_AffilieId",
                table: "Collectes",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_AgentId",
                table: "Collectes",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_DeviseId",
                table: "Collectes",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_FraisId",
                table: "Collectes",
                column: "FraisId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_PrestationIdPrestation",
                table: "Collectes",
                column: "PrestationIdPrestation");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_ReferencePaiement",
                table: "Collectes",
                column: "ReferencePaiement",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_SouscriptionPrestationId",
                table: "Collectes",
                column: "SouscriptionPrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_SouscriptionPrestationIdSouscriptionPrestation",
                table: "Collectes",
                column: "SouscriptionPrestationIdSouscriptionPrestation");

            migrationBuilder.CreateIndex(
                name: "IX_Collectes_SouscriptionsArriereesIdSouscriptionsArrierees",
                table: "Collectes",
                column: "SouscriptionsArriereesIdSouscriptionsArrierees");

            migrationBuilder.CreateIndex(
                name: "IX_Communes_ProvinceId",
                table: "Communes",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesBonEnvoi_AffilieId",
                table: "DemandesBonEnvoi",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesBonEnvoi_AgentId",
                table: "DemandesBonEnvoi",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesBonEnvoi_BonEnvoiId",
                table: "DemandesBonEnvoi",
                column: "BonEnvoiId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesBonEnvoi_JetonMedicalId",
                table: "DemandesBonEnvoi",
                column: "JetonMedicalId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesBonEnvoi_PrestationId",
                table: "DemandesBonEnvoi",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesRetraitAgents_AgentId",
                table: "DemandesRetraitAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesRetraitAgents_AgentValidationId",
                table: "DemandesRetraitAgents",
                column: "AgentValidationId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesRetraitAgents_JetonRetraitId",
                table: "DemandesRetraitAgents",
                column: "JetonRetraitId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependants_AffilieId",
                table: "Dependants",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_CreeParId",
                table: "Frais",
                column: "CreeParId");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_DeviseId",
                table: "Frais",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_ModifieParId",
                table: "Frais",
                column: "ModifieParId");

            migrationBuilder.CreateIndex(
                name: "IX_JetonsMedicaux_AffilieId",
                table: "JetonsMedicaux",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_JetonsMedicaux_HopitalPartenaireId",
                table: "JetonsMedicaux",
                column: "HopitalPartenaireId");

            migrationBuilder.CreateIndex(
                name: "IX_JetonsRetraits_AgentId",
                table: "JetonsRetraits",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_JetonsRetraits_DemandeRetraitId",
                table: "JetonsRetraits",
                column: "DemandeRetraitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileSyncData_UtilisateurId",
                table: "MobileSyncData",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileUserSessions_UtilisateurId",
                table: "MobileUserSessions",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TypeNotificationId",
                table: "Notifications",
                column: "TypeNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UtilisateurId",
                table: "PasswordResetTokens",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestations_HopitalPartenaireIdHopital",
                table: "Prestations",
                column: "HopitalPartenaireIdHopital");

            migrationBuilder.CreateIndex(
                name: "IX_Prestations_ProduitAssureurId",
                table: "Prestations",
                column: "ProduitAssureurId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestations_ProduitMutuelId",
                table: "Prestations",
                column: "ProduitMutuelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduitsAssureurs_AssureurId",
                table: "ProduitsAssureurs",
                column: "AssureurId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduitsAssureurs_DeviseId",
                table: "ProduitsAssureurs",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProduitsMutuels_DeviseId",
                table: "ProduitsMutuels",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UtilisateurId",
                table: "RefreshTokens",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_RetraitsAgents_AgentId",
                table: "RetraitsAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_RetraitsAgents_DeviseIdDevise",
                table: "RetraitsAgents",
                column: "DeviseIdDevise");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SouscriptionsArrierees_AffilieId",
                table: "SouscriptionsArrierees",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_SouscriptionsArrierees_PrestationId",
                table: "SouscriptionsArrierees",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_SouscriptionsPrestations_AffilieId",
                table: "SouscriptionsPrestations",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_SouscriptionsPrestations_PrestationId",
                table: "SouscriptionsPrestations",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetsAgents_AgentId",
                table: "TargetsAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeAdhesions_CategorieAdhesionId",
                table: "TypeAdhesions",
                column: "CategorieAdhesionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UtilisateurId",
                table: "UserDevices",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UtilisateurId",
                table: "UserPermissions",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UtilisateurId",
                table: "UserRoles",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_AffilieId",
                table: "Utilisateurs",
                column: "AffilieId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_AgentId",
                table: "Utilisateurs",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_EmailUtilisateur",
                table: "Utilisateurs",
                column: "EmailUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_PhoneUtilisateur",
                table: "Utilisateurs",
                column: "PhoneUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_RoleId",
                table: "Utilisateurs",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletMouvements_CollecteIdCollecte",
                table: "WalletMouvements",
                column: "CollecteIdCollecte");

            migrationBuilder.CreateIndex(
                name: "IX_WalletMouvements_WalletId",
                table: "WalletMouvements",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletsAgents_AgentId",
                table: "WalletsAgents",
                column: "AgentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletsVirtuelsAgents_AgentId",
                table: "WalletsVirtuelsAgents",
                column: "AgentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZonesSociales_CommuneId",
                table: "ZonesSociales",
                column: "CommuneId");

            migrationBuilder.AddForeignKey(
                name: "FK_DemandesRetraitAgents_JetonsRetraits_JetonRetraitId",
                table: "DemandesRetraitAgents",
                column: "JetonRetraitId",
                principalTable: "JetonsRetraits",
                principalColumn: "IdJeton");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandesRetraitAgents_Agents_AgentId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandesRetraitAgents_Agents_AgentValidationId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropForeignKey(
                name: "FK_JetonsRetraits_Agents_AgentId",
                table: "JetonsRetraits");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandesRetraitAgents_JetonsRetraits_JetonRetraitId",
                table: "DemandesRetraitAgents");

            migrationBuilder.DropTable(
                name: "Adhesions");

            migrationBuilder.DropTable(
                name: "Antecedants");

            migrationBuilder.DropTable(
                name: "CodesAdhesionSequences");

            migrationBuilder.DropTable(
                name: "DemandesBonEnvoi");

            migrationBuilder.DropTable(
                name: "Dependants");

            migrationBuilder.DropTable(
                name: "MobileAppConfigs");

            migrationBuilder.DropTable(
                name: "MobileSyncData");

            migrationBuilder.DropTable(
                name: "MobileUserSessions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RetraitsAgents");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "TargetsAgents");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "WalletMouvements");

            migrationBuilder.DropTable(
                name: "WalletsVirtuelsAgents");

            migrationBuilder.DropTable(
                name: "TypeAdhesions");

            migrationBuilder.DropTable(
                name: "BonsEnvoi");

            migrationBuilder.DropTable(
                name: "JetonsMedicaux");

            migrationBuilder.DropTable(
                name: "NotificationTypes");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Collectes");

            migrationBuilder.DropTable(
                name: "WalletsAgents");

            migrationBuilder.DropTable(
                name: "CategoriesAdhesions");

            migrationBuilder.DropTable(
                name: "Frais");

            migrationBuilder.DropTable(
                name: "SouscriptionsArrierees");

            migrationBuilder.DropTable(
                name: "SouscriptionsPrestations");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Prestations");

            migrationBuilder.DropTable(
                name: "Affilies");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "HopitalPartenaires");

            migrationBuilder.DropTable(
                name: "ProduitsAssureurs");

            migrationBuilder.DropTable(
                name: "ProduitsMutuels");

            migrationBuilder.DropTable(
                name: "Assureurs");

            migrationBuilder.DropTable(
                name: "Devises");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "CategoriesAgents");

            migrationBuilder.DropTable(
                name: "ZonesSociales");

            migrationBuilder.DropTable(
                name: "Communes");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropTable(
                name: "JetonsRetraits");

            migrationBuilder.DropTable(
                name: "DemandesRetraitAgents");
        }
    }
}
