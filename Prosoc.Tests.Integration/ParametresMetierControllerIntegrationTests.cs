using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class ParametresMetierControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ParametresMetierControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRetraitAgent_AsAdmin_ReturnsConfig()
    {
        TestAuthHandler.Roles = new[] { "Admin" };

        await _client.PutAsJsonAsync("/api/parametres-metier/retrait-agent", new RetraitAgentParametresUpdateDto
        {
            Fenetre1Debut = 15,
            Fenetre1Fin = 20,
            Fenetre2DerniersJours = 7,
            MontantMinimumPartiel = 5
        });

        var response = await _client.GetAsync("/api/parametres-metier/retrait-agent");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RetraitAgentParametresReadDto>();

        Assert.NotNull(result);
        Assert.Equal(15, result.Fenetre1Debut);
        Assert.Equal(20, result.Fenetre1Fin);
        Assert.Equal(7, result.Fenetre2DerniersJours);
        Assert.Equal(5m, result.MontantMinimumPartiel);
    }

    [Fact]
    public async Task PutRetraitAgent_AsAdmin_UpdatesConfig()
    {
        TestAuthHandler.Roles = new[] { "Admin" };

        var update = new RetraitAgentParametresUpdateDto
        {
            Fenetre1Debut = 16,
            Fenetre1Fin = 21,
            Fenetre2DerniersJours = 6,
            MontantMinimumPartiel = 10
        };

        var putResponse = await _client.PutAsJsonAsync("/api/parametres-metier/retrait-agent", update);
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync("/api/parametres-metier/retrait-agent");
        var result = await getResponse.Content.ReadFromJsonAsync<RetraitAgentParametresReadDto>();

        Assert.NotNull(result);
        Assert.Equal(16, result.Fenetre1Debut);
        Assert.Equal(10m, result.MontantMinimumPartiel);
    }

    [Fact]
    public async Task GetRetraitAgent_AsAgentWithoutPermission_ReturnsForbidden()
    {
        TestAuthHandler.Roles = new[] { "Agent (AA)" };

        var response = await _client.GetAsync("/api/parametres-metier/retrait-agent");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutRetraitAgent_InvalidOverlap_ReturnsBadRequest()
    {
        TestAuthHandler.Roles = new[] { "Admin" };

        var update = new RetraitAgentParametresUpdateDto
        {
            Fenetre1Debut = 1,
            Fenetre1Fin = 28,
            Fenetre2DerniersJours = 15,
            MontantMinimumPartiel = 5
        };

        var response = await _client.PutAsJsonAsync("/api/parametres-metier/retrait-agent", update);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
