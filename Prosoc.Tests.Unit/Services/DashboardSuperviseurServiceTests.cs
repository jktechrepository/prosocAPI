using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Exceptions;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardSuperviseurServiceTests
{
    private static async Task RunAsync(Func<SuperviseurService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new SuperviseurService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<SuperviseurService>>().Object);

        await test(service, db);
    }

    [Fact]
    public async Task GetMontantTotalHierarchieAsync_MontantsConsolidesEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
            db.Devises.AddRange(usd, cdf);
            await db.SaveChangesAsync();

            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var superviseur = new Agent
            {
                NomComplet = "Superviseur Test",
                Matricule = "SUP-001",
                Phone = "0990000100",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true
            };
            db.Agents.Add(superviseur);
            await db.SaveChangesAsync();

            var subordonne = new Agent
            {
                NomComplet = "Agent Sub",
                Matricule = "AG-SUB-001",
                Phone = "0990000101",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true
            };
            db.Agents.Add(subordonne);
            commune.SuperviseurAgentId = superviseur.IdAgent;

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-SUP-1",
                Nom = "Test",
                Prenom = "Sup",
                NomComplet = "Test Sup",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = affilie.IdAffilie,
                    AgentId = subordonne.IdAgent,
                    DeviseId = cdf.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    Statut = true,
                    DateCollecte = DateTime.Now.AddMonths(-1)
                },
                new Collecte
                {
                    AffilieId = affilie.IdAffilie,
                    AgentId = subordonne.IdAgent,
                    DeviseId = usd.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    Statut = true,
                    DateCollecte = DateTime.Now.AddMonths(-1)
                });
            await db.SaveChangesAsync();

            var montant = await service.GetMontantTotalHierarchieAsync(superviseur.IdAgent);
            var stats = await service.GetStatsSuperviseurAsync(superviseur.IdAgent);

            Assert.Equal(11m, montant);
            Assert.Equal(11m, stats.MontantTotalEquipe);
            Assert.Equal("USD", stats.DevisePrincipaleCode);
        });
    }

    [Fact]
    public async Task GetPerformancesAgentsAsync_CommissionsConvertiesEnDevisePrincipale()
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

            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var superviseur = new Agent
            {
                NomComplet = "Superviseur Comm",
                Matricule = "SUP-002",
                Phone = "0990000102",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true
            };
            db.Agents.Add(superviseur);
            await db.SaveChangesAsync();

            var subordonne = new Agent
            {
                NomComplet = "Agent Comm Sub",
                Matricule = "AG-SUB-002",
                Phone = "0990000103",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true
            };
            db.Agents.Add(subordonne);
            commune.SuperviseurAgentId = superviseur.IdAgent;
            await db.SaveChangesAsync();

            var wallet = new WalletAgent { AgentId = subordonne.IdAgent, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
            db.WalletMouvements.Add(new WalletMouvement
            {
                WalletId = wallet.IdWalletAgent,
                DeviseId = cdf.IdDevise,
                Montant = 285m,
                TypeOperation = "CREDIT",
                Source = "COMM_COLLECTE",
                DateOperation = now
            });
            await db.SaveChangesAsync();

            var performances = await service.GetPerformancesAgentsAsync(superviseur.IdAgent);
            var perfSubordonne = performances.Single(p => p.AgentId == subordonne.IdAgent);

            Assert.Equal(0.10m, perfSubordonne.MontantCommissions);
        });
    }

    [Fact]
    public async Task GetIdsAgentsDansHierarchieAsync_SansCommuneTitulaire_LeveSuperviseurSansCommuneTitulaireException()
    {
        await RunAsync(async (service, db) =>
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Zone SP", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var superviseur = new Agent
            {
                NomComplet = "Superviseur Orphelin",
                Matricule = "SUP-ORP",
                Phone = "0990000199",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true
            };
            db.Agents.Add(superviseur);
            await db.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<SuperviseurSansCommuneTitulaireException>(
                () => service.GetIdsAgentsDansHierarchieAsync(superviseur.IdAgent));

            Assert.Equal(superviseur.IdAgent, ex.SuperviseurAgentId);
        });
    }

    [Fact]
    public async Task GetDashboardSuperviseurAsync_ExposeDevisePrincipaleCode()
    {
        await RunAsync(async (service, db) =>
        {
            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            db.Devises.Add(usd);

            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var superviseur = new Agent
            {
                NomComplet = "Superviseur Dash",
                Matricule = "SUP-003",
                Phone = "0990000104",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true
            };
            db.Agents.Add(superviseur);
            await db.SaveChangesAsync();
            
            commune.SuperviseurAgentId = superviseur.IdAgent;
            await db.SaveChangesAsync();

            var dashboard = await service.GetDashboardSuperviseurAsync(superviseur.IdAgent);

            Assert.Equal("USD", dashboard.StatsSuperviseur.DevisePrincipaleCode);
            Assert.Equal("USD", dashboard.DevisePrincipaleCode);
        });
    }
}
