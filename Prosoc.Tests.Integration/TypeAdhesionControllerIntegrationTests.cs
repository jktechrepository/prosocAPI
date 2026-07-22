using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Integration;

public class TypeAdhesionControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TypeAdhesionControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithCategorieAdhesion_ThenGet_ReturnsSameCategorieAdhesionId()
    {
        var catCreate = new CategorieAdhesionCreateDto
        {
            Libelle = "Cat TA test",
            Description = "Cat pour TypeAdhesion",
            Statut = true
        };

        var catRes = await _client.PostAsJsonAsync("/api/CategorieAdhesion", catCreate);
        Assert.Equal(HttpStatusCode.Created, catRes.StatusCode);

        var cat = await catRes.Content.ReadFromJsonAsync<CategorieAdhesionReadDto>();
        Assert.NotNull(cat);
        Assert.True(cat!.IdCategorieAdhesion > 0);

        var taCreate = new TypeAdhesionCreateDto
        {
            Libelle = "TA Test",
            CategorieAdhesionId = cat.IdCategorieAdhesion,
            MaxDependants = 2,
            Description = "Type adhesion de test",
            Montant = 12345m,
            DeviseId = await GetDevisePrincipaleIdAsync(),
            Statut = true
        };

        var taRes = await _client.PostAsJsonAsync("/api/TypeAdhesion", taCreate);
        Assert.Equal(HttpStatusCode.Created, taRes.StatusCode);

        var created = await taRes.Content.ReadFromJsonAsync<TypeAdhesionReadDto>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(cat.IdCategorieAdhesion, created.CategorieAdhesionId);
        Assert.Equal(taCreate.DeviseId, created.DeviseId);

        var getRes = await _client.GetAsync($"/api/TypeAdhesion/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var fetched = await getRes.Content.ReadFromJsonAsync<TypeAdhesionReadDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(cat.IdCategorieAdhesion, fetched.CategorieAdhesionId);
        Assert.Equal(taCreate.DeviseId, fetched.DeviseId);
    }

    [Fact]
    public void TypeAdhesionCreateDto_SerializesWithDeviseId()
    {
        var payload = new TypeAdhesionCreateDto
        {
            Libelle = "TA Swagger Contract",
            CategorieAdhesionId = 1,
            MaxDependants = 0,
            Description = "contract test",
            Montant = 100m,
            DeviseId = 1,
            Statut = true
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Contains("\"deviseId\":1", json);
    }

    private async Task<int> GetDevisePrincipaleIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        return await db.Devises
            .Where(d => d.EstDevisePrincipale && d.Statut)
            .Select(d => d.IdDevise)
            .FirstAsync();
    }
}
