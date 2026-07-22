using Microsoft.EntityFrameworkCore;
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
    public class AgentAffecterAffiliesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private static int _phoneSequence;
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AgentAffecterAffiliesIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<AgentReadDto> CreateAgentAsync()
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = $"F{unique[..6]}",
                Description = "Catégorie test affectation affiliés"
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            categorieResponse.EnsureSuccessStatusCode();
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
            var agentDto = new AgentCreateDto
            {
                NomComplet = $"Agent Affilies {unique}",
                Matricule = $"AI{unique.PadRight(9, '0')}"[..11],
                Phone = $"099456{phoneSuffix:D4}",
                CategorieAgentId = createdCategorie!.IdCategorieAgent,
                Statut = true
            };

            var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
            agentResponse.EnsureSuccessStatusCode();
            var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(createdAgent);
            return createdAgent;
        }

        private async Task<(int AffilieId, int AdhesionId)> SeedAffilieWithAdhesionAsync(int agentId, string suffix)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var typeAdhesionId = await db.TypeAdhesions
                .Select(t => t.IdTypeAdhesion)
                .FirstAsync();

            var deviseId = await db.Devises
                .Select(d => d.IdDevise)
                .FirstAsync();

            var utilisateurId = await db.Utilisateurs
                .Select(u => u.IdUtilisateur)
                .FirstAsync();

            var affilie = new Affilie
            {
                CodeAdhesion = $"COD-{suffix}",
                Nom = "Test",
                Prenom = "Affilie",
                Postnom = "Affect",
                NomComplet = "Test Affilie Affect",
                DateNaissance = new DateTime(1990, 1, 1),
                Telephone = $"080{suffix.PadLeft(7, '0')}"[..10],
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
                StatutDossier = "A",
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Adhesions.Add(adhesion);
            await db.SaveChangesAsync();

            db.Collectes.Add(new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agentId,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 100m,
                DeviseId = deviseId,
                Mois = DateTime.Now.Month,
                Annee = DateTime.Now.Year,
                ReferencePaiement = $"REF-AFF-{suffix}",
                Statut = true,
                DateCreation = DateTime.Now,
                DateCollecte = DateTime.Now
            });
            await db.SaveChangesAsync();

            return (affilie.IdAffilie, adhesion.IdAdhesion);
        }

        private async Task<int> SeedAffilieWithoutAdhesionAsync(string suffix)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var affilie = new Affilie
            {
                CodeAdhesion = $"ORP-{suffix}",
                Nom = "Orphelin",
                Prenom = "Sans",
                Postnom = "Adhesion",
                NomComplet = "Orphelin Sans Adhesion",
                DateNaissance = new DateTime(1985, 5, 5),
                Telephone = $"081{suffix.PadLeft(7, '0')}"[..10],
                ProvinceResidence = "Kin",
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            return affilie.IdAffilie;
        }

        [Fact]
        public async Task AffecterAffilies_TransfertVersAutreAgent_MetAJourAdhesionEtCollectes()
        {
            var agentA = await CreateAgentAsync();
            var agentB = await CreateAgentAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var (affilieId, adhesionId) = await SeedAffilieWithAdhesionAsync(agentA.Id, suffix);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agentB.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto { AffilieIds = new List<int> { affilieId } });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentAffecterAffiliesResultDto>();
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalReussites);
            Assert.Equal(0, result.TotalEchecs);
            Assert.Equal(agentA.Id, result.Resultats[0].AncienAgentId);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var adhesion = await db.Adhesions.FindAsync(adhesionId);
            Assert.NotNull(adhesion);
            Assert.Equal(agentB.Id, adhesion.AgentId);

            var collecteAgentId = await db.Collectes
                .Where(c => c.AffilieId == affilieId)
                .Select(c => c.AgentId)
                .FirstAsync();
            Assert.Equal(agentB.Id, collecteAgentId);
        }

        [Fact]
        public async Task AffecterAffilies_SansAdhesion_RetourneBadRequest()
        {
            var agent = await CreateAgentAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var affilieId = await SeedAffilieWithoutAdhesionAsync(suffix);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto { AffilieIds = new List<int> { affilieId } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentAffecterAffiliesResultDto>();
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalReussites);
            Assert.Equal(1, result.TotalEchecs);
            Assert.Contains("adhésion", result.Resultats[0].Message!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AffecterAffilies_DejaAffecteMemeAgent_EstIdempotent()
        {
            var agent = await CreateAgentAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var (affilieId, _) = await SeedAffilieWithAdhesionAsync(agent.Id, suffix);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto { AffilieIds = new List<int> { affilieId } });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentAffecterAffiliesResultDto>();
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalReussites);
            Assert.True(result.Resultats[0].Succes);
        }

        [Fact]
        public async Task AffecterAffilies_AgentInconnu_RetourneNotFound()
        {
            var response = await _client.PutAsJsonAsync(
                "/api/Agent/999999999/affecter-affilies",
                new AgentAffecterAffiliesDto { AffilieIds = new List<int> { 1 } });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AffecterAffilies_ListeVide_RetourneBadRequest()
        {
            var agent = await CreateAgentAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto { AffilieIds = new List<int>() });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AffecterAffilies_TransfertMassif_SourceVersCible_TransfereTousLesAffiliesActifs()
        {
            var agentA = await CreateAgentAsync();
            var agentB = await CreateAgentAsync();
            var suffix1 = Guid.NewGuid().ToString("N")[..8];
            var suffix2 = Guid.NewGuid().ToString("N")[..8];
            var (affilieId1, adhesionId1) = await SeedAffilieWithAdhesionAsync(agentA.Id, suffix1);
            var (affilieId2, adhesionId2) = await SeedAffilieWithAdhesionAsync(agentA.Id, suffix2);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agentB.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto
                {
                    SourceAgentId = agentA.Id,
                    AffilieIds = new List<int>()
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentAffecterAffiliesResultDto>();
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalDemandes);
            Assert.Equal(2, result.TotalReussites);
            Assert.Equal(0, result.TotalEchecs);
            Assert.All(result.Resultats, r => Assert.Equal(agentA.Id, r.AncienAgentId));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var adhesion1 = await db.Adhesions.FindAsync(adhesionId1);
            var adhesion2 = await db.Adhesions.FindAsync(adhesionId2);
            Assert.NotNull(adhesion1);
            Assert.NotNull(adhesion2);
            Assert.Equal(agentB.Id, adhesion1.AgentId);
            Assert.Equal(agentB.Id, adhesion2.AgentId);

            var collecteAgentIds = await db.Collectes
                .Where(c => c.AffilieId == affilieId1 || c.AffilieId == affilieId2)
                .Select(c => c.AgentId)
                .Distinct()
                .ToListAsync();
            Assert.Single(collecteAgentIds);
            Assert.Equal(agentB.Id, collecteAgentIds[0]);
        }

        [Fact]
        public async Task AffecterAffilies_SourceEgaleCible_RetourneBadRequest()
        {
            var agent = await CreateAgentAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agent.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto
                {
                    SourceAgentId = agent.Id,
                    AffilieIds = new List<int>()
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AffecterAffilies_SourceSansAffilies_RetourneBadRequest()
        {
            var agentSource = await CreateAgentAsync();
            var agentCible = await CreateAgentAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agentCible.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto
                {
                    SourceAgentId = agentSource.Id,
                    AffilieIds = new List<int>()
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentAffecterAffiliesResultDto>();
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalReussites);
            Assert.Equal(0, result.TotalDemandes);
        }

        [Fact]
        public async Task AffecterAffilies_AvecSourceEtAffilieHorsSource_EchouePourCetAffilie()
        {
            var agentSource = await CreateAgentAsync();
            var agentAutre = await CreateAgentAsync();
            var agentCible = await CreateAgentAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var (affilieId, _) = await SeedAffilieWithAdhesionAsync(agentAutre.Id, suffix);

            var response = await _client.PutAsJsonAsync(
                $"/api/Agent/{agentCible.Id}/affecter-affilies",
                new AgentAffecterAffiliesDto
                {
                    SourceAgentId = agentSource.Id,
                    AffilieIds = new List<int> { affilieId }
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AgentAffecterAffiliesResultDto>();
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalReussites);
            Assert.Equal(1, result.TotalEchecs);
            Assert.Contains("agent source", result.Resultats[0].Message!, StringComparison.OrdinalIgnoreCase);
        }
    }
}
