using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class TypeAdhesionDependantsValidationServiceTests
{
    [Fact]
    public async Task ValidateDependantsCountAsync_F3_TroisDependants_LeveException()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", Symbole = "$", Statut = true, EstDevisePrincipale = true };
        db.Devises.Add(devise);

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var f3 = new TypeAdhesion
        {
            Libelle = "F3",
            MaxDependants = 2,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.TypeAdhesions.Add(f3);
        await db.SaveChangesAsync();

        var service = new TypeAdhesionDependantsValidationService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ValidateDependantsCountAsync(f3.IdTypeAdhesion, 3));
    }

    [Fact]
    public async Task ValidateDependantsCountAsync_F6_CinqDependants_Accepte()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", Symbole = "$", Statut = true, EstDevisePrincipale = true };
        db.Devises.Add(devise);

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var f6 = new TypeAdhesion
        {
            Libelle = "F6",
            MaxDependants = 5,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.TypeAdhesions.Add(f6);
        await db.SaveChangesAsync();

        var service = new TypeAdhesionDependantsValidationService(db);

        await service.ValidateDependantsCountAsync(f6.IdTypeAdhesion, 5);
    }
}
