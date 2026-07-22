using System.Net;
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
using ProsocAPI.Services;

namespace Prosoc.Tests.Integration;

public class ProvinceControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProvinceControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_AvecPagination_Retourne200()
    {
        var response = await _client.GetAsync("/api/Province?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class ProvinceControllerErrorIntegrationTests : IClassFixture<ProvinceErrorWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProvinceControllerErrorIntegrationTests(ProvinceErrorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ErreurTechnique_RetourneErrorResponseAvecCorrelationId()
    {
        var response = await _client.GetAsync("/api/Province?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        Assert.Equal("TECHNICAL_INTERNAL_ERROR", root.GetProperty("error").GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(root.GetProperty("correlationId").GetString()));
    }
}

public class ProvinceErrorWebApplicationFactory : WebApplicationFactory<Program>
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
