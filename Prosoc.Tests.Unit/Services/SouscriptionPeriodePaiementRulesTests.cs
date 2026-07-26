using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class SouscriptionPeriodePaiementRulesTests
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

    private static async Task<int> SeedSouscriptionAvecProduitAsync(
        ProsocDbContext db,
        decimal montantProduit,
        bool estGratuit = false)
    {
        var devise = new Devise
        {
            Code = "CDF",
            Nom = "Franc",
            Symbole = "FC",
            Statut = true,
            EstDevisePrincipale = false
        };
        db.Devises.Add(devise);
        await db.SaveChangesAsync();

        var produit = new ProduitMutuel
        {
            Nom = "Produit test période",
            Montant = montantProduit,
            EstGratuit = estGratuit,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.ProduitsMutuels.Add(produit);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Prestation test",
            Montant = montantProduit,
            DeviseId = devise.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        var affilie = new Affilie
        {
            Nom = "Test",
            Prenom = "Aff",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var souscription = new SouscriptionPrestation
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            DateSouscription = DateTime.UtcNow,
            Statut = true
        };
        db.SouscriptionsPrestations.Add(souscription);
        await db.SaveChangesAsync();

        return souscription.IdSouscriptionPrestation;
    }

    private static async Task AddCollecteAsync(
        ProsocDbContext db,
        int souscriptionId,
        int mois,
        int annee,
        decimal montant,
        string statutPaiement = CollecteStatutPaiement.Valide,
        decimal? montantTarifAttendu = null)
    {
        var souscription = await db.SouscriptionsPrestations.FindAsync(souscriptionId);
        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();
        db.Collectes.Add(new Collecte
        {
            TypeCollecte = TypeCollecte.Souscription,
            SouscriptionPrestationId = souscriptionId,
            AffilieId = souscription!.AffilieId,
            AgentId = null,
            Montant = montant,
            MontantAttendu = montant,
            MontantTarifAttendu = montantTarifAttendu ?? montant,
            DeviseId = deviseId,
            Mois = mois,
            Annee = annee,
            ModePaiement = "ESPECE",
            StatutPaiement = statutPaiement,
            Statut = true,
            DateCollecte = DateTime.UtcNow,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task EstPeriodeSoldee_QuandSommeAtteintAttendu_RetourneTrue()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var souscriptionId = await SeedSouscriptionAvecProduitAsync(db, 100m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 100m);

            var soldee = await SouscriptionPeriodePaiementRules.EstPeriodeSoldeeAsync(
                db, souscriptionId, 3, 2026, 100m);

            Assert.True(soldee);
        }
    }

    [Fact]
    public async Task EstPeriodeSoldee_PaiementPartiel_RetourneFalse()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var souscriptionId = await SeedSouscriptionAvecProduitAsync(db, 100m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 40m);

            var soldee = await SouscriptionPeriodePaiementRules.EstPeriodeSoldeeAsync(
                db, souscriptionId, 3, 2026, 100m);

            Assert.False(soldee);
        }
    }

    [Fact]
    public async Task EstPeriodeSoldee_DeuxPartielsAtteignentTotal_RetourneTrue()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var souscriptionId = await SeedSouscriptionAvecProduitAsync(db, 100m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 40m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 60m);

            var soldee = await SouscriptionPeriodePaiementRules.EstPeriodeSoldeeAsync(
                db, souscriptionId, 3, 2026, 100m);

            Assert.True(soldee);
        }
    }

    [Fact]
    public async Task EnsurePeriodeNonSoldee_QuandSoldee_ThrowAvecCode()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var souscriptionId = await SeedSouscriptionAvecProduitAsync(db, 5000m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 5000m);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                SouscriptionPeriodePaiementRules.EnsurePeriodeNonSoldeeAsync(
                    db, souscriptionId, 3, 2026));

            Assert.Contains(SouscriptionPeriodePaiementRules.CodeErreurDejaPayeePeriode, ex.Message);
            Assert.Contains("03/2026", ex.Message);
        }
    }

    [Fact]
    public async Task EnsurePeriodeNonSoldee_AutrePeriode_Ok()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var souscriptionId = await SeedSouscriptionAvecProduitAsync(db, 5000m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 5000m);

            await SouscriptionPeriodePaiementRules.EnsurePeriodeNonSoldeeAsync(
                db, souscriptionId, 4, 2026);
        }
    }

    [Fact]
    public async Task CalculerMontantPaye_IgnoreCollectesEnAttente()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var souscriptionId = await SeedSouscriptionAvecProduitAsync(db, 100m);
            await AddCollecteAsync(db, souscriptionId, 3, 2026, 100m, CollecteStatutPaiement.EnAttente);

            var somme = await SouscriptionPeriodePaiementRules.CalculerMontantPayePeriodeAsync(
                db, souscriptionId, 3, 2026);

            Assert.Equal(0m, somme);
        }
    }
}
