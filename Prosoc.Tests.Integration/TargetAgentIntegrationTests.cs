using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class TargetAgentIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TargetAgentIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidRoleNom_ReturnsCreatedTarget()
    {
        var response = await _client.PostAsJsonAsync("/api/TargetAgent", new TargetAgentCreateDto
        {
            RoleNom = "Agent (AA)",
            LibelleTarget = "Objectif adhésions AA — mensuel",
            Periodicite = PeriodiciteTarget.Mensuelle,
            Nombre = 123,
            Statut = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TargetAgentReadDto>();
        Assert.NotNull(created);
        Assert.Equal("Agent (AA)", created.RoleNom);
        Assert.Equal(123, created.Nombre);
        Assert.Equal(PeriodiciteTarget.Mensuelle, created.Periodicite);
    }

    [Fact]
    public async Task Create_WithInvalidRoleNom_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/TargetAgent", new TargetAgentCreateDto
        {
            RoleNom = "Rôle Inexistant",
            LibelleTarget = "Test",
            Periodicite = PeriodiciteTarget.Journaliere,
            Nombre = 10,
            Statut = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateActivePeriodiciteForSameRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/TargetAgent", new TargetAgentCreateDto
        {
            RoleNom = "Agent (AT)",
            LibelleTarget = "Doublon journalier AT",
            Periodicite = PeriodiciteTarget.Journaliere,
            Nombre = 50,
            Statut = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByRole_ReturnsSeededTargetsForAgentAt()
    {
        var roleNom = Uri.EscapeDataString("Agent (AT)");
        var response = await _client.GetAsync($"/api/TargetAgent/by-role/{roleNom}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var targets = await response.Content.ReadFromJsonAsync<List<TargetAgentReadDto>>();
        Assert.NotNull(targets);
        Assert.Equal(3, targets.Count);
        Assert.All(targets, t => Assert.Equal("Agent (AT)", t.RoleNom));
        Assert.Contains(targets, t => t.Periodicite == PeriodiciteTarget.Mensuelle && t.Nombre == 100);
    }

    [Fact]
    public async Task Create_UsesNombreFromRequestBody_InsteadOfPeriodiciteDefault()
    {
        var response = await _client.PostAsJsonAsync("/api/TargetAgent", new TargetAgentCreateDto
        {
            RoleNom = "Agent (AA)",
            LibelleTarget = "Objectif manuel nombre",
            Periodicite = PeriodiciteTarget.Journaliere,
            Nombre = 77,
            Statut = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TargetAgentReadDto>();
        Assert.NotNull(created);
        Assert.Equal(77, created!.Nombre);
        Assert.Equal(PeriodiciteTarget.Journaliere, created.Periodicite);
    }
}
