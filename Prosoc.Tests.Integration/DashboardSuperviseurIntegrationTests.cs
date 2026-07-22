using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

/// <summary>
/// Superviseur sans commune titulaire → 422 (pas 500).
/// </summary>
public class DashboardSuperviseurIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DashboardSuperviseurIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IndicateursPerformance_SansCommuneTitulaire_Retourne422()
    {
        var superviseurId = await SeedSuperviseurSansCommuneTitulaireAsync();

        TestAuthHandler.Roles = new[] { "Admin" };

        var response = await _client.GetAsync(
            $"/api/DashboardSuperviseur/indicateurs-performance/{superviseurId}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SuperviseurScopeErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("BUSINESS_SUPERVISEUR_SANS_COMMUNE_TITULAIRE", body!.CodeErreur);
        Assert.Equal(superviseurId, body.SuperviseurAgentId);
        Assert.Contains("non titulaire d'une commune", body.Message, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<int> SeedSuperviseurSansCommuneTitulaireAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var province = new Province { Nom = "Prov SP Test", Statut = true };
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var commune = new Commune { Nom = "Commune SP Test", ProvinceId = province.IdProvince, Statut = true };
        db.Communes.Add(commune);
        await db.SaveChangesAsync();

        var zone = new ZoneSociale
        {
            Nom = "Zone SP Test",
            CommuneId = commune.IdCommune,
            Statut = true
        };
        db.ZonesSociales.Add(zone);
        await db.SaveChangesAsync();

        var superviseur = new Agent
        {
            NomComplet = "Superviseur Sans Commune",
            Matricule = $"SP{Guid.NewGuid():N}"[..10],
            Phone = $"099{Guid.NewGuid():N}"[..7],
            ZoneSocialeId = zone.IdZoneSociale,
            Statut = true
        };
        db.Agents.Add(superviseur);
        await db.SaveChangesAsync();

        return superviseur.IdAgent;
    }

    private sealed class SuperviseurScopeErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string CodeErreur { get; set; } = string.Empty;
        public int SuperviseurAgentId { get; set; }
    }
}
