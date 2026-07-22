using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAgentServiceTests
{
    private static async Task<(ProsocDbContext Db, DashboardAgentService Service)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var conformiteMock = new Mock<IAffilieConformiteService>();
        conformiteMock
            .Setup(s => s.GetConformiteParAffiliesAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, AffilieConformiteDto>());

        var service = new DashboardAgentService(
            db,
            new DeviseConversionService(db),
            conformiteMock.Object,
            new Mock<ILogger<DashboardAgentService>>().Object);

        return (db, service);
    }

    private static async Task<(int AgentId, int AffilieId)> SeedAgentDataAsync(ProsocDbContext db)
    {
        var agent = new Agent
        {
            NomComplet = "Agent AT",
            Matricule = "AT000000001",
            Phone = "0990000001",
            Statut = true
        };
        db.Agents.Add(agent);

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-AT-1",
            Nom = "Paul",
            Prenom = "Test",
            NomComplet = "Paul Test",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var categorie = new CategorieAdhesion { Libelle = "Std", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var type = new TypeAdhesion
        {
            Libelle = "Ind",
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            Montant = 10m,
            Statut = true
        };
        db.TypeAdhesions.Add(type);
        await db.SaveChangesAsync();

        var user = new ProsocAPI.Models.Authentication.Utilisateur
        {
            NomUtilisateur = "at-user",
            MotDePasseHash = "hash",
            Statut = true
        };
        db.Utilisateurs.Add(user);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = type.IdTypeAdhesion,
            UtilisateurId = user.IdUtilisateur,
            StatutDossier = "VALIDÉ",
            Statut = true
        });
        await db.SaveChangesAsync();

        return (agent.IdAgent, affilie.IdAffilie);
    }

    [Fact]
    public async Task GetAgentKpisAsync_MontantsConsolidesEnDevisePrincipale()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentId, affilieId) = await SeedAgentDataAsync(db);

            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
            db.Devises.AddRange(usd, cdf);
            await db.SaveChangesAsync();

            db.TauxChangeDevises.Add(new TauxChangeDevise
            {
                DeviseSourceId = usd.IdDevise,
                DeviseCibleId = cdf.IdDevise,
                Taux = 2850m,
                DateEffet = new DateTime(2026, 1, 1),
                Statut = true
            });

            var walletUsd = new WalletAgent { AgentId = agentId, DeviseId = usd.IdDevise, Statut = true };
            var walletCdf = new WalletAgent { AgentId = agentId, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.AddRange(walletUsd, walletCdf);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = affilieId,
                    AgentId = agentId,
                    DeviseId = cdf.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    Statut = true,
                    DateCollecte = now
                },
                new Collecte
                {
                    AffilieId = affilieId,
                    AgentId = agentId,
                    DeviseId = usd.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    Statut = true,
                    DateCollecte = now
                });

            db.WalletMouvements.AddRange(
                new WalletMouvement
                {
                    WalletId = walletCdf.IdWalletAgent,
                    DeviseId = cdf.IdDevise,
                    Montant = 285m,
                    TypeOperation = "CREDIT",
                    Source = "COMM_COLLECTE",
                    Statut = true,
                    DateOperation = now
                },
                new WalletMouvement
                {
                    WalletId = walletUsd.IdWalletAgent,
                    DeviseId = usd.IdDevise,
                    Montant = 2m,
                    TypeOperation = "CREDIT",
                    Source = "COMM_COLLECTE",
                    Statut = true,
                    DateOperation = now
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetAgentKpisAsync(agentId);

            Assert.Equal(11m, kpis.TotalCollectesMois);
            Assert.Equal(2.10m, kpis.TotalCommissionsMois);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        }
    }

    [Fact]
    public async Task GetDashboardTerrainAsync_ExposeDevisePrincipaleCode()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentId, _) = await SeedAgentDataAsync(db);
            db.Devises.Add(new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true });
            await db.SaveChangesAsync();

            var dashboard = await service.GetDashboardTerrainAsync(agentId);

            Assert.Equal("USD", dashboard.Kpis.DevisePrincipaleCode);
            Assert.Equal("USD", dashboard.DevisePrincipaleCode);
        }
    }

    [Fact]
    public async Task GetCollectesEnAttenteAsync_Montant_EnDevisePrincipale()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentId, affilieId) = await SeedAgentDataAsync(db);

            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            db.Devises.Add(usd);
            await db.SaveChangesAsync();

            db.Collectes.Add(new Collecte
            {
                AffilieId = affilieId,
                AgentId = agentId,
                DeviseId = usd.IdDevise,
                DevisePrincipaleId = usd.IdDevise,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 2850m,
                MontantDevisePrincipale = 1m,
                Statut = true,
                StatutPaiement = CollecteStatutPaiement.EnAttente,
                DateCollecte = DateTime.Now
            });
            await db.SaveChangesAsync();

            var liste = await service.GetCollectesEnAttenteAsync(agentId);

            Assert.Single(liste);
            Assert.Equal(1m, liste[0].Montant);
        }
    }
}
