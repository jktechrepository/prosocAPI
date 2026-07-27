using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class PrestationWriteClosedIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PrestationWriteClosedIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "IT" };
            TestAuthHandler.Permissions = new[] { "CREATE_PRESTATION", "UPDATE_PRESTATION", "READ_PRESTATION" };

            var response = await _client.PostAsJsonAsync("/api/Prestation", new PrestationCreateDto
            {
                NomPrestation = "Test fermé",
                Montant = 10,
                DeviseId = 1
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Update_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Admin" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync("/api/Prestation/1", new PrestationUpdateDto
            {
                NomPrestation = "Test fermé",
                Montant = 10,
                DeviseId = 1
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Delete_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Admin" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.DeleteAsync("/api/Prestation/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
