using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAgentObjectifsTests
{
    [Fact]
    public async Task ResolveRoleAndTarget_ForLinkedAgent_ReturnsMonthlyTarget()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var atRole = new Role { Nom = "Agent (AT)", Code = "AT", Statut = true };
        db.Roles.Add(atRole);
        await db.SaveChangesAsync();

        var agent = new Agent
        {
            NomComplet = "AT Test",
            Matricule = "AT000000099",
            Phone = "0990000099",
            RoleAgent = "Agent (AT)",
            Statut = true
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.Utilisateurs.Add(new Utilisateur
        {
            NomUtilisateur = "at_test",
            EmailUtilisateur = "at_test@local.test",
            PhoneUtilisateur = "0880000099",
            MotDePasseHash = "hash",
            RoleId = atRole.IdRole,
            AgentId = agent.IdAgent,
            Statut = true
        });

        db.TargetsAgents.Add(new TargetAgent
        {
            RoleId = atRole.IdRole,
            LibelleTarget = "Objectif mensuel AT",
            Periodicite = PeriodiciteTarget.Mensuelle,
            Nombre = 100,
            Statut = true
        });
        await db.SaveChangesAsync();

        var roleId = await TargetAgentRoleResolver.ResolveRoleIdForAgentAsync(db, agent.IdAgent);
        Assert.Equal(atRole.IdRole, roleId);

        var targetMensuel = await db.TargetsAgents
            .AsNoTracking()
            .Where(t => t.RoleId == roleId!.Value
                && t.Statut
                && t.Periodicite == PeriodiciteTarget.Mensuelle)
            .OrderByDescending(t => t.DateCreation)
            .FirstOrDefaultAsync();

        Assert.NotNull(targetMensuel);
        Assert.Equal(100, targetMensuel.Nombre);
    }

    [Fact]
    public async Task ResolveRoleIdForAgentAsync_FallsBackToAgentRoleAgent()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        db.Roles.Add(new Role { Nom = "Agent (AT)", Code = "AT", Statut = true });
        db.Agents.Add(new Agent
        {
            NomComplet = "AT Sans User",
            Matricule = "AT000000100",
            Phone = "0990000100",
            RoleAgent = "Agent (AT)",
            Statut = true
        });
        await db.SaveChangesAsync();

        var agentId = await db.Agents.Select(a => a.IdAgent).FirstAsync();
        var roleId = await TargetAgentRoleResolver.ResolveRoleIdForAgentAsync(db, agentId);

        Assert.NotNull(roleId);
        Assert.Equal("Agent (AT)", await db.Roles.Where(r => r.IdRole == roleId).Select(r => r.Nom).FirstAsync());
    }

    [Fact]
    public async Task GetObjectifsAsync_UsesMonthlyTargetFromAgentRole()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var atRole = new Role { Nom = "Agent (AT)", Code = "AT", Statut = true };
        db.Roles.Add(atRole);
        await db.SaveChangesAsync();

        var agent = new Agent
        {
            NomComplet = "AT Objectifs",
            Matricule = "AT000000101",
            Phone = "0990000101",
            RoleAgent = "Agent (AT)",
            Statut = true
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.Utilisateurs.Add(new Utilisateur
        {
            NomUtilisateur = "at_obj",
            EmailUtilisateur = "at_obj@local.test",
            PhoneUtilisateur = "0880000101",
            MotDePasseHash = "hash",
            RoleId = atRole.IdRole,
            AgentId = agent.IdAgent,
            Statut = true
        });

        db.TargetsAgents.Add(new TargetAgent
        {
            RoleId = atRole.IdRole,
            LibelleTarget = "Objectif mensuel AT",
            Periodicite = PeriodiciteTarget.Mensuelle,
            Nombre = 100,
            Statut = true
        });
        await db.SaveChangesAsync();

        var arrieresProvider = new Mock<IParametresMetierProvider>();
        arrieresProvider.Setup(p => p.GetArrieresAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProsocAPI.Models.Configuration.ArrieresOptions());

        var service = new DashboardAgentService(
            db,
            new DeviseConversionService(db),
            new AffilieConformiteService(db, new ArrieresAffilieService(
                db,
                Mock.Of<ICotisationAffilieMetierService>(),
                arrieresProvider.Object,
                Mock.Of<ILogger<ArrieresAffilieService>>())),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DashboardAgentService>.Instance);

        var mois = DateTime.Now.Month;
        var annee = DateTime.Now.Year;
        var objectifs = await service.GetObjectifsAsync(agent.IdAgent, mois, annee);

        Assert.Equal(100, objectifs.ObjectifAdhesions);
    }
}
