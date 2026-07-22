using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardPercepteurServiceTests
{
    private static async Task RunAsync(Func<DashboardPercepteurService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var perceptionMock = new Mock<IPerceptionVirtuelleService>();
        perceptionMock.Setup(x => x.GetTotauxVirtuelsEnAttenteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((0m, 0));

        var service = new DashboardPercepteurService(
            db,
            new DeviseConversionService(db),
            perceptionMock.Object,
            new Mock<ILogger<DashboardPercepteurService>>().Object);

        await test(service, db);
    }

    [Fact]
    public async Task GetKpisPercepteurAsync_MontantsConsolidesEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
            db.Devises.AddRange(usd, cdf);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Perc",
                Matricule = "AG000000060",
                Phone = "0990000060",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-PERC-1",
                Nom = "Test",
                Prenom = "Perc",
                NomComplet = "Test Perc",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    DeviseId = cdf.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    Statut = true,
                    DateCollecte = DateTime.Today.AddHours(9)
                },
                new Collecte
                {
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
                    DeviseId = usd.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    Statut = true,
                    DateCollecte = DateTime.Today.AddHours(10)
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisPercepteurAsync();

            Assert.Equal(11m, kpis.MontantDuJour);
            Assert.Equal(11m, kpis.MontantSemaine);
            Assert.Equal(11m, kpis.MontantMois);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        });
    }

    [Fact]
    public async Task GetMontantCommissionsAsync_ConvertitEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
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

            var agent = new Agent
            {
                NomComplet = "Agent Comm",
                Matricule = "AG000000061",
                Phone = "0990000061",
                Statut = true
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            var wallet = new WalletAgent { AgentId = agent.IdAgent, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
            db.WalletMouvements.Add(new WalletMouvement
            {
                WalletId = wallet.IdWalletAgent,
                DeviseId = cdf.IdDevise,
                Montant = 2850m,
                TypeOperation = "CREDIT",
                Source = "COMM_COLLECTE",
                DateOperation = now
            });
            await db.SaveChangesAsync();

            var total = await service.GetMontantCommissionsAsync(now.AddDays(-1), now.AddDays(1));

            Assert.Equal(1m, total);
        });
    }

    [Fact]
    public async Task GetRapportPerceptionAsync_ClassifieAgentVaEtAffilieGuichet()
    {
        await RunAsync(async (service, db) =>
        {
            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            db.Devises.Add(usd);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Rapport",
                Matricule = "AG-RAP-001",
                Phone = "0990000100",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-RAP-1",
                Nom = "Dupont",
                Prenom = "Jean",
                NomComplet = "Jean Dupont",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var wallet = new WalletVirtuelAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = usd.IdDevise,
                SoldeVirtuel = 500m,
                Statut = true
            };
            db.WalletsVirtuelsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var collecteVa = new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = usd.IdDevise,
                DevisePrincipaleId = usd.IdDevise,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 50m,
                MontantDevisePrincipale = 50m,
                ModePaiement = MethodePaiementHelper.VirtualAccount,
                StatutPaiement = CollecteStatutPaiement.Valide,
                StatutPerception = CollecteStatutPerception.NonPerçu,
                Statut = true,
                DateCollecte = DateTime.Today
            };
            var collecteEspece = new Collecte
            {
                AffilieId = affilie.IdAffilie,
                DeviseId = usd.IdDevise,
                DevisePrincipaleId = usd.IdDevise,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 30m,
                MontantDevisePrincipale = 30m,
                ModePaiement = MethodePaiementHelper.Espece,
                StatutPaiement = CollecteStatutPaiement.Valide,
                Statut = true,
                DateCollecte = DateTime.Today.AddHours(1)
            };
            db.Collectes.AddRange(collecteVa, collecteEspece);
            await db.SaveChangesAsync();

            db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
            {
                WalletVirtuelId = wallet.IdWalletVirtuelAgent,
                Montant = 50m,
                TypeOperation = "DEBIT",
                Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
                ReferenceExterne = collecteVa.IdCollecte,
                Statut = true
            });
            await db.SaveChangesAsync();

            var mouvementId = await db.WalletVirtuelMouvements
                .Where(m => m.ReferenceExterne == collecteVa.IdCollecte)
                .Select(m => m.IdWalletVirtuelMouvement)
                .FirstAsync();

            var rapport = await service.GetRapportPerceptionAsync(
                pagination: new PaginationRequest { Page = 1, PageSize = 20 });

            Assert.Equal(2, rapport.Lignes.TotalItems);
            Assert.Equal(50m, rapport.Synthese.Agent.MontantEnAttente);
            Assert.Equal(1, rapport.Synthese.Agent.NombreEnAttente);
            Assert.Equal(30m, rapport.Synthese.Affilie.MontantPerçu);
            Assert.Equal(1, rapport.Synthese.Affilie.NombrePerçu);
            Assert.Equal(30m, rapport.Synthese.TotalPerçu);

            var ligneVa = rapport.Lignes.Data.First(l => l.IdCollecte == collecteVa.IdCollecte);
            Assert.Equal(PerceptionOrigineHelper.OrigineAgent, ligneVa.OriginePerception);
            Assert.Equal(PerceptionOrigineHelper.StatutEnAttente, ligneVa.StatutPerception);
            Assert.Equal(mouvementId, ligneVa.WalletVirtuelMouvementId);

            var ligneEspece = rapport.Lignes.Data.First(l => l.IdCollecte == collecteEspece.IdCollecte);
            Assert.Equal(PerceptionOrigineHelper.OrigineAffilie, ligneEspece.OriginePerception);
            Assert.Equal(PerceptionOrigineHelper.StatutPercu, ligneEspece.StatutPerception);
        });
    }

    [Fact]
    public async Task GetRapportPerceptionAsync_FiltreStatutEnAttente_ExclutAffilie()
    {
        await RunAsync(async (service, db) =>
        {
            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            db.Devises.Add(usd);

            var agent = new Agent
            {
                NomComplet = "Agent Filtre",
                Matricule = "AG-RAP-002",
                Phone = "0990000101",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-RAP-2",
                Nom = "Test",
                Prenom = "Filtre",
                NomComplet = "Test Filtre",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var wallet = new WalletVirtuelAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = usd.IdDevise,
                SoldeVirtuel = 100m,
                Statut = true
            };
            db.WalletsVirtuelsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var collecteVa = new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = usd.IdDevise,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 40m,
                MontantDevisePrincipale = 40m,
                ModePaiement = MethodePaiementHelper.VirtualAccount,
                StatutPaiement = CollecteStatutPaiement.Valide,
                StatutPerception = CollecteStatutPerception.NonPerçu,
                Statut = true,
                DateCollecte = DateTime.Today
            };
            db.Collectes.Add(collecteVa);
            await db.SaveChangesAsync();

            db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
            {
                WalletVirtuelId = wallet.IdWalletVirtuelAgent,
                Montant = 40m,
                TypeOperation = "DEBIT",
                Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
                ReferenceExterne = collecteVa.IdCollecte,
                Statut = true
            });
            await db.SaveChangesAsync();

            var rapport = await service.GetRapportPerceptionAsync(
                statut: PerceptionOrigineHelper.StatutEnAttente,
                pagination: new PaginationRequest { Page = 1, PageSize = 20 });

            Assert.Single(rapport.Lignes.Data);
            Assert.Equal(40m, rapport.Synthese.Agent.MontantEnAttente);
            Assert.Equal(0m, rapport.Synthese.Affilie.MontantPerçu);
        });
    }
}
