using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using Prosoc.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Helpers;

public class WalletVirtuelAgentHelpersTests
{
    [Fact]
    public async Task ResolveDeviseIdAsync_SansDeviseId_RetourneDevisePrincipale()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var db = new ProsocDbContext(new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options);
        await db.Database.EnsureCreatedAsync();

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc", EstDevisePrincipale = false, Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();

        var deviseId = await WalletVirtuelAgentHelpers.ResolveDeviseIdAsync(db, null);

        Assert.Equal(usd.IdDevise, deviseId);
    }

    [Fact]
    public async Task ToReadDto_ExposeInfosDevise()
    {
        var devise = new Devise { IdDevise = 1, Code = "USD", Nom = "Dollar", Symbole = "$" };
        var wallet = new WalletVirtuelAgent
        {
            IdWalletVirtuelAgent = 10,
            AgentId = 5,
            DeviseId = devise.IdDevise,
            Devise = devise,
            SoldeVirtuel = 100m,
            Statut = true
        };

        var dto = WalletVirtuelAgentHelpers.ToReadDto(wallet);

        Assert.Equal(1, dto.DeviseId);
        Assert.Equal("USD", dto.DeviseCode);
        Assert.Equal("Dollar", dto.DeviseNom);
        Assert.Equal("$", dto.DeviseSymbole);
    }
}
