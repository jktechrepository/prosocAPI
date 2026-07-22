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

public class AffilieControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AffilieControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAffilies_AvecPagination_Retourne200()
    {
        var response = await _client.GetAsync("/api/Affilie?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAffilie_RetourneDependantsAntecedantsEtPersonneContact()
    {
        int affilieId;
        var certificat = new byte[] { 10, 20, 30 };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Assoc",
                Prenom = "Detail",
                NomComplet = "Assoc Detail",
                DateNaissance = new DateTime(1988, 4, 4),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            db.PersonnesContact.Add(new PersonneContact
            {
                AffilieId = affilieId,
                NomComplet = "Jean Contact",
                LienParente = "EPOUX",
                Adresse = "Kinshasa, Gombe",
                Statut = true
            });

            var dependant = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Enfant Assoc",
                LienParente = "FILLE",
                Adresse = "Selembao",
                DateNaissance = new DateTime(2012, 5, 5),
                CertificatScolariteData = certificat,
                CertificatScolariteContentType = "image/png",
                Statut = true
            };
            db.Dependants.Add(dependant);
            await db.SaveChangesAsync();

            db.Antecedants.Add(new Antecedant
            {
                AffilieId = affilieId,
                Description = "Diabète titulaire",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/Affilie/{affilieId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<AffilieReadDto>();
        Assert.NotNull(dto);
        Assert.NotNull(dto!.PersonneContact);
        Assert.Equal("Jean Contact", dto.PersonneContact!.NomComplet);

        var dependantDto = Assert.Single(dto.Dependants);
        Assert.Equal("Enfant Assoc", dependantDto.Nom);
        Assert.Equal(Convert.ToBase64String(certificat), dependantDto.CertificatScolariteBase64);
        Assert.Equal("image/png", dependantDto.CertificatScolariteContentType);

        Assert.Contains(dto.Antecedants, a => a.Description == "Diabète titulaire");
    }

    [Fact]
    public async Task GetAntecedantsByAffilie_RetourneListePaginee()
    {
        int affilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = await db.Affilies
                .OrderBy(a => a.IdAffilie)
                .FirstOrDefaultAsync();
            if (affilie == null)
            {
                affilie = new Affilie
                {
                    CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                    Nom = "Test",
                    Prenom = "Antecedant",
                    NomComplet = "Test Antecedant",
                    DateNaissance = new DateTime(1990, 1, 1),
                    Statut = true
                };
                db.Affilies.Add(affilie);
                await db.SaveChangesAsync();
            }
            affilieId = affilie.IdAffilie;

            db.Antecedants.AddRange(
                new Antecedant
                {
                    AffilieId = affilieId,
                    Description = $"Antécédent test A {Guid.NewGuid():N}",
                    Statut = true,
                    DateCreation = DateTime.UtcNow.AddMinutes(-1)
                },
                new Antecedant
                {
                    AffilieId = affilieId,
                    Description = $"Antécédent test B {Guid.NewGuid():N}",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/Affilie/{affilieId}/antecedants?pageNumber=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<AntecedentReadDto>>();
        Assert.NotNull(payload);
        Assert.True(payload!.TotalItems >= 2);
        Assert.True(payload.Data.Count >= 2);
        Assert.All(payload.Data.Take(2), a => Assert.Equal(affilieId, a.AffilieId));
    }

    [Fact]
    public async Task GetDependantsByAffilie_RetourneListePagineeAvecAntecedants()
    {
        int affilieId;
        int dependantAvecAntecedentId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Parent",
                Prenom = "Dependants",
                NomComplet = "Parent Dependants",
                DateNaissance = new DateTime(1986, 6, 6),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var dependantAvec = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Enfant Avec",
                LienParente = "FILS",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            var dependantSans = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Enfant Sans",
                LienParente = "FILLE",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.AddRange(dependantAvec, dependantSans);
            await db.SaveChangesAsync();
            dependantAvecAntecedentId = dependantAvec.IdDependant;

            db.Antecedants.Add(new Antecedant
            {
                AffilieId = affilieId,
                DependantId = dependantAvecAntecedentId,
                Description = $"Allergie affilie route {Guid.NewGuid():N}",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/Affilie/{affilieId}/dependants?pageNumber=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<DependantReadDto>>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.TotalItems);

        var avec = payload.Data.First(d => d.IdDependant == dependantAvecAntecedentId);
        var sans = payload.Data.First(d => d.IdDependant != dependantAvecAntecedentId);

        Assert.NotNull(avec.Antecedants);
        Assert.Single(avec.Antecedants);
        Assert.NotNull(sans.Antecedants);
        Assert.Empty(sans.Antecedants);
    }

    [Fact]
    public async Task GetDependantsByAffilie_AffilieInexistant_Retourne404()
    {
        var response = await _client.GetAsync("/api/Affilie/999999999/dependants?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class AffilieControllerErrorIntegrationTests : IClassFixture<AffilieErrorWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AffilieControllerErrorIntegrationTests(AffilieErrorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAffilies_ErreurTechnique_RetourneErrorResponseAvecCorrelationId()
    {
        var response = await _client.GetAsync("/api/Affilie?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        Assert.Equal("TECHNICAL_INTERNAL_ERROR", root.GetProperty("error").GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(root.GetProperty("correlationId").GetString()));
        Assert.Equal(
            "Une erreur technique est survenue lors de la récupération des affiliés",
            root.GetProperty("error").GetProperty("message").GetString());
    }
}

public class AffilieErrorWebApplicationFactory : WebApplicationFactory<Program>
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
