using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Controllers;
using Xunit;

namespace Prosoc.Tests.Integration;

public class FraisCreatePermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FraisCreatePermissionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> GetUsdDeviseIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var usd = await db.Devises.AsNoTracking().FirstAsync(d => d.Code == "USD");
        return usd.IdDevise;
    }

    [Fact]
    public async Task Create_AsFinancierWithCreateFrais_ReturnsCreated()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "CREATE_FRAIS" };

            var unique = Guid.NewGuid().ToString("N")[..8];
            var response = await _client.PostAsJsonAsync("/api/Frais", new CreateFraisDto
            {
                Code = $"TF{unique}",
                Libelle = $"Frais test {unique}",
                Montant = 10,
                DeviseId = await GetUsdDeviseIdAsync(),
                TauxCommission = 0
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
    public async Task Create_AsFinancierWithoutCreateFrais_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync("/api/Frais", new CreateFraisDto
            {
                Code = "NOPE",
                Libelle = "Sans permission",
                Montant = 5,
                DeviseId = await GetUsdDeviseIdAsync(),
                TauxCommission = 0
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("CREATE_FRAIS", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
