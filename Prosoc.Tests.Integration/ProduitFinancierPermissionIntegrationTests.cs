using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class ProduitFinancierPermissionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProduitFinancierPermissionIntegrationTests(CustomWebApplicationFactory factory)
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

    private async Task<int> EnsureAssureurIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var existing = await db.Assureurs.AsNoTracking().FirstOrDefaultAsync();
        if (existing != null)
            return existing.IdAssureur;

        var assureur = new Assureur
        {
            Nom = $"Assureur test {Guid.NewGuid():N}"[..20],
            Statut = true,
            DateCreation = DateTime.Now
        };
        db.Assureurs.Add(assureur);
        await db.SaveChangesAsync();
        return assureur.IdAssureur;
    }

    [Fact]
    public async Task CreateProduitMutuel_AsFinancierWithPermission_ReturnsCreated()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "CREATE_PRODUIT_MUTUEL" };

            var unique = Guid.NewGuid().ToString("N")[..8];
            var response = await _client.PostAsJsonAsync("/api/ProduitMutuel", new ProduitMutuelCreateDto
            {
                Nom = $"Mutuel FI {unique}",
                Montant = 25m,
                Periodicite = "Mensuel",
                AgeMin = 18,
                AgeMax = 65,
                DeviseId = await GetUsdDeviseIdAsync(),
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
    public async Task CreateProduitMutuel_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync("/api/ProduitMutuel", new ProduitMutuelCreateDto
            {
                Nom = "Sans perm",
                Montant = 10m,
                Periodicite = "Mensuel",
                DeviseId = await GetUsdDeviseIdAsync()
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("CREATE_PRODUIT_MUTUEL", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateProduitMutuel_AsFinancierWithPermission_ReturnsOk()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Admin" };
            TestAuthHandler.Permissions = Array.Empty<string>();
            var deviseId = await GetUsdDeviseIdAsync();
            var createResponse = await _client.PostAsJsonAsync("/api/ProduitMutuel", new ProduitMutuelCreateDto
            {
                Nom = $"Mutuel upd {Guid.NewGuid():N}"[..20],
                Montant = 15m,
                Periodicite = "Mensuel",
                AgeMin = 18,
                AgeMax = 60,
                DeviseId = deviseId,
                Statut = true
            });
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ProduitMutuelReadDto>();
            Assert.NotNull(created);

            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "UPDATE_PRODUIT_MUTUEL" };

            var updateResponse = await _client.PutAsJsonAsync($"/api/ProduitMutuel/{created!.Id}", new ProduitMutuelUpdateDto
            {
                Nom = created.Nom + " maj",
                Montant = 20m,
                Periodicite = "Mensuel",
                AgeMin = 18,
                AgeMax = 60,
                DeviseId = deviseId,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateProduitMutuel_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync("/api/ProduitMutuel/1", new ProduitMutuelUpdateDto
            {
                Nom = "X",
                Montant = 1m,
                Periodicite = "Mensuel",
                DeviseId = await GetUsdDeviseIdAsync()
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("UPDATE_PRODUIT_MUTUEL", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task CreateProduitAssureur_AsFinancierWithPermission_ReturnsCreated()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "CREATE_PRODUIT_ASSUREUR" };

            var unique = Guid.NewGuid().ToString("N")[..8];
            var response = await _client.PostAsJsonAsync("/api/ProduitAssureur", new ProduitAssureurCreateDto
            {
                Nom = $"Assureur FI {unique}",
                Montant = 30m,
                Periodicite = "Mensuel",
                AgeMin = 18,
                AgeMax = 65,
                AssureurId = await EnsureAssureurIdAsync(),
                DeviseId = await GetUsdDeviseIdAsync(),
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
    public async Task CreateProduitAssureur_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync("/api/ProduitAssureur", new ProduitAssureurCreateDto
            {
                Nom = "Sans perm",
                Montant = 10m,
                Periodicite = "Mensuel",
                AssureurId = await EnsureAssureurIdAsync(),
                DeviseId = await GetUsdDeviseIdAsync()
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("CREATE_PRODUIT_ASSUREUR", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateProduitAssureur_AsFinancierWithPermission_ReturnsOk()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Admin" };
            TestAuthHandler.Permissions = Array.Empty<string>();
            var deviseId = await GetUsdDeviseIdAsync();
            var assureurId = await EnsureAssureurIdAsync();
            var createResponse = await _client.PostAsJsonAsync("/api/ProduitAssureur", new ProduitAssureurCreateDto
            {
                Nom = $"PA upd {Guid.NewGuid():N}"[..20],
                Montant = 15m,
                Periodicite = "Mensuel",
                AgeMin = 18,
                AgeMax = 60,
                AssureurId = assureurId,
                DeviseId = deviseId,
                Statut = true
            });
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<ProduitAssureurReadDto>();
            Assert.NotNull(created);

            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = new[] { "UPDATE_PRODUIT_ASSUREUR" };

            var updateResponse = await _client.PutAsJsonAsync($"/api/ProduitAssureur/{created!.Id}", new ProduitAssureurUpdateDto
            {
                Nom = created.Nom + " maj",
                Montant = 22m,
                Periodicite = "Mensuel",
                AgeMin = 18,
                AgeMax = 60,
                AssureurId = assureurId,
                DeviseId = deviseId,
                Statut = true
            });

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task UpdateProduitAssureur_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Financier" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PutAsJsonAsync("/api/ProduitAssureur/1", new ProduitAssureurUpdateDto
            {
                Nom = "X",
                Montant = 1m,
                Periodicite = "Mensuel",
                AssureurId = await EnsureAssureurIdAsync(),
                DeviseId = await GetUsdDeviseIdAsync()
            });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("UPDATE_PRODUIT_ASSUREUR", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }
}
