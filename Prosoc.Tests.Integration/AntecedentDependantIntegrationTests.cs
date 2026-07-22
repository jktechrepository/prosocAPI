using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace Prosoc.Tests.Integration;

public class AntecedentDependantIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AntecedentDependantIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAntecedantsByDependant_RetourneUniquementAntecedentsDuDependant()
    {
        int affilieId;
        int dependantId;
        const string dependantNom = "Enfant Test Antecedent";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Parent",
                Prenom = "Test",
                NomComplet = "Parent Test",
                DateNaissance = new DateTime(1985, 5, 5),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var dependant = new Dependant
            {
                AffilieId = affilieId,
                Nom = dependantNom,
                LienParente = "FILS",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.Add(dependant);
            await db.SaveChangesAsync();
            dependantId = dependant.IdDependant;

            db.Antecedants.AddRange(
                new Antecedant
                {
                    AffilieId = affilieId,
                    DependantId = null,
                    Description = $"Titulaire {Guid.NewGuid():N}",
                    Statut = true,
                    DateCreation = DateTime.UtcNow.AddMinutes(-2)
                },
                new Antecedant
                {
                    AffilieId = affilieId,
                    DependantId = dependantId,
                    Description = $"Dependant {Guid.NewGuid():N}",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/Dependant/{dependantId}/antecedants?pageNumber=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<AntecedentReadDto>>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload!.TotalItems);
        Assert.Single(payload.Data);
        Assert.Equal(dependantId, payload.Data[0].DependantId);
        Assert.Equal(dependantNom, payload.Data[0].DependantNom);
        Assert.Equal(affilieId, payload.Data[0].AffilieId);
    }

    [Fact]
    public async Task CreateAntecedent_AvecDependantValide_Retourne201()
    {
        int affilieId;
        int dependantId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Create",
                Prenom = "Test",
                NomComplet = "Create Test",
                DateNaissance = new DateTime(1988, 3, 3),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var dependant = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Dependant Create",
                LienParente = "FILLE",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.Add(dependant);
            await db.SaveChangesAsync();
            dependantId = dependant.IdDependant;
        }

        var dto = new AntecedentCreateDto
        {
            AffilieId = affilieId,
            DependantId = dependantId,
            Description = $"Allergie test {Guid.NewGuid():N}",
            Statut = true
        };

        var response = await _client.PostAsJsonAsync("/api/Antecedent", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<AntecedentReadDto>();
        Assert.NotNull(created);
        Assert.Equal(dependantId, created!.DependantId);
        Assert.Equal("Dependant Create", created.DependantNom);
        Assert.Equal(affilieId, created.AffilieId);
    }

    [Fact]
    public async Task CreateAntecedent_AvecDependantDunAutreAffilie_Retourne400()
    {
        int affilieId;
        int otherDependantId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Cible",
                Prenom = "Test",
                NomComplet = "Cible Test",
                DateNaissance = new DateTime(1992, 7, 7),
                Statut = true
            };
            var otherAffilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Autre",
                Prenom = "Test",
                NomComplet = "Autre Test",
                DateNaissance = new DateTime(1993, 8, 8),
                Statut = true
            };
            db.Affilies.AddRange(affilie, otherAffilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var otherDependant = new Dependant
            {
                AffilieId = otherAffilie.IdAffilie,
                Nom = "Dependant Autre",
                LienParente = "FILS",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.Add(otherDependant);
            await db.SaveChangesAsync();
            otherDependantId = otherDependant.IdDependant;
        }

        var dto = new AntecedentCreateDto
        {
            AffilieId = affilieId,
            DependantId = otherDependantId,
            Description = "Ne doit pas passer",
            Statut = true
        };

        var response = await _client.PostAsJsonAsync("/api/Antecedent", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAntecedantsByAffilie_InclutInfosDependant()
    {
        int affilieId;
        int dependantId;
        const string dependantNom = "Enfant Fiche Affilie";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Fiche",
                Prenom = "Affilie",
                NomComplet = "Fiche Affilie",
                DateNaissance = new DateTime(1980, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var dependant = new Dependant
            {
                AffilieId = affilieId,
                Nom = dependantNom,
                LienParente = "FILS",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.Add(dependant);
            await db.SaveChangesAsync();
            dependantId = dependant.IdDependant;

            db.Antecedants.AddRange(
                new Antecedant
                {
                    AffilieId = affilieId,
                    Description = $"Titulaire fiche {Guid.NewGuid():N}",
                    Statut = true,
                    DateCreation = DateTime.UtcNow.AddMinutes(-1)
                },
                new Antecedant
                {
                    AffilieId = affilieId,
                    DependantId = dependantId,
                    Description = $"Dependant fiche {Guid.NewGuid():N}",
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

        var dependantAntecedent = payload.Data.FirstOrDefault(a => a.DependantId == dependantId);
        Assert.NotNull(dependantAntecedent);
        Assert.Equal(dependantNom, dependantAntecedent!.DependantNom);

        var titulaireAntecedent = payload.Data.FirstOrDefault(a => a.DependantId is null);
        Assert.NotNull(titulaireAntecedent);
    }

    [Fact]
    public async Task GetDependantById_InclutAntecedantsDansReponse()
    {
        int affilieId;
        int dependantId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Detail",
                Prenom = "Dependant",
                NomComplet = "Detail Dependant",
                DateNaissance = new DateTime(1982, 2, 2),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var dependant = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Enfant Detail",
                LienParente = "FILS",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.Add(dependant);
            await db.SaveChangesAsync();
            dependantId = dependant.IdDependant;

            db.Antecedants.Add(new Antecedant
            {
                AffilieId = affilieId,
                DependantId = dependantId,
                Description = $"Allergie detail {Guid.NewGuid():N}",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/Dependant/{dependantId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<DependantReadDto>();
        Assert.NotNull(payload);
        Assert.NotNull(payload!.Antecedants);
        Assert.Single(payload.Antecedants);
        Assert.Equal(dependantId, payload.Antecedants[0].DependantId);
    }

    [Fact]
    public async Task GetDependantsByAffilie_InclutAntecedantsParDependant()
    {
        int affilieId;
        int dependantAvecAntecedentId;
        int dependantSansAntecedentId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-{Guid.NewGuid():N}"[..12],
                Nom = "Liste",
                Prenom = "Dependants",
                NomComplet = "Liste Dependants",
                DateNaissance = new DateTime(1984, 4, 4),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            var dependantAvec = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Avec Antecedent",
                LienParente = "FILS",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            var dependantSans = new Dependant
            {
                AffilieId = affilieId,
                Nom = "Sans Antecedent",
                LienParente = "FILLE",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.AddRange(dependantAvec, dependantSans);
            await db.SaveChangesAsync();
            dependantAvecAntecedentId = dependantAvec.IdDependant;
            dependantSansAntecedentId = dependantSans.IdDependant;

            db.Antecedants.Add(new Antecedant
            {
                AffilieId = affilieId,
                DependantId = dependantAvecAntecedentId,
                Description = $"Allergie liste {Guid.NewGuid():N}",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/Dependant/by-affilie/{affilieId}?pageNumber=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<DependantReadDto>>();
        Assert.NotNull(payload);

        var avec = payload!.Data.FirstOrDefault(d => d.IdDependant == dependantAvecAntecedentId);
        var sans = payload.Data.FirstOrDefault(d => d.IdDependant == dependantSansAntecedentId);

        Assert.NotNull(avec);
        Assert.NotNull(sans);
        Assert.NotNull(avec!.Antecedants);
        Assert.NotNull(sans!.Antecedants);
        Assert.Single(avec.Antecedants);
        Assert.Empty(sans.Antecedants);
    }
}
