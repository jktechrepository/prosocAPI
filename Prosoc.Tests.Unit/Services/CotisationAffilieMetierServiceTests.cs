using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class CotisationAffilieMetierServiceTests
{
    private static async Task<(ProsocDbContext db, SqliteConnection connection)> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (db, connection);
    }

    [Fact]
    public void CompterPersonnesAssurees_AvecDependants_RetourneTitulairePlusDependants()
    {
        var service = new CotisationAffilieMetierService(null!);
        Assert.Equal(4, service.CompterPersonnesAssurees(3));
        Assert.Equal(1, service.CompterPersonnesAssurees(0));
    }

    [Fact]
    public async Task CalculerMontantTotalAsync_F3_DeuxDependants_MultiplieParTroisPersonnes()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var devise = new Devise { Code = "USD", Nom = "Dollar", Statut = true };
            db.Devises.Add(devise);

            var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
            db.CategoriesAdhesions.Add(categorie);
            await db.SaveChangesAsync();

            var type = new TypeAdhesion
            {
                Libelle = "F3",
                MaxDependants = 2,
                CategorieAdhesionId = categorie.IdCategorieAdhesion,
                DeviseId = devise.IdDevise,
                Montant = 1m,
                Statut = true
            };
            db.TypeAdhesions.Add(type);
            await db.SaveChangesAsync();

            var cotisation = new CotisationAffilie
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = type.IdTypeAdhesion,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.CotisationsAffilie.Add(cotisation);
            await db.SaveChangesAsync();

            var service = new CotisationAffilieMetierService(db);
            var result = await service.CalculerMontantTotalAsync(cotisation.IdCotisationAffilie, 2);

            Assert.Equal(3, result.NombrePersonnes);
            Assert.Equal(5m, result.MontantUnitaire);
            Assert.Equal(15m, result.MontantTotal);
            Assert.Equal(devise.IdDevise, result.DeviseId);
            Assert.Equal("USD", result.DeviseCode);
        }
    }

    [Fact]
    public async Task ValidateCollecteCotisationAsync_MauvaisTypeAdhesion_LeveException()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var devise = new Devise { Code = "USD", Nom = "Dollar", Statut = true };
            db.Devises.Add(devise);

            var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
            db.CategoriesAdhesions.Add(categorie);
            await db.SaveChangesAsync();

            var solo = new TypeAdhesion { Libelle = "Solo", MaxDependants = 0, CategorieAdhesionId = categorie.IdCategorieAdhesion, DeviseId = devise.IdDevise, Statut = true };
            var f3 = new TypeAdhesion { Libelle = "F3", MaxDependants = 2, CategorieAdhesionId = categorie.IdCategorieAdhesion, DeviseId = devise.IdDevise, Statut = true };
            db.TypeAdhesions.AddRange(solo, f3);
            await db.SaveChangesAsync();

            var cotisation = new CotisationAffilie
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = f3.IdTypeAdhesion,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.CotisationsAffilie.Add(cotisation);
            await db.SaveChangesAsync();

            var service = new CotisationAffilieMetierService(db);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ValidateCollecteCotisationAsync(
                    cotisation.IdCotisationAffilie,
                    solo.IdTypeAdhesion,
                    5m,
                    0));
        }
    }
}
