using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.DashboardAgentAa;
using Prosoc.Models.DTOs.CategorieAgent;
using Xunit;

namespace Prosoc.Tests.Integration;

/// <summary>
/// Vérifie que le dashboard AA est vide sans affectation et se remplit après affecter-affilies.
/// </summary>
public class DashboardAgentAAIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private static int _phoneSequence;
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private string _originalUserId = TestAuthHandler.UserId;
    private IReadOnlyList<string> _originalRoles = TestAuthHandler.Roles;

    public DashboardAgentAAIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        TestAuthHandler.UserId = _originalUserId;
        TestAuthHandler.Roles = _originalRoles;
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Summary_SansAdhesionAffectee_RetourneZeros()
    {
        var (aaAgentId, aaUserId) = await CreateAgentAaWithUserAsync("empty");
        var atAgent = await CreateAgentAsync("at-empty");

        var seed = await SeedAffilieWithAdhesionAsync(
            atAgent.Id,
            "dash-aa-empty",
            statutDossier: "EN ATTENTE");
        var affilieId = seed.AffilieId;

        AuthenticateAs(aaUserId, aaAgentId);

        var response = await _client.GetAsync("/api/DashboardAgentAA/summary");
        response.EnsureSuccessStatusCode();

        var summary = await response.Content.ReadFromJsonAsync<DashboardAgentAaDto>();
        Assert.NotNull(summary);
        Assert.Equal(aaAgentId, summary!.AgentId);
        Assert.Equal(0, summary.Kpis.TotalDossiers);
        Assert.Empty(summary.RepartitionStatuts);
        Assert.Empty(summary.DossiersATraiter);

        // Dossier existe mais rattaché à l'agent AT, pas à l'AA
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var adhesion = await db.Adhesions.AsNoTracking()
                .FirstAsync(a => a.AffilieId == affilieId);
            Assert.Equal(atAgent.Id, adhesion.AgentId);
        }
    }

    [Fact]
    public async Task Summary_ApresAffecterAffilies_RetourneKpisNonNuls()
    {
        var (aaAgentId, aaUserId) = await CreateAgentAaWithUserAsync("filled");
        var atAgent = await CreateAgentAsync("at-filled");

        var seed = await SeedAffilieWithAdhesionAsync(
            atAgent.Id,
            "dash-aa-fill",
            statutDossier: "EN ATTENTE");
        var affilieId = seed.AffilieId;

        // Affectation métier (Admin)
        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.UserId = "1";

        var affectResponse = await _client.PutAsJsonAsync(
            $"/api/Agent/{aaAgentId}/affecter-affilies",
            new AgentAffecterAffiliesDto { AffilieIds = new List<int> { affilieId } });
        affectResponse.EnsureSuccessStatusCode();

        AuthenticateAs(aaUserId, aaAgentId);

        var response = await _client.GetAsync("/api/DashboardAgentAA/summary");
        response.EnsureSuccessStatusCode();

        var summary = await response.Content.ReadFromJsonAsync<DashboardAgentAaDto>();
        Assert.NotNull(summary);
        Assert.Equal(1, summary!.Kpis.TotalDossiers);
        Assert.Equal(1, summary.Kpis.DossiersEnAttente);
        Assert.Single(summary.RepartitionStatuts);
        Assert.Single(summary.DossiersATraiter);
        Assert.Equal("EN ATTENTE", summary.DossiersATraiter[0].StatutDossier);
    }

    private void AuthenticateAs(int userId, int agentId)
    {
        TestAuthHandler.UserId = userId.ToString();
        TestAuthHandler.Roles = new[] { "Agent (AA)" };
    }

    private async Task<(int AgentId, int UserId)> CreateAgentAaWithUserAsync(string suffix)
    {
        var agent = await CreateAgentAsync($"aa-{suffix}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var aaRoleId = await db.Roles
            .Where(r => r.Nom == "Agent (AA)")
            .Select(r => r.IdRole)
            .FirstAsync();

        var user = new Utilisateur
        {
            NomUtilisateur = $"aa-user-{suffix}",
            MotDePasseHash = "hash",
            AgentId = agent.Id,
            Statut = true,
            DateCreation = DateTime.Now
        };
        db.Utilisateurs.Add(user);
        await db.SaveChangesAsync();

        db.UserRoles.Add(new UserRole
        {
            UtilisateurId = user.IdUtilisateur,
            RoleId = aaRoleId,
            IsPrimary = true,
            Statut = true,
            DateAttribution = DateTime.Now
        });
        await db.SaveChangesAsync();

        return (agent.Id, user.IdUtilisateur);
    }

    private async Task<AgentReadDto> CreateAgentAsync(string suffix)
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = $"D{unique[..6]}",
            Description = "Catégorie test dashboard AA"
        };

        var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        categorieResponse.EnsureSuccessStatusCode();
        var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
        Assert.NotNull(createdCategorie);

        var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
        var agentDto = new AgentCreateDto
        {
            NomComplet = $"Agent Dash AA {suffix} {unique}",
            Matricule = $"DA{unique.PadRight(9, '0')}"[..11],
            Phone = $"099789{phoneSuffix:D4}",
            CategorieAgentId = createdCategorie!.IdCategorieAgent,
            Statut = true
        };

        var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
        agentResponse.EnsureSuccessStatusCode();
        var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
        Assert.NotNull(createdAgent);
        return createdAgent;
    }

    private async Task<(int AffilieId, int AdhesionId)> SeedAffilieWithAdhesionAsync(
        int agentId,
        string suffix,
        string statutDossier)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
        var utilisateurId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

        var affilie = new Affilie
        {
            CodeAdhesion = $"DAA-{suffix}",
            Nom = "Test",
            Prenom = "Dash",
            Postnom = "AA",
            NomComplet = "Test Dash AA",
            DateNaissance = new DateTime(1990, 5, 1),
            Telephone = $"081{suffix.PadLeft(7, '0')}"[..10],
            ProvinceResidence = "Kin",
            Statut = true,
            DateCreation = DateTime.Now
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var adhesion = new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agentId,
            TypeAdhesionId = typeAdhesionId,
            UtilisateurId = utilisateurId,
            StatutDossier = statutDossier,
            Statut = true,
            DateCreation = DateTime.Now
        };
        db.Adhesions.Add(adhesion);
        await db.SaveChangesAsync();

        return (affilie.IdAffilie, adhesion.IdAdhesion);
    }
}
