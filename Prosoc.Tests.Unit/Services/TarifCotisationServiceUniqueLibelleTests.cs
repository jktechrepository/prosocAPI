using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class TarifCotisationServiceUniqueLibelleTests
{
    private static async Task<(ProsocDbContext db, SqliteConnection connection, int typeAdhesionId, int deviseId)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", Symbole = "$", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        var categorie = new CategorieAdhesion { Libelle = "Particulier", Description = "cat", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var type = new TypeAdhesion
        {
            Libelle = "Solo",
            Description = "desc",
            MaxDependants = 0,
            Montant = 1m,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.TypeAdhesions.Add(type);
        await db.SaveChangesAsync();

        return (db, connection, type.IdTypeAdhesion, devise.IdDevise);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateActiveLibelle_Normalized()
    {
        var (db, connection, typeAdhesionId, deviseId) = await CreateContextAsync();
        await using (connection)
        await using (db)
        {
            var service = new TarifCotisationService(db);
            await service.CreateAsync(new TarifCotisation
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = typeAdhesionId,
                DeviseId = deviseId,
                LibelleTarifCotisation = "Tarif Gold",
                Statut = true
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new TarifCotisation
            {
                Montant = 10m,
                Periodicite = "Annuel",
                TypeAdhesionId = typeAdhesionId,
                DeviseId = deviseId,
                LibelleTarifCotisation = "  tarif gold ",
                Statut = true
            }));
        }
    }

    [Fact]
    public async Task CreateAsync_AllowsSameLibelle_WhenExistingIsInactive()
    {
        var (db, connection, typeAdhesionId, deviseId) = await CreateContextAsync();
        await using (connection)
        await using (db)
        {
            var service = new TarifCotisationService(db);
            await service.CreateAsync(new TarifCotisation
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = typeAdhesionId,
                DeviseId = deviseId,
                LibelleTarifCotisation = "Tarif Bronze",
                Statut = false
            });

            var created = await service.CreateAsync(new TarifCotisation
            {
                Montant = 10m,
                Periodicite = "Annuel",
                TypeAdhesionId = typeAdhesionId,
                DeviseId = deviseId,
                LibelleTarifCotisation = "tarif bronze",
                Statut = true
            });

            Assert.True(created.IdCotisationAffilie > 0);
        }
    }

    [Fact]
    public async Task CreateAsync_AllowsNullOrWhitespaceLibelle()
    {
        var (db, connection, typeAdhesionId, deviseId) = await CreateContextAsync();
        await using (connection)
        await using (db)
        {
            var service = new TarifCotisationService(db);
            var a = await service.CreateAsync(new TarifCotisation
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = typeAdhesionId,
                DeviseId = deviseId,
                LibelleTarifCotisation = null,
                Statut = true
            });
            var b = await service.CreateAsync(new TarifCotisation
            {
                Montant = 10m,
                Periodicite = "Annuel",
                TypeAdhesionId = typeAdhesionId,
                DeviseId = deviseId,
                LibelleTarifCotisation = "   ",
                Statut = true
            });

            Assert.Null(a.LibelleTarifCotisationNormalized);
            Assert.Null(b.LibelleTarifCotisationNormalized);
        }
    }
}

