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

public class WalletVirtuelMouvementByAgentPaginatedIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private const int MovementCount = 15;
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WalletVirtuelMouvementByAgentPaginatedIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMouvementsByAgentPaginated_ReturnsPageWithCorrectTotals()
    {
        var agentId = await SeedAgentWithWalletVirtuelMovementsAsync();

        var response = await _client.GetAsync(
            $"/api/WalletVirtuelAgent/by-agent/{agentId}/mouvements/paginated?pageNumber=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<WalletVirtuelMouvementReadDto>>();
        Assert.NotNull(result);
        Assert.Equal(10, result.Data.Count);
        Assert.Equal(MovementCount, result.TotalItems);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.All(result.Data, m =>
        {
            Assert.Equal(agentId, m.AgentId);
            Assert.False(string.IsNullOrEmpty(m.SourceLibelle));
        });
    }

    [Fact]
    public async Task GetMouvementsByAgentPaginated_FiltreCredit_RetourneUniquementCredits()
    {
        var agentId = await SeedAgentWithWalletVirtuelMovementsAsync();

        var response = await _client.GetAsync(
            $"/api/WalletVirtuelAgent/by-agent/{agentId}/mouvements/paginated?pageNumber=1&pageSize=50&typeOperation=CREDIT");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<WalletVirtuelMouvementReadDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Data);
        Assert.All(result.Data, m => Assert.Equal("CREDIT", m.TypeOperation));
    }

    private async Task<int> SeedAgentWithWalletVirtuelMovementsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var unique = Guid.NewGuid().ToString("N")[..8];

        var agent = new Agent
        {
            NomComplet = $"Agent WVM {unique}",
            Matricule = $"WV{unique.PadRight(9, '0')}"[..11],
            Phone = $"099777{unique[..4]}",
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var deviseId = await db.Devises
            .Where(d => d.EstDevisePrincipale && d.Statut)
            .Select(d => d.IdDevise)
            .FirstAsync();

        var wallet = new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = deviseId,
            SoldeVirtuel = 0,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();

        for (var i = 0; i < MovementCount; i++)
        {
            db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
            {
                WalletVirtuelId = wallet.IdWalletVirtuelAgent,
                Montant = 10m + i,
                TypeOperation = i % 2 == 0 ? "CREDIT" : "DEBIT",
                Source = WalletVirtuelMouvementSources.AjoutSolde,
                Description = $"Mouvement virtuel test {i}",
                DateOperation = DateTime.UtcNow.AddMinutes(-i),
                DateCreation = DateTime.UtcNow,
                Statut = true
            });
        }

        await db.SaveChangesAsync();
        return agent.IdAgent;
    }
}
