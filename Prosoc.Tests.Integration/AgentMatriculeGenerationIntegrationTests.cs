using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Prosoc.Models.DTOs.CategorieAgent;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration
{
    public class AgentMatriculeGenerationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AgentMatriculeGenerationIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<int> CreateZoneSocialeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Agents-Test", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Agents-Test", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Agents-Test", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            return zone.IdZoneSociale;
        }

        [Fact]
        public async Task CreateAgent_WithoutMatricule_GeneratesMatriculeAutomatically()
        {
            var zoneSocialeId = await CreateZoneSocialeAsync();

            // Arrange - Créer une catégorie d'agent
            var unique = Guid.NewGuid().ToString("N")[..8];
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = "SU",
                Description = "Super test",
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            categorieResponse.EnsureSuccessStatusCode();
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            // Act - Créer un agent sans matricule mais avec CategorieAgentId
            var agentDto = new AgentCreateDto
            {
                NomComplet = "Agent Test Superviseur",
                Matricule = null, // Laisser vide pour génération automatique
                Phone = "0991234567",
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

            // Assert - Vérifier que le matricule a été généré
            Assert.NotNull(createdAgent.Matricule);
            Assert.Equal(11, createdAgent.Matricule.Length); // 2 caractères + 9 chiffres
            Assert.StartsWith("SU", createdAgent.Matricule.ToUpperInvariant()); // préfixe des 2 premières lettres du libellé
            
            // Vérifier que les 9 derniers caractères sont des chiffres
            var numericPart = createdAgent.Matricule.Substring(2);
            Assert.True(numericPart.All(char.IsDigit));

            Assert.Equal(createdCategorie.IdCategorieAgent, createdAgent.CategorieAgentId);
            Assert.Equal("SU", createdAgent.CategorieAgentCode);
            Assert.Equal("Super test", createdAgent.CategorieAgentDescription);

            var getResponse = await _client.GetAsync($"/api/Agent/{createdAgent.Id}");
            getResponse.EnsureSuccessStatusCode();
            var fetched = await getResponse.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(fetched);
            Assert.Equal(createdCategorie.IdCategorieAgent, fetched!.CategorieAgentId);
            Assert.Equal("SU", fetched.CategorieAgentCode);
            Assert.Equal("Super test", fetched.CategorieAgentDescription);
        }

        [Fact]
        public async Task CreateAgent_WithMatricule_UsesProvidedMatricule()
        {
            var zoneSocialeId = await CreateZoneSocialeAsync();

            // Arrange - Créer une catégorie d'agent
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = "AG",
                Description = "Agent test",
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            // Act - Créer un agent avec un matricule fourni
            var agentDto = new AgentCreateDto
            {
                NomComplet = "Agent Test Manuel",
                Matricule = "AG123456789", // Matricule fourni manuellement
                Phone = "0123456789",
                ZoneSocialeId = zoneSocialeId,
                CategorieAgentId = createdCategorie!.IdCategorieAgent,
                Statut = true
            };

            var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
            agentResponse.EnsureSuccessStatusCode();

            var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(createdAgent);

            // Assert - Vérifier que le matricule fourni a été conservé
            Assert.Equal("AG123456789", createdAgent.Matricule);
        }

        [Fact]
        public async Task CreateAgent_WithoutCategorieAgentId_ReturnsBadRequest()
        {
            var zoneSocialeId = await CreateZoneSocialeAsync();

            // Arrange & Act - Créer un agent sans CategorieAgentId
            var agentDto = new AgentCreateDto
            {
                NomComplet = "Agent Test Erreur",
                Matricule = null,
                Phone = "0123456789",
                ZoneSocialeId = zoneSocialeId,
                CategorieAgentId = null, // Manquant
                Statut = true
            };

            var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);

            // Assert - Doit retourner une erreur 400
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, agentResponse.StatusCode);
        }

        [Fact]
        public async Task CreateMultipleAgents_GeneratesUniqueMatricules()
        {
            var zoneSocialeId = await CreateZoneSocialeAsync();

            // Arrange - Créer une catégorie d'agent
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = "TE",
                Description = "Technicien test",
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            // Act - Créer plusieurs agents sans matricule
            var agents = new List<AgentCreateDto>();
            for (int i = 0; i < 3; i++)
            {
                agents.Add(new AgentCreateDto
                {
                    NomComplet = $"Technicien Test {i}",
                    Matricule = null,
                    Phone = $"012345678{i}",
                    ZoneSocialeId = zoneSocialeId,
                    CategorieAgentId = createdCategorie!.IdCategorieAgent,
                    Statut = true
                });
            }

            var createdAgents = new List<AgentReadDto>();
            foreach (var agentDto in agents)
            {
                var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
                agentResponse.EnsureSuccessStatusCode();
                var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
                createdAgents.Add(createdAgent!);
            }

            // Assert - Vérifier que tous les matricules sont uniques et valides
            var matricules = createdAgents.Select(a => a.Matricule).ToList();
            Assert.Equal(3, matricules.Count);
            Assert.Equal(3, matricules.Distinct().Count()); // Tous uniques

            foreach (var matricule in matricules)
            {
                Assert.NotNull(matricule);
                Assert.Equal(11, matricule.Length);
                Assert.StartsWith("TE", matricule); // "Technicien" -> "TE"
                
                var numericPart = matricule.Substring(2);
                Assert.True(numericPart.All(char.IsDigit));
            }
        }
    }
}
