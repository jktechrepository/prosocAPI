using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class WalletVirtuelPaymentServiceTests
{
    private static async Task<(ProsocDbContext db, Devise usd, Devise cdf, WalletVirtuelAgent wallet)> SeedAsync(
        SqliteConnection connection,
        decimal soldeVirtuelUsd = 100m,
        bool withTaux = true)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", EstDevisePrincipale = false, Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();

        if (withTaux)
        {
            db.TauxChangeDevises.Add(new TauxChangeDevise
            {
                DeviseSourceId = usd.IdDevise,
                DeviseCibleId = cdf.IdDevise,
                Taux = 2850m,
                DateEffet = new DateTime(2026, 1, 1),
                Statut = true
            });
            await db.SaveChangesAsync();
        }

        var agent = new Agent { NomComplet = "Agent", Matricule = "AG-WV", Phone = "0990000001", Statut = true };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var wallet = new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = usd.IdDevise,
            SoldeVirtuel = soldeVirtuelUsd,
            Statut = true
        };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();

        wallet.Devise = usd;
        return (db, usd, cdf, wallet);
    }

    private static WalletVirtuelPaymentService CreateService(ProsocDbContext db) =>
        new(db, new DeviseConversionService(db), new WalletVirtuelMouvementService(db));

    [Fact]
    public async Task ComputeMontantDebitAsync_CdfCollecteUsdWallet_ConvertitVersUsd()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf, wallet) = await SeedAsync(connection);
        var service = CreateService(db);

        var collecte = new Collecte
        {
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            DateCollecte = new DateTime(2026, 5, 1)
        };

        var montantDebit = await service.ComputeMontantDebitAsync(collecte, wallet, collecte.DateCollecte);

        Assert.Equal(10m, montantDebit);
    }

    [Fact]
    public async Task ComputeMontantDebitAsync_UsdCollecteCdfWallet_ConvertitVersCdf()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf, _) = await SeedAsync(connection);

        var agent2 = new Agent { NomComplet = "Agent2", Matricule = "AG-WV2", Phone = "0990000002", Statut = true };
        db.Agents.Add(agent2);
        await db.SaveChangesAsync();

        var walletCdf = new WalletVirtuelAgent
        {
            AgentId = agent2.IdAgent,
            DeviseId = cdf.IdDevise,
            SoldeVirtuel = 28500m,
            Statut = true
        };
        db.WalletsVirtuelsAgents.Add(walletCdf);
        await db.SaveChangesAsync();
        walletCdf.Devise = cdf;

        var service = CreateService(db);
        var collecte = new Collecte
        {
            Montant = 10m,
            DeviseId = usd.IdDevise,
            DateCollecte = new DateTime(2026, 5, 1)
        };

        var montantDebit = await service.ComputeMontantDebitAsync(collecte, walletCdf, collecte.DateCollecte);

        Assert.Equal(28500m, montantDebit);
    }

    [Fact]
    public async Task ComputeMontantDebitAsync_MemeDevise_PasDeConversion()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, _, wallet) = await SeedAsync(connection);
        var service = CreateService(db);

        var collecte = new Collecte
        {
            Montant = 42.50m,
            DeviseId = usd.IdDevise,
            DateCollecte = new DateTime(2026, 5, 1)
        };

        var montantDebit = await service.ComputeMontantDebitAsync(collecte, wallet, collecte.DateCollecte);

        Assert.Equal(42.50m, montantDebit);
    }

    [Fact]
    public async Task ValidateSoldeSuffisantAsync_SoldeInsuffisantApresConversion_LeveException()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, _, cdf, wallet) = await SeedAsync(connection, soldeVirtuelUsd: 5m);
        var service = CreateService(db);

        var collecte = new Collecte
        {
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            DateCollecte = new DateTime(2026, 5, 1)
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ValidateSoldeSuffisantAsync(collecte, wallet, collecte.DateCollecte));

        Assert.Contains("insuffisant", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("USD", ex.Message);
    }

    [Fact]
    public async Task ComputeMontantDebitAsync_TauxManquant_LeveException()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, _, cdf, wallet) = await SeedAsync(connection, withTaux: false);
        var service = CreateService(db);

        var collecte = new Collecte
        {
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            DateCollecte = new DateTime(2026, 5, 1)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ComputeMontantDebitAsync(collecte, wallet, collecte.DateCollecte));
    }

    [Fact]
    public async Task ComputeMontantDebitAsync_VirtualAccount_UtiliseTauxDuJourPasDatePeriode()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf, wallet) = await SeedAsync(connection, withTaux: false);
        var service = CreateService(db);

        db.TauxChangeDevises.Add(new TauxChangeDevise
        {
            DeviseSourceId = usd.IdDevise,
            DeviseCibleId = cdf.IdDevise,
            Taux = 2850m,
            DateEffet = DateTime.UtcNow.Date,
            Statut = true
        });
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            ModePaiement = "VIRTUAL_ACCOUNT",
            DateCollecte = new DateTime(2026, 3, 1)
        };

        var montantDebit = await service.ComputeMontantDebitAsync(collecte, wallet, collecte.DateCollecte);

        Assert.Equal(10m, montantDebit);
    }

    [Fact]
    public async Task DebitAsync_CrossDevise_DebiteMontantConverti()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf, wallet) = await SeedAsync(connection);
        var service = CreateService(db);
        var agent = await db.Agents.FirstAsync();

        var collecte = new Collecte
        {
            IdCollecte = 1,
            TypeCollecte = TypeCollecte.Souscription,
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            ModePaiement = "VIRTUAL_ACCOUNT",
            AgentId = agent.IdAgent,
            AffilieId = 1,
            DateCollecte = new DateTime(2026, 5, 1),
            Statut = true
        };

        await service.DebitAsync(collecte, agent.IdAgent);

        var updated = await db.WalletsVirtuelsAgents.FirstAsync();
        Assert.Equal(90m, updated.SoldeVirtuel);

        var mouvement = await db.WalletVirtuelMouvements.SingleAsync();
        Assert.Equal(10m, mouvement.Montant);
        Assert.Equal("DEBIT", mouvement.TypeOperation);
    }
}
