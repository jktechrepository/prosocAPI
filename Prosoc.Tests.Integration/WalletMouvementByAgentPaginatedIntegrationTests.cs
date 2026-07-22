using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration;

public class WalletMouvementByAgentPaginatedIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private const int MovementCount = 15;
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WalletMouvementByAgentPaginatedIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByAgentPaginated_ReturnsPageWithCorrectTotals()
    {
        var agentId = await SeedAgentWithWalletMovementsAsync();

        var response = await _client.GetAsync(
            $"/api/WalletMouvement/by-agent/{agentId}/paginated?pageNumber=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<WalletMouvementReadDto>>();
        Assert.NotNull(result);
        Assert.Equal(10, result.Data.Count);
        Assert.Equal(MovementCount, result.TotalItems);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.All(result.Data, m => Assert.Equal(agentId, m.WalletAgentId));

        var first = result.Data[0];
        Assert.True(first.DeviseId > 0);
        Assert.False(string.IsNullOrWhiteSpace(first.DeviseCode));
        Assert.False(string.IsNullOrWhiteSpace(first.DeviseNom));
    }

    [Fact]
    public async Task GetByAgentPaginated_CommissionCollecte_RetourneDescriptionLisible()
    {
        var (agentId, affilieNom) = await SeedAgentWithCommissionMovementAsync();

        var response = await _client.GetAsync(
            $"/api/WalletMouvement/by-agent/{agentId}/paginated?pageNumber=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<WalletMouvementReadDto>>();
        Assert.NotNull(result);
        var commission = Assert.Single(result!.Data, m => m.Source == WalletMouvementSources.CommissionCollecte);
        Assert.Contains(affilieNom, commission.Description);
        Assert.DoesNotContain("Affilie", commission.Description, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<int> SeedAgentWithWalletMovementsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();
        var unique = Guid.NewGuid().ToString("N")[..8];

        var agent = new Agent
        {
            NomComplet = $"Agent WM {unique}",
            Matricule = $"WM{unique.PadRight(9, '0')}"[..11],
            Phone = $"099888{unique[..4]}",
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var wallet = new WalletAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = deviseId,
            SoldeCourant = 0,
            SoldeDisponible = 0,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.WalletsAgents.Add(wallet);
        await db.SaveChangesAsync();

        for (var i = 0; i < MovementCount; i++)
        {
            db.WalletMouvements.Add(new WalletMouvement
            {
                WalletId = wallet.IdWalletAgent,
                DeviseId = deviseId,
                Montant = 10m + i,
                TypeOperation = "CREDIT",
                Source = "COLLECTE",
                Description = $"Mouvement test {i}",
                DateOperation = DateTime.UtcNow.AddMinutes(-i),
                DateCreation = DateTime.UtcNow,
                Statut = true
            });
        }

        await db.SaveChangesAsync();
        return agent.IdAgent;
    }

    private async Task<(int AgentId, string AffilieNom)> SeedAgentWithCommissionMovementAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();
        var unique = Guid.NewGuid().ToString("N")[..8];

        var agent = new Agent
        {
            NomComplet = $"Agent WM {unique}",
            Matricule = $"WM{unique.PadRight(9, '0')}"[..11],
            Phone = $"099888{unique[..4]}",
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var wallet = new WalletAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = deviseId,
            SoldeCourant = 0,
            SoldeDisponible = 0,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.WalletsAgents.Add(wallet);
        await db.SaveChangesAsync();

        var affilieNom = "Jean Mukendi Test";
        var affilie = new Affilie
        {
            CodeAdhesion = $"AFF-WM-{unique}",
            Nom = "Jean",
            Prenom = "Mukendi",
            NomComplet = affilieNom,
            DateNaissance = new DateTime(1988, 3, 3),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            DeviseId = deviseId,
            Montant = 120m,
            TypeCollecte = TypeCollecte.Frais,
            Statut = true,
            DateCollecte = DateTime.UtcNow
        };
        db.Collectes.Add(collecte);
        await db.SaveChangesAsync();

        db.WalletMouvements.Add(new WalletMouvement
        {
            WalletId = wallet.IdWalletAgent,
            DeviseId = deviseId,
            Montant = 30m,
            TypeOperation = "CREDIT",
            Source = WalletMouvementSources.CommissionCollecte,
            Description = $"Commission collecte #{collecte.IdCollecte} - Affilie {affilie.IdAffilie}",
            DateOperation = DateTime.UtcNow,
            DateCreation = DateTime.UtcNow,
            Statut = true
        });

        await db.SaveChangesAsync();
        return (agent.IdAgent, affilieNom);
    }
}
