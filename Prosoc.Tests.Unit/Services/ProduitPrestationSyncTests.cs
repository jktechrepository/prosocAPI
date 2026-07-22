using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class ProduitPrestationSyncTests
{
    private static async Task<(SqliteConnection Connection, ProsocDbContext Db)> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (connection, db);
    }

    [Fact]
    public async Task EnsureAndSyncMutuel_Update_PropageMontantEtNom()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            var produit = new ProduitMutuel
            {
                Nom = "Pack A",
                Montant = 10m,
                Periodicite = "Mensuel",
                AgeMin = 0,
                AgeMax = 65,
                DeviseId = devise.IdDevise,
                TauxCommissionAT = 10m,
                Statut = true
            };
            db.ProduitsMutuels.Add(produit);
            await db.SaveChangesAsync();

            db.Prestations.Add(new Prestation
            {
                NomPrestation = "Pack A",
                Montant = 10m,
                DeviseId = devise.IdDevise,
                ProduitMutuelId = produit.IdProduit,
                Statut = true
            });
            await db.SaveChangesAsync();

            produit.Nom = "Pack A Plus";
            produit.Montant = 25m;
            await ProduitPrestationSync.EnsureAndSyncMutuelAsync(db, produit);

            var prestation = await db.Prestations.SingleAsync(p => p.ProduitMutuelId == produit.IdProduit);
            Assert.Equal("Pack A Plus", prestation.NomPrestation);
            Assert.Equal(25m, prestation.Montant);
            Assert.Equal("Mensuel", prestation.Periodicite);
            Assert.Contains("25", prestation.Description);
        }
    }

    [Fact]
    public async Task ValidateDeleteMutuel_AvecSouscription_LeveException()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
            db.Devises.Add(devise);
            var affilie = new Affilie
            {
                CodeAdhesion = "ADH-SYNC",
                Nom = "Test",
                Prenom = "User",
                NomComplet = "Test User",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var produit = new ProduitMutuel
            {
                Nom = "P",
                Montant = 5m,
                Periodicite = "Mensuel",
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.ProduitsMutuels.Add(produit);
            await db.SaveChangesAsync();

            var prestation = new Prestation
            {
                NomPrestation = "P",
                Montant = 5m,
                DeviseId = devise.IdDevise,
                ProduitMutuelId = produit.IdProduit,
                Statut = true
            };
            db.Prestations.Add(prestation);
            await db.SaveChangesAsync();

            db.SouscriptionsPrestations.Add(new SouscriptionPrestation
            {
                AffilieId = affilie.IdAffilie,
                PrestationId = prestation.IdPrestation,
                Statut = true
            });
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                ProduitPrestationSync.ValidateDeleteMutuelAsync(db, produit.IdProduit));
        }
    }
}
