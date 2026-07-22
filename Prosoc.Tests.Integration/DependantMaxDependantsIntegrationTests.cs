using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Integration;

public class DependantMaxDependantsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DependantMaxDependantsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin" };
    }

    [Fact]
    public async Task PostDependant_F3_AvecUnDependantExistant_Retourne201()
    {
        var (affilieId, _) = await SeedAffilieAvecAdhesionEtDependantsAsync(
            typeLibelle: "F3",
            maxDependants: 2,
            nombreDependantsInitiaux: 1);

        var response = await _client.PostAsJsonAsync("/api/Dependant", NouveauFilsDto(affilieId, "Enfant Deux"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<DependantReadDto>();
        Assert.NotNull(created);
        Assert.Equal(affilieId, created!.AffilieId);
    }

    [Fact]
    public async Task PostDependant_F3_DejaPlein_Retourne400()
    {
        var (affilieId, _) = await SeedAffilieAvecAdhesionEtDependantsAsync(
            typeLibelle: "F3",
            maxDependants: 2,
            nombreDependantsInitiaux: 2);

        var response = await _client.PostAsJsonAsync("/api/Dependant", NouveauFilsDto(affilieId, "Enfant Trop"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("maximum autorisé", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2", body);
    }

    [Fact]
    public async Task PutDependant_VersAffilieF3Plein_Retourne400()
    {
        var (cibleId, _) = await SeedAffilieAvecAdhesionEtDependantsAsync(
            typeLibelle: "F3",
            maxDependants: 2,
            nombreDependantsInitiaux: 2);

        var (sourceId, sourceDependantIds) = await SeedAffilieAvecAdhesionEtDependantsAsync(
            typeLibelle: "F6",
            maxDependants: 5,
            nombreDependantsInitiaux: 1);

        var dependantId = sourceDependantIds[0];

        var response = await _client.PutAsJsonAsync($"/api/Dependant/{dependantId}", new DependantUpdateDto
        {
            Nom = "Transfert Plein",
            LienParente = "FILS",
            AffilieId = cibleId,
            DateNaissance = DateTime.Today.AddYears(-10)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("maximum autorisé", doc.RootElement.GetProperty("message").GetString() ?? "", StringComparison.OrdinalIgnoreCase);

        // Le dépendant reste sur l'affilié source
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var still = await db.Dependants.AsNoTracking().FirstAsync(d => d.IdDependant == dependantId);
        Assert.Equal(sourceId, still.AffilieId);
    }

    private static DependantCreateDto NouveauFilsDto(int affilieId, string nom) => new()
    {
        AffilieId = affilieId,
        Nom = nom,
        LienParente = "FILS",
        DateNaissance = DateTime.Today.AddYears(-8)
    };

    private async Task<(int AffilieId, List<int> DependantIds)> SeedAffilieAvecAdhesionEtDependantsAsync(
        string typeLibelle,
        int maxDependants,
        int nombreDependantsInitiaux)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();
        var categorieId = await db.CategoriesAdhesions.Select(c => c.IdCategorieAdhesion).FirstAsync();
        var utilisateurId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

        var type = await db.TypeAdhesions
            .FirstOrDefaultAsync(t => t.Libelle == typeLibelle && t.MaxDependants == maxDependants);
        if (type == null)
        {
            type = new TypeAdhesion
            {
                Libelle = typeLibelle,
                MaxDependants = maxDependants,
                CategorieAdhesionId = categorieId,
                DeviseId = deviseId,
                Description = $"Test {typeLibelle}",
                Montant = 1.5m,
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.TypeAdhesions.Add(type);
            await db.SaveChangesAsync();
        }

        var affilie = new Affilie
        {
            CodeAdhesion = $"MX-{suffix}",
            Nom = "Parent",
            Prenom = "Max",
            NomComplet = $"Parent Max {suffix}",
            DateNaissance = new DateTime(1985, 1, 1),
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = type.IdTypeAdhesion,
            UtilisateurId = utilisateurId,
            StatutDossier = "VALIDÉ",
            Statut = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var dependantIds = new List<int>();
        for (var i = 0; i < nombreDependantsInitiaux; i++)
        {
            var dep = new Dependant
            {
                AffilieId = affilie.IdAffilie,
                Nom = $"Existants {i + 1} {suffix}",
                LienParente = "FILS",
                DateNaissance = DateTime.Today.AddYears(-(5 + i)),
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Dependants.Add(dep);
            await db.SaveChangesAsync();
            dependantIds.Add(dep.IdDependant);
        }

        return (affilie.IdAffilie, dependantIds);
    }
}
