using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Integration;

public class TarifCotisationControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TarifCotisationControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByAffilie_AffilieInexistant_Retourne404()
    {
        var response = await _client.GetAsync("/api/TarifCotisation/Affilie?idAffilie=999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("introuvable", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByAffilie_IdInvalide_Retourne400()
    {
        var response = await _client.GetAsync("/api/TarifCotisation/Affilie?idAffilie=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithLibelleTarifCotisation_PersistsAndReturnsLibelle()
    {
        var (typeAdhesionId, deviseId) = await CreateTypeAdhesionForTarifAsync();
        var periodicite = $"Annuel";
        var payload = new TarifCotisationCreateDto
        {
            Montant = 99.5m,
            Periodicite = periodicite,
            TypeAdhesionId = typeAdhesionId,
            DeviseId = deviseId,
            LibelleTarifCotisation = "Tarif cotisation test integration",
            Statut = true
        };

        var post = await _client.PostAsJsonAsync("/api/TarifCotisation", payload);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<TarifCotisationReadDto>();
        Assert.NotNull(created);
        Assert.Equal(payload.LibelleTarifCotisation, created!.LibelleTarifCotisation);

        var get = await _client.GetAsync($"/api/TarifCotisation/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var fetched = await get.Content.ReadFromJsonAsync<TarifCotisationReadDto>();
        Assert.NotNull(fetched);
        Assert.Equal(payload.LibelleTarifCotisation, fetched!.LibelleTarifCotisation);
    }

    [Fact]
    public async Task Create_DuplicateActiveLibelleNormalized_ReturnsConflict()
    {
        var (typeAdhesionId, deviseId) = await CreateTypeAdhesionForTarifAsync();
        var payload1 = new TarifCotisationCreateDto
        {
            Montant = 10m,
            Periodicite = "Mensuel",
            TypeAdhesionId = typeAdhesionId,
            DeviseId = deviseId,
            LibelleTarifCotisation = "Tarif Premium",
            Statut = true
        };
        var payload2 = new TarifCotisationCreateDto
        {
            Montant = 20m,
            Periodicite = "Annuel",
            TypeAdhesionId = typeAdhesionId,
            DeviseId = deviseId,
            LibelleTarifCotisation = "  tarif premium  ",
            Statut = true
        };

        var res1 = await _client.PostAsJsonAsync("/api/TarifCotisation", payload1);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);

        var res2 = await _client.PostAsJsonAsync("/api/TarifCotisation", payload2);
        Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
    }

    [Fact]
    public async Task Create_SameLibelleWhenExistingInactive_IsAllowed()
    {
        var (typeAdhesionId, deviseId) = await CreateTypeAdhesionForTarifAsync();
        var inactive = new TarifCotisationCreateDto
        {
            Montant = 10m,
            Periodicite = "Mensuel",
            TypeAdhesionId = typeAdhesionId,
            DeviseId = deviseId,
            LibelleTarifCotisation = "Tarif Silver",
            Statut = false
        };
        var active = new TarifCotisationCreateDto
        {
            Montant = 20m,
            Periodicite = "Annuel",
            TypeAdhesionId = typeAdhesionId,
            DeviseId = deviseId,
            LibelleTarifCotisation = "tarif silver",
            Statut = true
        };

        var res1 = await _client.PostAsJsonAsync("/api/TarifCotisation", inactive);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);

        var res2 = await _client.PostAsJsonAsync("/api/TarifCotisation", active);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
    }

    private async Task<(int typeAdhesionId, int deviseId)> CreateTypeAdhesionForTarifAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var deviseId = await db.Devises
            .Where(d => d.EstDevisePrincipale && d.Statut)
            .Select(d => d.IdDevise)
            .FirstAsync();

        var categorie = new CategorieAdhesion
        {
            Libelle = $"Cat Tarif {Guid.NewGuid():N}".Substring(0, 20),
            Description = "Catégorie test tarif",
            Statut = true
        };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var type = new TypeAdhesion
        {
            Libelle = $"TA{Guid.NewGuid():N}".Substring(0, 10),
            Description = "Type test tarif",
            MaxDependants = 0,
            Montant = 1m,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            DeviseId = deviseId,
            Statut = true
        };
        db.TypeAdhesions.Add(type);
        await db.SaveChangesAsync();

        return (type.IdTypeAdhesion, deviseId);
    }
}

public class TarifCotisationControllerErrorIntegrationTests : IClassFixture<TarifCotisationErrorWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TarifCotisationControllerErrorIntegrationTests(TarifCotisationErrorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByAffilie_ErreurTechnique_RetourneErrorResponseAvecCorrelationId()
    {
        var response = await _client.GetAsync("/api/TarifCotisation/Affilie?idAffilie=42");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        Assert.Equal("TECHNICAL_INTERNAL_ERROR", root.GetProperty("error").GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(root.GetProperty("correlationId").GetString()));
        Assert.Equal(
            "Une erreur technique est survenue lors de la récupération des cotisations pour l'affilié",
            root.GetProperty("error").GetProperty("message").GetString());
    }
}

/// <summary>Factory avec dépôt tarif simulé en erreur pour valider la réponse 500 structurée.</summary>
public class TarifCotisationErrorWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProsocDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ProsocDbContext));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.RemoveAll(typeof(ITarifCotisationRepository));

            _connection.Open();
            services.AddSingleton(_connection);
            services.AddDbContext<ProsocDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<ITarifCotisationRepository, ThrowingTarifCotisationRepository>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            db.Database.EnsureCreated();
            SeedData.InitializeAsync(db, NullLogger.Instance).GetAwaiter().GetResult();
            TestAuthHandler.UserId = db.Utilisateurs.Select(u => u.IdUtilisateur).First().ToString();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

internal sealed class ThrowingTarifCotisationRepository : ITarifCotisationRepository
{
    public Task<List<TarifCotisation>> GetByAffilieIdAsync(int affilieId, CancellationToken ct = default)
        => throw new InvalidOperationException("erreur simulée pour test d'intégration");

    public Task<List<TarifCotisation>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TarifCotisation?> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<TarifCotisation>> GetByTypeAdhesionIdAsync(int typeAdhesionId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TarifCotisation> CreateAsync(TarifCotisation entity, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<TarifCotisation?> UpdateAsync(int id, TarifCotisation entity, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => throw new NotImplementedException();
}
