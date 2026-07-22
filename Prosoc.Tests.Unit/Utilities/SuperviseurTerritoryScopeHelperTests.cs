using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Utilities;

public class SuperviseurTerritoryScopeHelperTests
{
    private static async Task<(ProsocDbContext Db, SqliteConnection Connection)> CreateDbAsync()
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

    [Fact]
    public async Task GetCommuneIdForSuperviseurAsync_RetourneCommuneTitulaire()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
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

            var sp = new Agent { NomComplet = "SP", Matricule = "SP-01", Phone = "0993000001", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            db.Agents.Add(sp);
            await db.SaveChangesAsync();

            commune.SuperviseurAgentId = sp.IdAgent;
            await db.SaveChangesAsync();

            var communeId = await SuperviseurTerritoryScopeHelper.GetCommuneIdForSuperviseurAsync(db, sp.IdAgent);

            Assert.Equal(commune.IdCommune, communeId);
        }
    }

    [Fact]
    public async Task GetAgentIdsDansCommuneAsync_InclutAgentsDesDeuxZones()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
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

            var atA = new Agent { NomComplet = "AT A", Matricule = "AT-A", Phone = "0993000010", ZoneSocialeId = zoneA.IdZoneSociale, Statut = true };
            var atB = new Agent { NomComplet = "AT B", Matricule = "AT-B", Phone = "0993000011", ZoneSocialeId = zoneB.IdZoneSociale, Statut = true };
            var horsCommune = new Agent { NomComplet = "Hors", Matricule = "HORS", Phone = "0993000012", Statut = true };
            db.Agents.AddRange(atA, atB, horsCommune);
            await db.SaveChangesAsync();

            var ids = await SuperviseurTerritoryScopeHelper.GetAgentIdsDansCommuneAsync(db, commune.IdCommune);

            Assert.Equal(2, ids.Count);
            Assert.Contains(atA.IdAgent, ids);
            Assert.Contains(atB.IdAgent, ids);
            Assert.DoesNotContain(horsCommune.IdAgent, ids);
        }
    }
}
