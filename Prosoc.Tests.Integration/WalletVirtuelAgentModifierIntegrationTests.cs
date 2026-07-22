using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.DTOs.Core;
using Prosoc.Models.DTOs.CategorieAgent;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration
{
    public class WalletVirtuelAgentModifierIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private static int _phoneSequence;
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public WalletVirtuelAgentModifierIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<(int AgentId, int WalletVirtuelId)> CreateAgentWithWalletVirtuelAsync()
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = $"M{unique[..6]}",
                Description = "Catégorie test modifier wallet"
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            categorieResponse.EnsureSuccessStatusCode();
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
            var agentDto = new AgentCreateDto
            {
                NomComplet = $"Agent Mod {unique}",
                Matricule = $"WM{unique.PadRight(9, '0')}"[..11],
                Phone = $"099321{phoneSuffix:D4}",
                CategorieAgentId = createdCategorie!.IdCategorieAgent,
                Statut = true
            };

            var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
            agentResponse.EnsureSuccessStatusCode();
            var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(createdAgent);
            Assert.True(createdAgent.WalletVirtuelCree);
            Assert.NotNull(createdAgent.WalletVirtuelId);

            return (createdAgent.Id, createdAgent.WalletVirtuelId!.Value);
        }

        [Fact]
        public async Task ModifierWalletAgents_BatchValide_RemplaceSoldes()
        {
            var (agent1Id, _) = await CreateAgentWithWalletVirtuelAsync();
            var (agent2Id, _) = await CreateAgentWithWalletVirtuelAsync();

            var body = new List<WalletVirtuelAgentModifierItemDto>
            {
                new() { AgentId = agent1Id, SoldeVirtuel = 500m },
                new() { AgentId = agent2Id, SoldeVirtuel = 1200m }
            };

            var response = await _client.PutAsJsonAsync("/api/WalletVirtuelAgent/modifier-solde-wallet-agents", body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<WalletVirtuelAgentModifierResultDto>();
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalReussites);
            Assert.Equal(0, result.TotalEchecs);

            var solde1 = await _client.GetFromJsonAsync<decimal>($"/api/WalletVirtuelAgent/solde/{agent1Id}");
            var solde2 = await _client.GetFromJsonAsync<decimal>($"/api/WalletVirtuelAgent/solde/{agent2Id}");
            Assert.Equal(500m, solde1);
            Assert.Equal(1200m, solde2);
        }

        [Fact]
        public async Task ModifierWalletAgents_AgentSansWallet_RetourneBadRequest()
        {
            var response = await _client.PutAsJsonAsync(
                "/api/WalletVirtuelAgent/modifier-solde-wallet-agents",
                new List<WalletVirtuelAgentModifierItemDto>
                {
                    new() { AgentId = 999999999, SoldeVirtuel = 100m }
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<WalletVirtuelAgentModifierResultDto>();
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalReussites);
        }

        [Fact]
        public async Task ModifierWalletAgents_WalletInactif_RetourneBadRequest()
        {
            var (agentId, walletId) = await CreateAgentWithWalletVirtuelAsync();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
                var wallet = await db.WalletsVirtuelsAgents.FindAsync(walletId);
                Assert.NotNull(wallet);
                wallet!.Statut = false;
                await db.SaveChangesAsync();
            }

            var response = await _client.PutAsJsonAsync(
                "/api/WalletVirtuelAgent/modifier-solde-wallet-agents",
                new List<WalletVirtuelAgentModifierItemDto>
                {
                    new() { AgentId = agentId, SoldeVirtuel = 50m }
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifierWalletAgents_ListeVide_RetourneBadRequest()
        {
            var response = await _client.PutAsJsonAsync(
                "/api/WalletVirtuelAgent/modifier-solde-wallet-agents",
                new List<WalletVirtuelAgentModifierItemDto>());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ModifierWalletAgents_MemeSolde_EstIdempotent()
        {
            var (agentId, _) = await CreateAgentWithWalletVirtuelAsync();

            var response = await _client.PutAsJsonAsync(
                "/api/WalletVirtuelAgent/modifier-solde-wallet-agents",
                new List<WalletVirtuelAgentModifierItemDto>
                {
                    new() { AgentId = agentId, SoldeVirtuel = 0m }
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<WalletVirtuelAgentModifierResultDto>();
            Assert.NotNull(result);
            Assert.True(result.Resultats[0].Succes);
        }

        [Fact]
        public async Task UpdateById_LegacyEndpoint_FonctionneToujours()
        {
            var (agentId, walletId) = await CreateAgentWithWalletVirtuelAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/WalletVirtuelAgent/{walletId}",
                new WalletVirtuelAgentUpdateDto { SoldeVirtuel = 750m, Statut = true });

            response.EnsureSuccessStatusCode();
            var dto = await response.Content.ReadFromJsonAsync<WalletVirtuelAgentReadDto>();
            Assert.NotNull(dto);
            Assert.Equal(750m, dto.SoldeVirtuel);

            var solde = await _client.GetFromJsonAsync<decimal>($"/api/WalletVirtuelAgent/solde/{agentId}");
            Assert.Equal(750m, solde);
        }

        [Fact]
        public async Task ModifierWalletAgents_DoublonAgentId_DernierGagne()
        {
            var (agentId, _) = await CreateAgentWithWalletVirtuelAsync();

            var response = await _client.PutAsJsonAsync(
                "/api/WalletVirtuelAgent/modifier-solde-wallet-agents",
                new List<WalletVirtuelAgentModifierItemDto>
                {
                    new() { AgentId = agentId, SoldeVirtuel = 100m },
                    new() { AgentId = agentId, SoldeVirtuel = 300m }
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var solde = await _client.GetFromJsonAsync<decimal>($"/api/WalletVirtuelAgent/solde/{agentId}");
            Assert.Equal(300m, solde);
        }
    }
}
