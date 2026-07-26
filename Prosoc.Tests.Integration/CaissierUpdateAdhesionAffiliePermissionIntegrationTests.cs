using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class CaissierUpdateAdhesionAffiliePermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CaissierUpdateAdhesionAffiliePermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static AffilieUpdateDto MinimalAffilieUpdateDto() => new()
    {
        CodeAdhesion = "CAISSIER-TEST",
        Nom = "Test",
        Prenom = "Caissier",
        DateNaissance = new DateTime(1990, 1, 1)
    };

    private static AdhesionUpdateWithAffilieDto MinimalAdhesionUpdateDto() => new()
    {
        Affilie = new AffilieForUpdateDto
        {
            Nom = "Test",
            Prenom = "Caissier",
            DateNaissance = new DateTime(1990, 1, 1)
        },
        Adhesion = new AdhesionForUpdateDto
        {
            StatutDossier = "EN ATTENTE"
        },
        Souscriptions = new List<SouscriptionPrestationForUpdateDto>(),
        Dependents = new List<DependantForUpdateDto>()
    };

    [Fact]
    public async Task UpdateAffilie_AsCaissierWithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync("/api/Affilie/1", MinimalAffilieUpdateDto());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("UPDATE_AFFILIE", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateAffilie_AsCaissierWithPermission_DoesNotReturnForbiddenForMissingPermission()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "UPDATE_AFFILIE" };

            var response = await _client.PutAsJsonAsync("/api/Affilie/999999999", MinimalAffilieUpdateDto());

            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateAdhesion_AsCaissierWithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync(
                "/api/Adhesion/UpdateWithAffilieAsync/1",
                MinimalAdhesionUpdateDto());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("UPDATE_ADHESION", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateAdhesion_AsCaissierWithPermission_DoesNotReturnForbiddenForMissingPermission()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "UPDATE_ADHESION" };

            var response = await _client.PutAsJsonAsync(
                "/api/Adhesion/UpdateWithAffilieAsync/999999999",
                MinimalAdhesionUpdateDto());

            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
