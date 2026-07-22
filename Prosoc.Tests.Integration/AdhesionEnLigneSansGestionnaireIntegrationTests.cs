using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using Prosoc.Models.DTOs.CategorieAgent;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration;

public class AdhesionEnLigneSansGestionnaireIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private static int _phoneSequence;
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdhesionEnLigneSansGestionnaireIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin" };
    }

    private async Task<(int AffilieId, int AdhesionId)> SeedAdhesionEnLigneSansAgentAsync(string suffix)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();
        var utilisateurId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

        var affilie = new Affilie
        {
            CodeAdhesion = $"ONL-{suffix}",
            Nom = "EnLigne",
            Prenom = "Test",
            Postnom = "Affilie",
            NomComplet = "EnLigne Test Affilie",
            DateNaissance = new DateTime(1992, 3, 3),
            Telephone = $"085{suffix.PadLeft(7, '0')}"[..10],
            EmailAffilie = $"enligne.{suffix}@test.cd",
            ProvinceResidence = "Kinshasa",
            Statut = true,
            DateCreation = DateTime.Now
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var adhesion = new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            AgentId = null,
            TypeAdhesionId = typeAdhesionId,
            UtilisateurId = utilisateurId,
            StatutDossier = "EN ATTENTE",
            Statut = true,
            DateCreation = DateTime.Now
        };
        db.Adhesions.Add(adhesion);
        await db.SaveChangesAsync();

        db.Collectes.Add(new Collecte
        {
            AffilieId = affilie.IdAffilie,
            AgentId = null,
            TypeCollecte = TypeCollecte.Cotisation,
            Montant = 10m,
            DeviseId = deviseId,
            Mois = DateTime.Now.Month,
            Annee = DateTime.Now.Year,
            ReferencePaiement = $"REF-ONL-{suffix}",
            ModePaiement = "MOBILE_MONEY",
            Statut = true,
            DateCreation = DateTime.Now,
            DateCollecte = DateTime.Now
        });
        await db.SaveChangesAsync();

        return (affilie.IdAffilie, adhesion.IdAdhesion);
    }

    private async Task<AgentReadDto> CreateAgentAtAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = $"OL{unique[..6]}",
            Description = "Catégorie AT test en ligne"
        };

        var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        categorieResponse.EnsureSuccessStatusCode();
        var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
        Assert.NotNull(createdCategorie);

        var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
        var agentDto = new AgentCreateDto
        {
            NomComplet = $"Agent AT {unique}",
            Matricule = null,
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

    [Fact]
    public async Task GetEnLigneSansGestionnaire_RetourneAdhesionsAgentIdNull()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (affilieId, _) = await SeedAdhesionEnLigneSansAgentAsync(suffix);

        var response = await _client.GetAsync("/api/Adhesion/en-ligne-sans-gestionnaire?page=1&pageSize=50");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<AdhesionEnLigneSansGestionnaireDto>>();
        Assert.NotNull(result);
        Assert.Contains(result!.Data, d => d.IdAffilie == affilieId);
        var item = result.Data.First(d => d.IdAffilie == affilieId);
        Assert.Equal("MOBILE_MONEY", item.ModePaiementAdhesion);
    }

    [Fact]
    public async Task AffecterAffilies_ApresAffectation_DisparaitDeLaListeEnLigne()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (affilieId, adhesionId) = await SeedAdhesionEnLigneSansAgentAsync(suffix);
        var agent = await CreateAgentAtAsync();

        var affectResponse = await _client.PutAsJsonAsync(
            $"/api/Agent/{agent.Id}/affecter-affilies",
            new AgentAffecterAffiliesDto { AffilieIds = new List<int> { affilieId } });
        affectResponse.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var adhesion = await db.Adhesions.FindAsync(adhesionId);
            Assert.NotNull(adhesion);
            Assert.Equal(agent.Id, adhesion!.AgentId);
        }

        var listResponse = await _client.GetAsync("/api/Adhesion/en-ligne-sans-gestionnaire?page=1&pageSize=100");
        listResponse.EnsureSuccessStatusCode();
        var list = await listResponse.Content.ReadFromJsonAsync<PaginatedResponse<AdhesionEnLigneSansGestionnaireDto>>();
        Assert.NotNull(list);
        Assert.DoesNotContain(list!.Data, d => d.IdAffilie == affilieId);
    }

    [Fact]
    public async Task WithAffilie_SansAgentId_RetourneBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/Adhesion/with-affilie",
            new AdhesionWithAffilieCreateDto
            {
                Nom = "Sans",
                Prenom = "Agent",
                DateNaissance = new DateTime(1990, 1, 1),
                Telephone = "0811111111",
                ProvinceResidence = "Kinshasa",
                StatutDossier = "EN ATTENTE",
                TypeAdhesionId = 1,
                AgentId = null,
                Collectes = new List<CollecteAvecSouscriptionDto>()
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
