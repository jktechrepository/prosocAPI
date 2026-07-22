using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardFinancierServiceTests
{
    private static async Task RunAsync(Func<DashboardFinancierService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new DashboardFinancierService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<DashboardFinancierService>>().Object);

        await test(service, db);
    }

    [Fact]
    public async Task GetKpisFinanciersAsync_MontantsConsolidesEnDevisePrincipale()
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
                NomComplet = "Agent Fin",
                Matricule = "AG000000050",
                Phone = "0990000050",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-FIN-1",
                Nom = "Test",
                Prenom = "Fin",
                NomComplet = "Test Fin",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var walletUsd = new WalletAgent { AgentId = agent.IdAgent, DeviseId = usd.IdDevise, Statut = true };
            var walletCdf = new WalletAgent { AgentId = agent.IdAgent, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.AddRange(walletUsd, walletCdf);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
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
                    DateCollecte = now
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
                    DateOperation = now
                },
                new WalletMouvement
                {
                    WalletId = walletUsd.IdWalletAgent,
                    DeviseId = usd.IdDevise,
                    Montant = 2m,
                    TypeOperation = "CREDIT",
                    Source = "COMM_COLLECTE",
                    DateOperation = now
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisFinanciersAsync();

            Assert.Equal(11m, kpis.MontantTotalCollectes);
            Assert.Equal(11m, kpis.ChiffreAffairesTotal);
            Assert.Equal(2.10m, kpis.MontantTotalCommissions);
            Assert.Equal("USD", kpis.CodeDeviseConsolidation);
        });
    }

    [Fact]
    public async Task GetCommissionsAgentsAsync_SommeCommissionsEnDevisePrincipale()
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
                Matricule = "AG000000051",
                Phone = "0990000051",
                Statut = true
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            var walletCdf = new WalletAgent { AgentId = agent.IdAgent, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.Add(walletCdf);
            await db.SaveChangesAsync();

            db.WalletMouvements.Add(new WalletMouvement
            {
                WalletId = walletCdf.IdWalletAgent,
                DeviseId = cdf.IdDevise,
                Montant = 2850m,
                TypeOperation = "CREDIT",
                Source = "COMM_COLLECTE",
                DateOperation = DateTime.Now
            });
            await db.SaveChangesAsync();

            var commissions = await service.GetCommissionsAgentsAsync();
            var agentCommission = commissions.Single(c => c.AgentId == agent.IdAgent);

            Assert.Equal(1m, agentCommission.MontantCommission);
        });
    }
}
