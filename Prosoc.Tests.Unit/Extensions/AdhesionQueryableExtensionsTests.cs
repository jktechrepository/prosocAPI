using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Extensions;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Extensions;

public class AdhesionQueryableExtensionsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ProsocDbContext _db;

    public AdhesionQueryableExtensionsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new ProsocDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ApplyAdhesionSearch_ParNomAffilie_RetourneAdhesionCorrespondante()
    {
        var (adhesionId, _) = await SeedAdhesionAsync(
            nomComplet: "Jean Mukendi Test",
            codeAdhesion: "CODE-SEARCH-1",
            statutDossier: "EN ATTENTE",
            typeLibelle: "F3");

        var result = await _db.Adhesions
            .Include(a => a.Affilie)
            .Include(a => a.TypeAdhesion)
            .ApplyAdhesionSearch("mukendi")
            .Select(a => a.IdAdhesion)
            .ToListAsync();

        Assert.Contains(adhesionId, result);
    }

    [Fact]
    public async Task ApplyAdhesionSearch_ParCodeAdhesion_RetourneAdhesionCorrespondante()
    {
        var (adhesionId, _) = await SeedAdhesionAsync(
            nomComplet: "Autre Personne",
            codeAdhesion: "PROSOC-UNIQUE-99",
            statutDossier: "A",
            typeLibelle: "Solo");

        var result = await _db.Adhesions
            .Include(a => a.Affilie)
            .Include(a => a.TypeAdhesion)
            .ApplyAdhesionSearch("unique-99")
            .Select(a => a.IdAdhesion)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal(adhesionId, result[0]);
    }

    [Fact]
    public async Task ApplyAdhesionSearch_ParIdAdhesion_RetourneAdhesionCorrespondante()
    {
        var (adhesionId, _) = await SeedAdhesionAsync(
            nomComplet: "Id Search",
            codeAdhesion: "CODE-ID-1",
            statutDossier: "VALIDÉ",
            typeLibelle: "F6");

        var result = await _db.Adhesions
            .Include(a => a.Affilie)
            .Include(a => a.TypeAdhesion)
            .ApplyAdhesionSearch(adhesionId.ToString())
            .Select(a => a.IdAdhesion)
            .ToListAsync();

        Assert.Single(result);
        Assert.Equal(adhesionId, result[0]);
    }

    [Fact]
    public async Task ApplyAdhesionSearch_ParStatutDossier_RetourneAdhesionsCorrespondantes()
    {
        await SeedAdhesionAsync("Alpha", "CODE-A", "EN ATTENTE", "Solo");
        await SeedAdhesionAsync("Beta", "CODE-B", "VALIDÉ", "F3");

        var result = await _db.Adhesions
            .Include(a => a.Affilie)
            .Include(a => a.TypeAdhesion)
            .ApplyAdhesionSearch("valid")
            .Select(a => a.StatutDossier)
            .ToListAsync();

        Assert.Contains("VALIDÉ", result);
        Assert.DoesNotContain("EN ATTENTE", result);
    }

    [Fact]
    public async Task ApplyAdhesionSearch_TermeVide_RetourneToutesLesAdhesions()
    {
        await SeedAdhesionAsync("Un", "CODE-1", "A", "Solo");
        await SeedAdhesionAsync("Deux", "CODE-2", "B", "F3");

        var result = await _db.Adhesions
            .Include(a => a.Affilie)
            .Include(a => a.TypeAdhesion)
            .ApplyAdhesionSearch("   ")
            .CountAsync();

        Assert.Equal(2, result);
    }

    private async Task<(int AdhesionId, int AffilieId)> SeedAdhesionAsync(
        string nomComplet,
        string codeAdhesion,
        string statutDossier,
        string typeLibelle)
    {
        if (!await _db.CategoriesAdhesions.AnyAsync())
        {
            _db.CategoriesAdhesions.Add(new CategorieAdhesion
            {
                Libelle = "Particulier",
                Description = "Test",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        if (!await _db.Devises.AnyAsync())
        {
            _db.Devises.Add(new Devise
            {
                Nom = "Franc Congolais",
                Code = "CDF",
                Symbole = "FC",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        var categorieId = await _db.CategoriesAdhesions.Select(c => c.IdCategorieAdhesion).FirstAsync();
        var deviseId = await _db.Devises.Select(d => d.IdDevise).FirstAsync();

        var type = new TypeAdhesion
        {
            Libelle = typeLibelle,
            CategorieAdhesionId = categorieId,
            MaxDependants = 0,
            Description = "Test",
            Montant = 1m,
            DeviseId = deviseId,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        _db.TypeAdhesions.Add(type);
        await _db.SaveChangesAsync();

        if (!await _db.Utilisateurs.AnyAsync())
        {
            _db.Utilisateurs.Add(new Utilisateur
            {
                NomUtilisateur = "test-user",
                MotDePasseHash = "hash",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        var utilisateurId = await _db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();

        var affilie = new Affilie
        {
            CodeAdhesion = codeAdhesion,
            Nom = nomComplet.Split(' ').First(),
            Prenom = "Test",
            NomComplet = nomComplet,
            DateNaissance = new DateTime(1990, 1, 1),
            Telephone = "0811111111",
            EmailAffilie = $"{codeAdhesion}@test.cd",
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        _db.Affilies.Add(affilie);
        await _db.SaveChangesAsync();

        var adhesion = new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = type.IdTypeAdhesion,
            UtilisateurId = utilisateurId,
            StatutDossier = statutDossier,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        _db.Adhesions.Add(adhesion);
        await _db.SaveChangesAsync();

        return (adhesion.IdAdhesion, affilie.IdAffilie);
    }
}
