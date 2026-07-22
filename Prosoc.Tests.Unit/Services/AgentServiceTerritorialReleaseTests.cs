using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class AgentServiceTerritorialReleaseTests
{
    [Fact]
    public async Task UpdateAsync_DesactivationLibereTitularitesTerritoriales()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var province = new Province { Nom = "Kinshasa", Statut = true };
        db.Provinces.Add(province);
        await db.SaveChangesAsync();
        var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
        db.Communes.Add(commune);
        await db.SaveChangesAsync();
        var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
        db.ZonesSociales.Add(zone);
        await db.SaveChangesAsync();

        var agent = new Agent
        {
            NomComplet = "CE à désactiver",
            Matricule = "CE-OFF",
            Phone = "0996000001",
            ZoneSocialeId = zone.IdZoneSociale,
            Statut = true
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        zone.ChefEquipeAgentId = agent.IdAgent;
        commune.SuperviseurAgentId = agent.IdAgent;
        await db.SaveChangesAsync();

        var territorialMock = new Mock<ITerritorialEncadrementService>();
        territorialMock
            .Setup(s => s.ReleaseTitularitesForAgentAsync(agent.IdAgent, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new AgentService(
            db,
            new Mock<IMatriculeGeneratorService>().Object,
            new Mock<IEmailService>().Object,
            territorialMock.Object,
            NullLogger<AgentService>.Instance);

        var updated = await service.UpdateAsync(agent.IdAgent, new Agent
        {
            NomComplet = agent.NomComplet,
            Matricule = agent.Matricule,
            Phone = agent.Phone,
            ZoneSocialeId = agent.ZoneSocialeId,
            Statut = false,
            DateModification = DateTime.Now
        });

        Assert.NotNull(updated);
        Assert.False(updated!.Statut);
        territorialMock.Verify(
            s => s.ReleaseTitularitesForAgentAsync(agent.IdAgent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_StatutResteActif_NeLiberePasTitularites()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var agent = new Agent
        {
            NomComplet = "Actif",
            Matricule = "ACT-01",
            Phone = "0996000002",
            Statut = true
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var territorialMock = new Mock<ITerritorialEncadrementService>();

        var service = new AgentService(
            db,
            new Mock<IMatriculeGeneratorService>().Object,
            new Mock<IEmailService>().Object,
            territorialMock.Object,
            NullLogger<AgentService>.Instance);

        await service.UpdateAsync(agent.IdAgent, new Agent
        {
            NomComplet = "Actif modifié",
            Matricule = agent.Matricule,
            Phone = agent.Phone,
            Statut = true,
            DateModification = DateTime.Now
        });

        territorialMock.Verify(
            s => s.ReleaseTitularitesForAgentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
