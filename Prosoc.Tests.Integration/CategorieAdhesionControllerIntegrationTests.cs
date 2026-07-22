using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Integration;

public class CategorieAdhesionControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategorieAdhesionControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Crud_Works_EndToEnd()
    {
        var create = new CategorieAdhesionCreateDto
        {
            Libelle = "Cat test",
            Description = "Desc test",
            Statut = true
        };

        var createRes = await _client.PostAsJsonAsync("/api/CategorieAdhesion", create);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);

        var created = await createRes.Content.ReadFromJsonAsync<CategorieAdhesionReadDto>();
        Assert.NotNull(created);
        Assert.True(created!.IdCategorieAdhesion > 0);
        Assert.Equal(create.Libelle, created.Libelle);
        Assert.Equal(create.Description, created.Description);
        Assert.True(created.Statut);

        var getRes = await _client.GetAsync($"/api/CategorieAdhesion/{created.IdCategorieAdhesion}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var fetched = await getRes.Content.ReadFromJsonAsync<CategorieAdhesionReadDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.IdCategorieAdhesion, fetched!.IdCategorieAdhesion);

        var update = new CategorieAdhesionUpdateDto
        {
            Libelle = "Cat test 2",
            Description = "Desc test 2",
            Statut = false
        };

        var updateRes = await _client.PutAsJsonAsync($"/api/CategorieAdhesion/{created.IdCategorieAdhesion}", update);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        var updated = await updateRes.Content.ReadFromJsonAsync<CategorieAdhesionReadDto>();
        Assert.NotNull(updated);
        Assert.Equal(update.Libelle, updated!.Libelle);
        Assert.Equal(update.Description, updated.Description);
        Assert.False(updated.Statut);

        var activesRes = await _client.GetAsync("/api/CategorieAdhesion/actives");
        activesRes.EnsureSuccessStatusCode();
        var actives = await activesRes.Content.ReadFromJsonAsync<List<CategorieAdhesionReadDto>>();
        Assert.NotNull(actives);
        Assert.DoesNotContain(actives!, a => a.IdCategorieAdhesion == created.IdCategorieAdhesion);

        var delRes = await _client.DeleteAsync($"/api/CategorieAdhesion/{created.IdCategorieAdhesion}");
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        var getAfterDel = await _client.GetAsync($"/api/CategorieAdhesion/{created.IdCategorieAdhesion}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDel.StatusCode);
    }
}
