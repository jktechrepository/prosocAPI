using System.Net;
using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class DeviseCreatePermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeviseCreatePermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDevise_AsFinancierWithPermission_ReturnsCreated()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "CREATE_DEVISE" };

            var unique = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var response = await _client.PostAsJsonAsync("/api/Devise", new DeviseCreateDto
            {
                Code = $"T{unique}",
                Nom = $"Devise test {unique}",
                Symbole = "T",
                EstDevisePrincipale = false,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task CreateDevise_AsFinancierWithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync("/api/Devise", new DeviseCreateDto
            {
                Code = "ZZZ",
                Nom = "Sans permission",
                Statut = true
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("CREATE_DEVISE", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task CreateTauxChange_AsFinancierWithPermission_ReturnsCreated()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "CREATE_TAUX_CHANGE" };

            var response = await _client.PostAsJsonAsync("/api/Devise/taux-change", new TauxChangeDeviseCreateDto
            {
                CodeDeviseSource = "USD",
                CodeDeviseCible = "CDF",
                Taux = 2900.5m,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task CreateTauxChange_AsFinancierWithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync("/api/Devise/taux-change", new TauxChangeDeviseCreateDto
            {
                CodeDeviseSource = "USD",
                CodeDeviseCible = "CDF",
                Taux = 2800m,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("CREATE_TAUX_CHANGE", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
