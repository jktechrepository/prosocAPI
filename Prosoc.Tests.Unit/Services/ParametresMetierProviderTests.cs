using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using Xunit;

namespace Prosoc.Tests.Unit.Services;

public class ParametresMetierProviderTests
{
    private static async Task<(ProsocDbContext db, SqliteConnection connection)> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (db, connection);
    }

    private static ParametresMetierProvider CreateProvider(
        ProsocDbContext db,
        RetraitAgentOptions? defaults = null)
    {
        return new ParametresMetierProvider(
            db,
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(defaults ?? new RetraitAgentOptions
            {
                Fenetre1Debut = 15,
                Fenetre1Fin = 20,
                Fenetre2DerniersJours = 7,
                MontantMinimumPartiel = 5
            }),
            Options.Create(new AgentMaashOptions()),
            Options.Create(new ArrieresOptions()),
            Options.Create(new PenaliteOptions()),
            NullLogger<ParametresMetierProvider>.Instance);
    }

    [Fact]
    public async Task GetRetraitAgentAsync_SeedsFromDefaults_WhenRowMissing()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var provider = CreateProvider(db);

            var options = await provider.GetRetraitAgentAsync();

            Assert.Equal(15, options.Fenetre1Debut);
            Assert.Equal(5, options.MontantMinimumPartiel);
            Assert.Single(await db.ParametresMetier.Where(p => p.Code == ParametreMetierCodes.RetraitAgent).ToListAsync());
        }
    }

    [Fact]
    public async Task UpdateRetraitAgentAsync_PersistsAndInvalidatesCache()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var provider = CreateProvider(db);
            await provider.GetRetraitAgentAsync();

            var updated = await provider.UpdateRetraitAgentAsync(
                new RetraitAgentParametresUpdateDto
                {
                    Fenetre1Debut = 10,
                    Fenetre1Fin = 12,
                    Fenetre2DerniersJours = 3,
                    MontantMinimumPartiel = 20
                },
                utilisateurId: 0);

            Assert.Equal(10, updated.Fenetre1Debut);
            Assert.Equal(20, updated.MontantMinimumPartiel);

            var reloaded = await provider.GetRetraitAgentAsync();
            Assert.Equal(10, reloaded.Fenetre1Debut);
            Assert.Equal(20, reloaded.MontantMinimumPartiel);
        }
    }

    [Fact]
    public async Task UpdateRetraitAgentAsync_InvalidDto_Throws()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var provider = CreateProvider(db);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                provider.UpdateRetraitAgentAsync(
                    new RetraitAgentParametresUpdateDto
                    {
                        Fenetre1Debut = 15,
                        Fenetre1Fin = 20,
                        Fenetre2DerniersJours = 7,
                        MontantMinimumPartiel = -1
                    },
                    0));
        }
    }
}
