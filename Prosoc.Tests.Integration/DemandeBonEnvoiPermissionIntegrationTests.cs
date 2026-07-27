using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration;

public class DemandeBonEnvoiPermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DemandeBonEnvoiPermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ValiderEtGenerer_WithoutConfirmPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Percepteur" };
            TestAuthHandler.Permissions = new[] { "READ_DEMANDE_BON_ENVOI" };

            var response = await _client.PostAsJsonAsync("/api/DemandeBonEnvoi/valider-et-generer", new
            {
                idDemande = 1,
                agentId = 1,
                hopitalPartenaireId = 1
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("CONFIRM_DEMANDE_BON_ENVOI", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Confirmer_WithoutConfirmPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "READ_DEMANDE_BON_ENVOI" };

            var response = await _client.PostAsJsonAsync("/api/DemandeBonEnvoi/1/confirmer", new
            {
                decision = "VALIDEE",
                motifRejet = (string?)null
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("CONFIRM_DEMANDE_BON_ENVOI", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
