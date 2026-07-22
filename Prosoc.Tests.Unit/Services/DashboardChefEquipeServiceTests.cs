using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardChefEquipeServiceTests
{
    private static async Task<(ProsocDbContext Db, DashboardChefEquipeService Service)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var dashboardAgent = new Mock<IDashboardAgentRepository>();
        dashboardAgent
            .Setup(x => x.GetCommissionsResumeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentCommissionsResumeDto { SoldeWallet = 0m });

        var service = new DashboardChefEquipeService(
            db,
            dashboardAgent.Object,
            new Mock<ILogger<DashboardChefEquipeService>>().Object);

        return (db, service);
    }

    [Fact]
    public async Task GetAgentsZoneAsync_RetourneSeulementAtDeLaZone()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var atRole = new Role { Nom = "Agent (AT)", Code = "AT", Statut = true };
            db.Roles.Add(atRole);
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune1 = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            var commune2 = new Commune { Nom = "Ngaliema", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.AddRange(commune1, commune2);
            await db.SaveChangesAsync();

            var zone1 = new ZoneSociale { Nom = "Zone 1", CommuneId = commune1.IdCommune, Statut = true };
            var zone2 = new ZoneSociale { Nom = "Zone 2", CommuneId = commune2.IdCommune, Statut = true };
            db.ZonesSociales.AddRange(zone1, zone2);
            await db.SaveChangesAsync();

            var chef = new Agent { NomComplet = "Chef", Matricule = "CHEF-10", Phone = "0991000001", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            var atZone = new Agent { NomComplet = "AT Zone", Matricule = "ATZ-10", Phone = "0991000002", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            var atHorsZone = new Agent { NomComplet = "AT Hors", Matricule = "ATH-10", Phone = "0991000003", ZoneSocialeId = zone2.IdZoneSociale, Statut = true };
            db.Agents.AddRange(chef, atZone, atHorsZone);
            await db.SaveChangesAsync();

            var u1 = new Utilisateur { NomUtilisateur = "atz", MotDePasseHash = "x", AgentId = atZone.IdAgent, Statut = true };
            var u2 = new Utilisateur { NomUtilisateur = "ath", MotDePasseHash = "x", AgentId = atHorsZone.IdAgent, Statut = true };
            db.Utilisateurs.AddRange(u1, u2);
            await db.SaveChangesAsync();

            db.UserRoles.AddRange(
                new UserRole { UtilisateurId = u1.IdUtilisateur, RoleId = atRole.IdRole, Statut = true },
                new UserRole { UtilisateurId = u2.IdUtilisateur, RoleId = atRole.IdRole, Statut = true });
            await db.SaveChangesAsync();

            var agents = await service.GetAgentsZoneAsync(chef.IdAgent);

            Assert.Single(agents);
            Assert.Equal(atZone.IdAgent, agents[0].AgentId);
        }
    }
}
