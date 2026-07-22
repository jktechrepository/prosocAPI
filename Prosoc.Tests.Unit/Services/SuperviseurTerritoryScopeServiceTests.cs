using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class SuperviseurTerritoryScopeServiceTests
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
    public async Task GetIdsAgentsDansHierarchieAsync_UtilisePérimètreCommunalQuandTitulaire()
    {
        await RunAsync(async (service, db) =>
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zoneA = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            var zoneB = new ZoneSociale { Nom = "Zone B", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.AddRange(zoneA, zoneB);
            await db.SaveChangesAsync();

            var sp = new Agent { NomComplet = "SP", Matricule = "SP-COM", Phone = "0994000001", ZoneSocialeId = zoneA.IdZoneSociale, Statut = true };
            var atZoneA = new Agent { NomComplet = "AT A", Matricule = "AT-A", Phone = "0994000002", ZoneSocialeId = zoneA.IdZoneSociale, Statut = true };
            var atZoneB = new Agent { NomComplet = "AT B", Matricule = "AT-B", Phone = "0994000003", ZoneSocialeId = zoneB.IdZoneSociale, Statut = true };
            db.Agents.AddRange(sp, atZoneA, atZoneB);
            await db.SaveChangesAsync();

            commune.SuperviseurAgentId = sp.IdAgent;
            await db.SaveChangesAsync();

            var ids = await service.GetIdsAgentsDansHierarchieAsync(sp.IdAgent);

            Assert.Contains(sp.IdAgent, ids);
            Assert.Contains(atZoneA.IdAgent, ids);
            Assert.Contains(atZoneB.IdAgent, ids);
            Assert.Equal(3, ids.Count);
        });
    }

    [Fact]
    public async Task EstDansHierarchieAsync_VraiPourAgentDeLaCommune()
    {
        await RunAsync(async (service, db) =>
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var sp = new Agent { NomComplet = "SP", Matricule = "SP-EST", Phone = "0994000010", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            var at = new Agent { NomComplet = "AT", Matricule = "AT-EST", Phone = "0994000011", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            db.Agents.AddRange(sp, at);
            await db.SaveChangesAsync();

            commune.SuperviseurAgentId = sp.IdAgent;
            await db.SaveChangesAsync();

            Assert.True(await service.EstDansHierarchieAsync(sp.IdAgent, at.IdAgent));
            Assert.False(await service.EstDansHierarchieAsync(sp.IdAgent, 99999));
        });
    }
}
