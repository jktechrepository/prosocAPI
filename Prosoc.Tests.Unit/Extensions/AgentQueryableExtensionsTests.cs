using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Extensions;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Extensions;

public class AgentQueryableExtensionsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ProsocDbContext _db;

    public AgentQueryableExtensionsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new ProsocDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<(Role superAdmin, Role admin, Role superviseur, Role at)> SeedRolesAsync()
    {
        var superAdmin = new Role { Nom = "SuperAdmin", Code = "SA", Niveau = 0, Statut = true };
        var admin = new Role { Nom = "Admin", Code = "AD", Niveau = 1, Statut = true };
        var superviseur = new Role { Nom = "Superviseur", Code = "SP", Niveau = 5, Statut = true };
        var at = new Role { Nom = "Agent (AT)", Code = "AT", Niveau = 7, Statut = true };
        _db.Roles.AddRange(superAdmin, admin, superviseur, at);
        await _db.SaveChangesAsync();
        return (superAdmin, admin, superviseur, at);
    }

    private async Task<Agent> SeedAgentWithRoleAsync(string nom, Role role)
    {
        var agent = new Agent
        {
            NomComplet = nom,
            Matricule = nom,
            Phone = "+243900000000",
            EmailAgent = $"{nom.Replace(" ", "").ToLower()}@test.cd",
            Statut = true
        };
        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();

        var user = new Utilisateur
        {
            NomUtilisateur = nom,
            MotDePasseHash = "x",
            AgentId = agent.IdAgent,
            Statut = true
        };
        _db.Utilisateurs.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole
        {
            UtilisateurId = user.IdUtilisateur,
            RoleId = role.IdRole,
            Statut = true,
            IsPrimary = true
        });
        await _db.SaveChangesAsync();
        return agent;
    }

    private async Task<Agent> SeedOrphanAgentAsync(string nom)
    {
        var agent = new Agent
        {
            NomComplet = nom,
            Matricule = nom,
            Phone = "+243911111111",
            EmailAgent = "orphan@test.cd",
            Statut = true
        };
        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();
        return agent;
    }

    [Fact]
    public async Task At_NeVoitPas_Superviseur()
    {
        var (_, _, superviseur, at) = await SeedRolesAsync();
        var agentSp = await SeedAgentWithRoleAsync("Agent SP", superviseur);
        var agentAt = await SeedAgentWithRoleAsync("Agent AT", at);

        var ids = await _db.Agents
            .ApplyRoleNiveauVisibility(_db, callerMinNiveau: 7)
            .Select(a => a.IdAgent)
            .ToListAsync();

        Assert.Contains(agentAt.IdAgent, ids);
        Assert.DoesNotContain(agentSp.IdAgent, ids);
    }

    [Fact]
    public async Task Superviseur_Voit_At()
    {
        var (_, _, superviseur, at) = await SeedRolesAsync();
        var agentSp = await SeedAgentWithRoleAsync("Agent SP2", superviseur);
        var agentAt = await SeedAgentWithRoleAsync("Agent AT2", at);

        var ids = await _db.Agents
            .ApplyRoleNiveauVisibility(_db, callerMinNiveau: 5)
            .Select(a => a.IdAgent)
            .ToListAsync();

        Assert.Contains(agentSp.IdAgent, ids);
        Assert.Contains(agentAt.IdAgent, ids);
    }

    [Fact]
    public async Task SuperAdmin_Voit_Admin_Et_Orphelin()
    {
        var (superAdmin, admin, _, _) = await SeedRolesAsync();
        var agentSa = await SeedAgentWithRoleAsync("Agent SA", superAdmin);
        var agentAd = await SeedAgentWithRoleAsync("Agent AD", admin);
        var orphan = await SeedOrphanAgentAsync("Orphan");

        var ids = await _db.Agents
            .ApplyRoleNiveauVisibility(_db, callerMinNiveau: 0)
            .Select(a => a.IdAgent)
            .ToListAsync();

        Assert.Contains(agentSa.IdAgent, ids);
        Assert.Contains(agentAd.IdAgent, ids);
        Assert.Contains(orphan.IdAgent, ids);
    }

    [Fact]
    public async Task Admin_NeVoitPas_SuperAdmin_Ni_Orphelin()
    {
        var (superAdmin, admin, _, at) = await SeedRolesAsync();
        var agentSa = await SeedAgentWithRoleAsync("Agent SA2", superAdmin);
        var agentAd = await SeedAgentWithRoleAsync("Agent AD2", admin);
        var agentAt = await SeedAgentWithRoleAsync("Agent AT3", at);
        var orphan = await SeedOrphanAgentAsync("Orphan2");

        var ids = await _db.Agents
            .ApplyRoleNiveauVisibility(_db, callerMinNiveau: 1)
            .Select(a => a.IdAgent)
            .ToListAsync();

        Assert.DoesNotContain(agentSa.IdAgent, ids);
        Assert.Contains(agentAd.IdAgent, ids);
        Assert.Contains(agentAt.IdAgent, ids);
        Assert.DoesNotContain(orphan.IdAgent, ids);
    }

    [Fact]
    public async Task MultiRoles_Utilise_MinNiveau()
    {
        var (_, admin, superviseur, at) = await SeedRolesAsync();
        var agent = await SeedAgentWithRoleAsync("Multi", at);
        var user = await _db.Utilisateurs.FirstAsync(u => u.AgentId == agent.IdAgent);
        _db.UserRoles.Add(new UserRole
        {
            UtilisateurId = user.IdUtilisateur,
            RoleId = admin.IdRole,
            Statut = true
        });
        await _db.SaveChangesAsync();

        // Niveau agent = MIN(7, 1) = 1 → visible pour caller Admin (1), pas pour AT (7)
        var visiblePourAdmin = await _db.Agents
            .ApplyRoleNiveauVisibility(_db, 1)
            .AnyAsync(a => a.IdAgent == agent.IdAgent);
        var visiblePourAt = await _db.Agents
            .ApplyRoleNiveauVisibility(_db, 7)
            .AnyAsync(a => a.IdAgent == agent.IdAgent);

        Assert.True(visiblePourAdmin);
        Assert.False(visiblePourAt);
        _ = superviseur;
    }

    [Fact]
    public async Task ResolveCallerMinNiveau_Prend_MinDesRolesJwt()
    {
        await SeedRolesAsync();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Agent (AT)"),
            new Claim(ClaimTypes.Role, "Superviseur")
        }, "test");
        var principal = new ClaimsPrincipal(identity);

        var niveau = await AgentQueryableExtensions.ResolveCallerMinNiveauAsync(_db, principal);
        Assert.Equal(5, niveau);
    }

    [Fact]
    public async Task IsAgentVisible_Respecte_Filtre()
    {
        var (_, _, superviseur, at) = await SeedRolesAsync();
        var agentSp = await SeedAgentWithRoleAsync("SP Vis", superviseur);
        var agentAt = await SeedAgentWithRoleAsync("AT Vis", at);

        Assert.False(await AgentQueryableExtensions.IsAgentVisibleAsync(_db, agentSp.IdAgent, 7));
        Assert.True(await AgentQueryableExtensions.IsAgentVisibleAsync(_db, agentAt.IdAgent, 7));
        Assert.True(await AgentQueryableExtensions.IsAgentVisibleAsync(_db, agentSp.IdAgent, 5));
    }

    [Fact]
    public async Task CanRecharge_SuperviseurVersAt_Autorise()
    {
        var (_, _, superviseur, at) = await SeedRolesAsync();
        var agentAt = await SeedAgentWithRoleAsync("AT Recharge", at);
        var principal = PrincipalWithRoles("Superviseur");

        Assert.True(await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, principal, agentAt.IdAgent));
        _ = superviseur;
    }

    [Fact]
    public async Task CanRecharge_AtVersSuperviseur_Refuse()
    {
        var (_, _, superviseur, _) = await SeedRolesAsync();
        var agentSp = await SeedAgentWithRoleAsync("SP Cible", superviseur);
        var principal = PrincipalWithRoles("Agent (AT)");

        Assert.False(await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, principal, agentSp.IdAgent));
    }

    [Fact]
    public async Task CanRecharge_PairMemeNiveau_Refuse()
    {
        var (_, _, superviseur, _) = await SeedRolesAsync();
        var agentSp = await SeedAgentWithRoleAsync("SP Pair", superviseur);
        var principal = PrincipalWithRoles("Superviseur");

        Assert.False(await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, principal, agentSp.IdAgent));
    }

    [Fact]
    public async Task CanRecharge_AutoRecharge_Refuse()
    {
        var (_, _, _, at) = await SeedRolesAsync();
        var agentAt = await SeedAgentWithRoleAsync("AT Self", at);
        var user = await _db.Utilisateurs.FirstAsync(u => u.AgentId == agentAt.IdAgent);
        var principal = PrincipalWithRolesAndUserId("Agent (AT)", user.IdUtilisateur);

        Assert.False(await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, principal, agentAt.IdAgent));
    }

    [Fact]
    public async Task CanRecharge_SuperAdmin_AutoriseMemeSansRoleCible()
    {
        await SeedRolesAsync();
        var orphan = await SeedOrphanAgentAsync("Orphan WV");
        var principal = PrincipalWithRoles("SuperAdmin");

        Assert.True(await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, principal, orphan.IdAgent));
    }

    [Fact]
    public async Task CanRecharge_Admin_CibleSansRole_Refuse()
    {
        await SeedRolesAsync();
        var orphan = await SeedOrphanAgentAsync("Orphan Admin");
        var principal = PrincipalWithRoles("Admin");

        Assert.False(await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, principal, orphan.IdAgent));
    }

    private static ClaimsPrincipal PrincipalWithRoles(params string[] roles)
    {
        var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ClaimsPrincipal PrincipalWithRolesAndUserId(string role, int utilisateurId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new("uid", utilisateurId.ToString()),
            new("UserId", utilisateurId.ToString())
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
