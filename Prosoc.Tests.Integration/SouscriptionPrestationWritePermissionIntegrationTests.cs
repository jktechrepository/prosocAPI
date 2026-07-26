using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class SouscriptionPrestationWritePermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SouscriptionPrestationWritePermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Update_AsFinancierWithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync("/api/SouscriptionPrestation/1", new SouscriptionPrestationUpdateDto
            {
                AffilieId = 1,
                PrestationId = 1,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("UPDATE_SOUSCRIPTION_PRESTATION", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Delete_AsFinancierWithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.DeleteAsync("/api/SouscriptionPrestation/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("DELETE_SOUSCRIPTION_PRESTATION", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Update_AsFinancierWithPermission_DoesNotReturnForbiddenForMissingPermission()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "UPDATE_SOUSCRIPTION_PRESTATION" };

            var response = await _client.PutAsJsonAsync("/api/SouscriptionPrestation/999999999", new SouscriptionPrestationUpdateDto
            {
                AffilieId = 1,
                PrestationId = 1,
                Statut = true
            });

            // Permission OK → NotFound (entité absente), pas 403
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Delete_AsFinancierWithPermission_DoesNotReturnForbiddenForMissingPermission()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "DELETE_SOUSCRIPTION_PRESTATION" };

            var response = await _client.DeleteAsync("/api/SouscriptionPrestation/999999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Update_AsCaissierWithoutWritePermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "READ_SOUSCRIPTION_PRESTATION" };

            var response = await _client.PutAsJsonAsync("/api/SouscriptionPrestation/1", new SouscriptionPrestationUpdateDto
            {
                AffilieId = 1,
                PrestationId = 1,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("UPDATE_SOUSCRIPTION_PRESTATION", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Delete_AsCaissierWithoutWritePermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Caissier" };
            TestAuthHandler.Permissions = new[] { "READ_SOUSCRIPTION_PRESTATION" };

            var response = await _client.DeleteAsync("/api/SouscriptionPrestation/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("DELETE_SOUSCRIPTION_PRESTATION", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
