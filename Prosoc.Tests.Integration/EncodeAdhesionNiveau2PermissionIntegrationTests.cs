using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class EncodeAdhesionNiveau2PermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EncodeAdhesionNiveau2PermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static AdhesionNiveau2EncodeurDto MinimalDto() => new()
    {
        Valider = false,
        Dependants = new List<DependantNiveau2Dto>(),
        PersonneContact = new PersonneContactNiveau2Dto
        {
            NomComplet = "Contact Test",
            LienParente = "Autre",
            Adresse = "Adresse test"
        }
    };

    [Fact]
    public async Task Niveau2Encodeur_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Agent (AA)" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync(
                "/api/Adhesion/1/niveau-2-encodeur",
                MinimalDto());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("ENCODE_ADHESION_NIVEAU_2", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Niveau2Encodeur_WithPermission_DoesNotReturnForbiddenForMissingPermission()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Agent (AA)" };
            TestAuthHandler.Permissions = new[] { "ENCODE_ADHESION_NIVEAU_2" };

            var response = await _client.PutAsJsonAsync(
                "/api/Adhesion/999999/niveau-2-encodeur",
                MinimalDto());

            // Gate permission passé : pas 403 pour ENCODE_ADHESION_NIVEAU_2
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
