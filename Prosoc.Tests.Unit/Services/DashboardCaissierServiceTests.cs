using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardCaissierServiceTests
{
    private static async Task<ProsocDbContext> CreateDbContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static DashboardCaissierService CreateService(ProsocDbContext db) =>
        new(db, new Mock<ILogger<DashboardCaissierService>>().Object);

    private static async Task RunAsync(Func<DashboardCaissierService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await test(CreateService(db), db);
    }

    [Fact]
    public async Task GetKpisAsync_CompteUniquementLesCollectesDeLOperateur()
    {
        await RunAsync(async (service, db) =>
        {
            var caissier1 = new Utilisateur
            {
                NomUtilisateur = "caissier1",
                MotDePasseHash = "hash",
                Statut = true
            };
            var caissier2 = new Utilisateur
            {
                NomUtilisateur = "caissier2",
                MotDePasseHash = "hash",
                Statut = true
            };
            db.Utilisateurs.AddRange(caissier1, caissier2);

            var agent = new Agent
            {
                NomComplet = "Agent",
                Matricule = "AG000000001",
                Phone = "0990000001",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF001",
                Nom = "Jean",
                Prenom = "Dupont",
                NomComplet = "Jean Dupont",
                DateNaissance = new DateTime(1980, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);

            var devise = new Devise { Code = "USD", Nom = "Dollar", Statut = true };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = caissier1.IdUtilisateur,
                    Montant = 100m,
                    DeviseId = devise.IdDevise,
                    DateCollecte = DateTime.Today.AddHours(10),
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true
                },
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Frais,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = caissier2.IdUtilisateur,
                    Montant = 500m,
                    DeviseId = devise.IdDevise,
                    DateCollecte = DateTime.Today.AddHours(11),
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync(caissier1.IdUtilisateur);

            Assert.Equal(100m, kpis.MontantDuJour);
            Assert.Equal(1, kpis.NombreCollectesDuJour);
        });
    }

    [Fact]
    public async Task GetKpisAsync_MontantsConsolidesEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            var caissier = new Utilisateur
            {
                NomUtilisateur = "caissier-mix",
                MotDePasseHash = "hash",
                Statut = true
            };
            db.Utilisateurs.Add(caissier);

            var agent = new Agent
            {
                NomComplet = "Agent",
                Matricule = "AG000000003",
                Phone = "0990000003",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF003",
                Nom = "Paul",
                Prenom = "Test",
                NomComplet = "Paul Test",
                DateNaissance = new DateTime(1980, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);

            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
            db.Devises.AddRange(usd, cdf);
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = caissier.IdUtilisateur,
                    DeviseId = cdf.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    DateCollecte = DateTime.Today.AddHours(9),
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true
                },
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = caissier.IdUtilisateur,
                    DeviseId = usd.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    DateCollecte = DateTime.Today.AddHours(10),
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync(caissier.IdUtilisateur);

            Assert.Equal(11m, kpis.MontantDuJour);
            Assert.Equal(11m, kpis.MontantSemaine);
            Assert.Equal(11m, kpis.MontantMois);
            Assert.Equal(5.5m, kpis.MontantMoyen);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        });
    }

    [Fact]
    public async Task GetCollectesRecentesAsync_FiltreParOperateurUtilisateurId()
    {
        await RunAsync(async (service, db) =>
        {
            var caissier = new Utilisateur
            {
                NomUtilisateur = "caissier",
                MotDePasseHash = "hash",
                Statut = true
            };
            db.Utilisateurs.Add(caissier);

            var agent = new Agent
            {
                NomComplet = "Agent",
                Matricule = "AG000000002",
                Phone = "0990000002",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF002",
                Nom = "Marie",
                Prenom = "Martin",
                NomComplet = "Marie Martin",
                DateNaissance = new DateTime(1985, 5, 5),
                Statut = true
            };
            db.Affilies.Add(affilie);

            var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            db.Collectes.Add(new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                OperateurUtilisateurId = caissier.IdUtilisateur,
                Montant = 50m,
                DeviseId = devise.IdDevise,
                DateCollecte = DateTime.Now,
                Statut = true
            });
            db.Collectes.Add(new Collecte
            {
                TypeCollecte = TypeCollecte.Frais,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                OperateurUtilisateurId = null,
                Montant = 999m,
                DeviseId = devise.IdDevise,
                DateCollecte = DateTime.Now,
                Statut = true
            });
            await db.SaveChangesAsync();

            var collectes = await service.GetCollectesRecentesAsync(caissier.IdUtilisateur, 10);

            Assert.Single(collectes);
            Assert.Equal(50m, collectes[0].Montant);
        });
    }

    [Fact]
    public async Task GetCollectesHistoriqueAsync_FiltreOperateurDatesEtModePaiement()
    {
        await RunAsync(async (service, db) =>
        {
            var caissier = new Utilisateur
            {
                NomUtilisateur = "caissier-hist",
                MotDePasseHash = "hash",
                Statut = true
            };
            db.Utilisateurs.Add(caissier);

            var agent = new Agent
            {
                NomComplet = "Agent Hist",
                Matricule = "AG000000099",
                Phone = "0990000099",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF099",
                Nom = "Paul",
                Prenom = "Test",
                NomComplet = "Paul Test",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);

            var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            var baseDate = new DateTime(2026, 7, 10);
            db.Collectes.AddRange(
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Frais,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = caissier.IdUtilisateur,
                    Montant = 100m,
                    ModePaiement = "ESPECE",
                    DeviseId = devise.IdDevise,
                    DateCollecte = baseDate,
                    Statut = true
                },
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Frais,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = caissier.IdUtilisateur,
                    Montant = 200m,
                    ModePaiement = "MOBILE_MONEY",
                    DeviseId = devise.IdDevise,
                    DateCollecte = baseDate.AddDays(5),
                    Statut = true
                },
                new Collecte
                {
                    TypeCollecte = TypeCollecte.Frais,
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    OperateurUtilisateurId = null,
                    Montant = 999m,
                    ModePaiement = "ESPECE",
                    DeviseId = devise.IdDevise,
                    DateCollecte = baseDate,
                    Statut = true
                });
            await db.SaveChangesAsync();

            var result = await service.GetCollectesHistoriqueAsync(
                caissier.IdUtilisateur,
                new GuichetCollecteHistoriqueFiltreDto
                {
                    DateDebut = baseDate,
                    DateFin = baseDate.AddDays(1),
                    ModePaiement = "ESPECE"
                },
                new PaginationRequest { Page = 1, PageSize = 10 });

            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Data);
            Assert.Equal(100m, result.Data[0].Montant);
            Assert.Equal("ESPECE", result.Data[0].ModePaiement);
        });
    }
}
