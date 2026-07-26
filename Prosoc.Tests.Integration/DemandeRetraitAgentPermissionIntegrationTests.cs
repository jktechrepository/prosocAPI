using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration;

public class DemandeRetraitAgentPermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DemandeRetraitAgentPermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEnAttente_WithReadPermission_ReturnsOk()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

            var response = await _client.GetAsync("/api/RetraitAgent/en-attente");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task GetEnAttente_WithoutReadPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.GetAsync("/api/RetraitAgent/en-attente");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("READ_DEMANDE_RETRAIT_AGENT", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Create_WithoutCreatePermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

            var response = await _client.PostAsJsonAsync("/api/RetraitAgent", new
            {
                agentId = 1,
                montantDemande = 10m,
                typeRetrait = "PARTIEL",
                motifRetrait = "Test permission"
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("CREATE_DEMANDE_RETRAIT_AGENT", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task ValiderEtGenererJeton_WithoutValidatePermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

            var response = await _client.PostAsJsonAsync("/api/RetraitAgent/valider-et-generer-jeton", new
            {
                idDemande = 1,
                statutDemande = "VALIDEE",
                agentValidationId = 1
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("VALIDATE_DEMANDE_RETRAIT_AGENT", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
