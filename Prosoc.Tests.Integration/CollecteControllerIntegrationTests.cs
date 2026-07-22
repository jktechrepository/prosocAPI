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
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace Prosoc.Tests.Integration;

public class CollecteControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CollecteControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_AvecPagination_Retourne200()
    {
        var response = await _client.GetAsync("/api/Collecte?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByAffilieSimple_AffilieSansCollecte_RetourneListeVide()
    {
        var response = await _client.GetAsync("/api/Collecte/by-affilie/999999/simple");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<CollecteReadDto>>();
        Assert.NotNull(list);
        Assert.Empty(list);
    }
}

public class CollecteControllerErrorIntegrationTests : IClassFixture<CollecteErrorWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CollecteControllerErrorIntegrationTests(CollecteErrorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ErreurTechnique_RetourneErrorResponseAvecCorrelationId()
    {
        var response = await _client.GetAsync("/api/Collecte?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        Assert.Equal("TECHNICAL_INTERNAL_ERROR", root.GetProperty("error").GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(root.GetProperty("correlationId").GetString()));
        Assert.Equal(
            "Une erreur technique est survenue lors de la récupération des collectes paginées",
            root.GetProperty("error").GetProperty("message").GetString());
    }
}

public class CollecteErrorWebApplicationFactory : WebApplicationFactory<Program>
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

            services.RemoveAll(typeof(IPaginationService));
            services.AddScoped<IPaginationService, ThrowingPaginationService>();

            _connection.Open();
            services.AddSingleton(_connection);
            services.AddDbContext<ProsocDbContext>(options => options.UseSqlite(_connection));

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

internal sealed class ThrowingPaginationService : IPaginationService
{
    public Task<PaginatedResponse<T>> CreatePaginatedResponseAsync<T>(
        IQueryable<T> query,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("erreur simulée pagination collectes");

    public Task<ExtendedPaginatedResponse<T>> CreateExtendedPaginatedResponseAsync<T>(
        IQueryable<T> query,
        AdvancedPaginationRequest request,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterRequest> filters) => query;

    public IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, string sortDirection = "asc") => query;

    public IQueryable<T> ApplySearch<T>(IQueryable<T> query, string searchTerm, List<string>? searchFields = null) => query;
}
