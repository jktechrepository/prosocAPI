using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Prosoc.Models.DTOs.CategorieAgent;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration
{
    public class AgentAffecterZoneSocialeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private static int _phoneSequence;
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AgentAffecterZoneSocialeIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<int> CreateZoneSocialeAsync(string suffix = "")
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = $"P-Affect-{suffix}", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = $"C-Affect-{suffix}", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = $"Z-Affect-{suffix}", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            return zone.IdZoneSociale;
        }

        private async Task<int> CreateInactiveZoneSocialeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Affect-Inactive", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Affect-Inactive", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Affect-Inactive", CommuneId = commune.IdCommune, Statut = false };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            return zone.IdZoneSociale;
        }

        private async Task<AgentReadDto> CreateAgentAsync(int? zoneSocialeId = null)
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = $"Z{unique[..6]}",
                Description = "Catégorie test affectation zone"
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            categorieResponse.EnsureSuccessStatusCode();
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
            var agentDto = new AgentCreateDto
            {
                NomComplet = $"Agent Affect {unique}",
                Matricule = $"AF{unique.PadRight(9, '0')}"[..11],
                Phone = $"099123{phoneSuffix:D4}",
                ZoneSocialeId = zoneSocialeId,
                CategorieAgentId = createdCategorie!.IdCategorieAgent,
                Statut = true
            };

            var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
            if (!agentResponse.IsSuccessStatusCode)
            {
                var body = await agentResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Agent create failed {(int)agentResponse.StatusCode}: {body}");
            }

            var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(createdAgent);
            return createdAgent;
        }

        [Fact]
        public async Task AffecterZoneSociale_WithValidZone_ReturnsOkAndUpdatesAgent()
        {
            var zoneId = await CreateZoneSocialeAsync("valid");
            var agent = await CreateAgentAsync(zoneSocialeId: null);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-zone-sociale",
                new AgentAffecterZoneSocialeDto { ZoneSocialeId = zoneId });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(result);
            Assert.Equal(zoneId, result.ZoneSocialeId);
            Assert.Equal("Z-Affect-valid", result.ZoneSocialeNom);
        }

        [Fact]
        public async Task AffecterZoneSociale_WithNull_DesaffecteZone()
        {
            var zoneId = await CreateZoneSocialeAsync("desaffect");
            var agent = await CreateAgentAsync(zoneSocialeId: zoneId);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-zone-sociale",
                new AgentAffecterZoneSocialeDto { ZoneSocialeId = null });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(result);
            Assert.Null(result.ZoneSocialeId);
            Assert.Null(result.ZoneSocialeNom);
        }

        [Fact]
        public async Task AffecterZoneSociale_WithUnknownZone_ReturnsNotFound()
        {
            var agent = await CreateAgentAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-zone-sociale",
                new AgentAffecterZoneSocialeDto { ZoneSocialeId = 999_999_999 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AffecterZoneSociale_WithInactiveZone_ReturnsBadRequest()
        {
            var inactiveZoneId = await CreateInactiveZoneSocialeAsync();
            var agent = await CreateAgentAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-zone-sociale",
                new AgentAffecterZoneSocialeDto { ZoneSocialeId = inactiveZoneId });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AffecterZoneSociale_WithUnknownAgent_ReturnsNotFound()
        {
            var zoneId = await CreateZoneSocialeAsync("noagent");

            var response = await _client.PutAsJsonAsync(
                "/api/Agent/999999999/affecter-zone-sociale",
                new AgentAffecterZoneSocialeDto { ZoneSocialeId = zoneId });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
