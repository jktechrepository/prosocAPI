using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using Prosoc.Models.DTOs.CategorieAgent;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class AgentEmailAgentUniqueIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AgentEmailAgentUniqueIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<int> CreateZoneSocialeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var province = new ProsocAPI.Models.Core.Province { Nom = "P-EmailAgent-Test", Statut = true };
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var commune = new ProsocAPI.Models.Core.Commune { Nom = "C-EmailAgent-Test", ProvinceId = province.IdProvince, Statut = true };
        db.Communes.Add(commune);
        await db.SaveChangesAsync();

        var zone = new ProsocAPI.Models.Core.ZoneSociale { Nom = "Z-EmailAgent-Test", CommuneId = commune.IdCommune, Statut = true };
        db.ZonesSociales.Add(zone);
        await db.SaveChangesAsync();

        return zone.IdZoneSociale;
    }

    private async Task<int> CreateCategorieAgentAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..2].ToUpperInvariant();
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = unique,
            Description = "Email agent test"
        };

        var res = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        res.EnsureSuccessStatusCode();

        var created = await res.Content.ReadFromJsonAsync<CategorieAgentDto>();
        Assert.NotNull(created);
        return created!.IdCategorieAgent;
    }

    [Fact]
    public async Task CreateAgent_EmailAgent_IsUnique_CaseInsensitive_Trimmed()
    {
        var zoneSocialeId = await CreateZoneSocialeAsync();
        var categorieId = await CreateCategorieAgentAsync();

        var a = new AgentCreateDto
        {
            NomComplet = "Agent Email A",
            Matricule = null,
            Phone = "0991234501",
            EmailAgent = "Test@Prosoc.cd",
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };

        var resA = await _client.PostAsJsonAsync("/api/Agent", a);
        resA.EnsureSuccessStatusCode();

        var b = new AgentCreateDto
        {
            NomComplet = "Agent Email B",
            Matricule = null,
            Phone = "0991234502",
            EmailAgent = " test@prosoc.cd ",
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };

        var resB = await _client.PostAsJsonAsync("/api/Agent", b);
        Assert.Equal(HttpStatusCode.BadRequest, resB.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_AllowsMultipleNullEmailAgent()
    {
        var zoneSocialeId = await CreateZoneSocialeAsync();
        var categorieId = await CreateCategorieAgentAsync();

        var a = new AgentCreateDto
        {
            NomComplet = "Agent Null Email A",
            Matricule = null,
            Phone = "0991234511",
            EmailAgent = null,
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };

        var b = new AgentCreateDto
        {
            NomComplet = "Agent Null Email B",
            Matricule = null,
            Phone = "0991234512",
            EmailAgent = "   ",
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };

        var resA = await _client.PostAsJsonAsync("/api/Agent", a);
        resA.EnsureSuccessStatusCode();

        var resB = await _client.PostAsJsonAsync("/api/Agent", b);
        resB.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateAgent_EmailAgent_CannotCollide_WithAnotherAgent()
    {
        var zoneSocialeId = await CreateZoneSocialeAsync();
        var categorieId = await CreateCategorieAgentAsync();

        var a = new AgentCreateDto
        {
            NomComplet = "Agent Update A",
            Matricule = null,
            Phone = "0991234521",
            EmailAgent = "a@prosoc.cd",
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };
        var resA = await _client.PostAsJsonAsync("/api/Agent", a);
        resA.EnsureSuccessStatusCode();
        var createdA = await resA.Content.ReadFromJsonAsync<AgentReadDto>();
        Assert.NotNull(createdA);

        var b = new AgentCreateDto
        {
            NomComplet = "Agent Update B",
            Matricule = null,
            Phone = "0991234522",
            EmailAgent = "b@prosoc.cd",
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };
        var resB = await _client.PostAsJsonAsync("/api/Agent", b);
        resB.EnsureSuccessStatusCode();

        var update = new AgentUpdateDto
        {
            NomComplet = "Agent Update A (edited)",
            Matricule = createdA!.Matricule,
            Phone = "0991234521",
            EmailAgent = " B@PROSOC.CD ",
            Fonction = null,
            RoleAgent = "Agent",
            PhotoUrl = null,
            ZoneSocialeId = zoneSocialeId,
            CategorieAgentId = categorieId,
            Statut = true
        };

        var resUpdate = await _client.PutAsJsonAsync($"/api/Agent/{createdA.Id}", update);
        Assert.Equal(HttpStatusCode.BadRequest, resUpdate.StatusCode);
    }
}

