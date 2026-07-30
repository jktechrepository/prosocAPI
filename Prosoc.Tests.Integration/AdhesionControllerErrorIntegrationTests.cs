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
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace Prosoc.Tests.Integration;

public class AdhesionControllerPaginationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AdhesionControllerPaginationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin" };
    }

    [Fact]
    public async Task GetPaginated_AvecPagination_Retourne200()
    {
        var response = await _client.GetAsync("/api/Adhesion/paginated?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AvecSearchParNomAffilie_RetourneAdhesionCorrespondante()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        int adhesionId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
            var utilisateurId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

            var affilie = new Affilie
            {
                CodeAdhesion = $"SRCH-{suffix}",
                Nom = "Kabila",
                Prenom = "Search",
                NomComplet = $"SearchTest Kabila {suffix}",
                DateNaissance = new DateTime(1988, 6, 6),
                Telephone = "0820000001",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var adhesion = new Adhesion
            {
                AffilieId = affilie.IdAffilie,
                TypeAdhesionId = typeAdhesionId,
                UtilisateurId = utilisateurId,
                StatutDossier = "EN ATTENTE",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Adhesions.Add(adhesion);
            await db.SaveChangesAsync();
            adhesionId = adhesion.IdAdhesion;
        }

        var response = await _client.GetAsync($"/api/Adhesion?search={suffix}&page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<AdhesionListItemForSearchTest>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Data, a => a.Id == adhesionId);
    }

    [Fact]
    public async Task GetAll_FiltreStatutDossierEnAttente_RetourneUniquementEnAttente()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        int idEnAttente;
        int idValide;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
            var utilisateurId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

            var affilieAttente = new Affilie
            {
                CodeAdhesion = $"ATT-{suffix}",
                Nom = "Filtre",
                Prenom = "Attente",
                NomComplet = $"Filtre Attente {suffix}",
                DateNaissance = new DateTime(1990, 1, 1),
                Telephone = "0820000011",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            var affilieValide = new Affilie
            {
                CodeAdhesion = $"VAL-{suffix}",
                Nom = "Filtre",
                Prenom = "Valide",
                NomComplet = $"Filtre Valide {suffix}",
                DateNaissance = new DateTime(1991, 2, 2),
                Telephone = "0820000012",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Affilies.AddRange(affilieAttente, affilieValide);
            await db.SaveChangesAsync();

            var adhesionAttente = new Adhesion
            {
                AffilieId = affilieAttente.IdAffilie,
                TypeAdhesionId = typeAdhesionId,
                UtilisateurId = utilisateurId,
                StatutDossier = AdhesionStatutDossierRegles.EnAttente,
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            var adhesionValide = new Adhesion
            {
                AffilieId = affilieValide.IdAffilie,
                TypeAdhesionId = typeAdhesionId,
                UtilisateurId = utilisateurId,
                StatutDossier = AdhesionStatutDossierRegles.Valide,
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Adhesions.AddRange(adhesionAttente, adhesionValide);
            await db.SaveChangesAsync();
            idEnAttente = adhesionAttente.IdAdhesion;
            idValide = adhesionValide.IdAdhesion;
        }

        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = new[] { "READ_ADHESION" };

        var response = await _client.GetAsync(
            $"/api/Adhesion?statutDossier=EN%20ATTENTE&search={suffix}&page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<AdhesionListItemForStatutTest>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Data, a => a.Id == idEnAttente);
        Assert.DoesNotContain(payload.Data, a => a.Id == idValide);
        Assert.All(payload.Data, a => Assert.Equal(AdhesionStatutDossierRegles.EnAttente, a.StatutDossier));
    }

    [Fact]
    public async Task GetAll_FiltreStatutDossierInvalide_RetourneBadRequest()
    {
        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = new[] { "READ_ADHESION" };

        var response = await _client.GetAsync("/api/Adhesion?statutDossier=INCONNU");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("statutDossier", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAll_AsAgentAa_ForceFiltreEnAttente_IgnoreStatutDossierValide()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        int idEnAttente;
        int idValide;

        try
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
                var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
                var utilisateurId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

                var affilieAttente = new Affilie
                {
                    CodeAdhesion = $"AA-A-{suffix}",
                    Nom = "AaScope",
                    Prenom = "Attente",
                    NomComplet = $"AaScope Attente {suffix}",
                    DateNaissance = new DateTime(1992, 3, 3),
                    Telephone = "0820000021",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                };
                var affilieValide = new Affilie
                {
                    CodeAdhesion = $"AA-V-{suffix}",
                    Nom = "AaScope",
                    Prenom = "Valide",
                    NomComplet = $"AaScope Valide {suffix}",
                    DateNaissance = new DateTime(1993, 4, 4),
                    Telephone = "0820000022",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                };
                db.Affilies.AddRange(affilieAttente, affilieValide);
                await db.SaveChangesAsync();

                var adhesionAttente = new Adhesion
                {
                    AffilieId = affilieAttente.IdAffilie,
                    TypeAdhesionId = typeAdhesionId,
                    UtilisateurId = utilisateurId,
                    StatutDossier = AdhesionStatutDossierRegles.EnAttente,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                };
                var adhesionValide = new Adhesion
                {
                    AffilieId = affilieValide.IdAffilie,
                    TypeAdhesionId = typeAdhesionId,
                    UtilisateurId = utilisateurId,
                    StatutDossier = AdhesionStatutDossierRegles.Valide,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                };
                db.Adhesions.AddRange(adhesionAttente, adhesionValide);
                await db.SaveChangesAsync();
                idEnAttente = adhesionAttente.IdAdhesion;
                idValide = adhesionValide.IdAdhesion;
            }

            TestAuthHandler.Roles = new[] { "Agent (AA)" };
            TestAuthHandler.Permissions = new[] { "READ_ADHESION" };

            var response = await _client.GetAsync(
                $"/api/Adhesion?statutDossier=VALID%C3%89&search={suffix}&page=1&pageSize=50");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<AdhesionListItemForStatutTest>>();
            Assert.NotNull(payload);
            Assert.Contains(payload!.Data, a => a.Id == idEnAttente);
            Assert.DoesNotContain(payload.Data, a => a.Id == idValide);
            Assert.All(payload.Data, a => Assert.Equal(AdhesionStatutDossierRegles.EnAttente, a.StatutDossier));
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    private sealed class AdhesionListItemForSearchTest
    {
        public int Id { get; set; }
    }

    private sealed class AdhesionListItemForStatutTest
    {
        public int Id { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
    }
}

public class AdhesionControllerErrorIntegrationTests : IClassFixture<AdhesionErrorWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdhesionControllerErrorIntegrationTests(AdhesionErrorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPaginated_ErreurTechnique_RetourneErrorResponseAvecCorrelationId()
    {
        var response = await _client.GetAsync("/api/Adhesion/paginated?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        Assert.Equal("TECHNICAL_INTERNAL_ERROR", root.GetProperty("error").GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(root.GetProperty("correlationId").GetString()));
        Assert.Equal(
            "Une erreur technique est survenue lors de la récupération des adhésions paginées",
            root.GetProperty("error").GetProperty("message").GetString());
    }
}

public class AdhesionErrorWebApplicationFactory : WebApplicationFactory<Program>
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
