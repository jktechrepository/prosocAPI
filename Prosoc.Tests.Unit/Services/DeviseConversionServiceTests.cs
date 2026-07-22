using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using Prosoc.Tests.Unit.Helpers;

namespace Prosoc.Tests.Unit.Services;

public class DeviseConversionServiceTests
{
    private static async Task<(ProsocDbContext db, Devise usd, Devise cdf)> SeedDevisesAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", EstDevisePrincipale = false, Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();

        db.TauxChangeDevises.Add(new TauxChangeDevise
        {
            DeviseSourceId = usd.IdDevise,
            DeviseCibleId = cdf.IdDevise,
            Taux = 2850m,
            DateEffet = new DateTime(2026, 1, 1),
            Statut = true
        });
        await db.SaveChangesAsync();

        return (db, usd, cdf);
    }

    [Fact]
    public async Task ConvertirAsync_MemeDevise_RetourneMontantIdentique()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, _) = await SeedDevisesAsync(connection);
        var service = new DeviseConversionService(db);

        var (montant, taux) = await service.ConvertirAsync(100m, usd.IdDevise, usd.IdDevise, DateTime.UtcNow);

        Assert.Equal(100m, montant);
        Assert.Equal(1m, taux);
    }

    [Fact]
    public async Task ConvertirAsync_UsdVersCdf_AppliqueTauxDirect()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf) = await SeedDevisesAsync(connection);
        var service = new DeviseConversionService(db);

        var (montant, taux) = await service.ConvertirAsync(10m, usd.IdDevise, cdf.IdDevise, new DateTime(2026, 5, 1));

        Assert.Equal(28500m, montant);
        Assert.Equal(2850m, taux);
    }

    [Fact]
    public async Task ConvertirAsync_CdfVersUsd_UtiliseTauxInverse()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf) = await SeedDevisesAsync(connection);
        var service = new DeviseConversionService(db);

        var (montant, taux) = await service.ConvertirAsync(28500m, cdf.IdDevise, usd.IdDevise, new DateTime(2026, 5, 1));

        Assert.Equal(10m, montant);
        Assert.True(taux > 0 && taux < 1m);
    }

    [Fact]
    public async Task PreviewConversionAsync_VersDevisePrincipaleParDefaut()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, _, cdf) = await SeedDevisesAsync(connection);
        var service = new DeviseConversionService(db);

        var preview = await service.PreviewConversionAsync("CDF", 28500m, null, new DateTime(2026, 5, 1));

        Assert.Equal("CDF", preview.CodeDeviseSource);
        Assert.Equal("USD", preview.CodeDeviseCible);
        Assert.Equal(10m, preview.MontantConverti);
    }

    [Fact]
    public async Task CollecteMultidevise_ValidePaiementCrossDevise()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf) = await SeedDevisesAsync(connection);

        var categorie = new CategorieAdhesion { Libelle = "Part", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var typeAdhesion = new TypeAdhesion
        {
            Libelle = "Solo",
            MaxDependants = 0,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            Montant = 1m,
            Statut = true
        };
        db.TypeAdhesions.Add(typeAdhesion);
        await db.SaveChangesAsync();

        var cotisation = new CotisationAffilie
        {
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            Periodicite = "Mensuel",
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            Statut = true
        };
        db.CotisationsAffilie.Add(cotisation);
        await db.SaveChangesAsync();

        var conversion = new DeviseConversionService(db);
        var cotisationMetier = new CotisationAffilieMetierService(db);
        var multidevise = new CollecteMultideviseService(
            db, conversion, cotisationMetier,
            Options.Create(new MultideviseOptions { DeviseTarifCotisationCode = "CDF" }));

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Cotisation,
            CotisationAffilieId = cotisation.IdCotisationAffilie,
            AffilieId = 1,
            AgentId = 1,
            Montant = 10m,
            DeviseId = usd.IdDevise,
            DateCollecte = new DateTime(2026, 5, 1)
        };

        await multidevise.ValidateAndApplySnapshotAsync(collecte, 0);

        Assert.Equal(usd.IdDevise, collecte.DevisePrincipaleId);
        Assert.Equal(10m, collecte.MontantDevisePrincipale);
        Assert.Equal(cdf.IdDevise, collecte.DeviseTarifId);
        Assert.Equal(28500m, collecte.MontantTarifAttendu);
    }

    [Fact]
    public async Task CollecteMultidevise_DateConversionPaiement_UtiliseTauxDuJour()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf) = await SeedDevisesAsync(connection);

        var categorie = new CategorieAdhesion { Libelle = "Part", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var typeAdhesion = new TypeAdhesion
        {
            Libelle = "Solo",
            MaxDependants = 0,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            Montant = 1m,
            Statut = true
        };
        db.TypeAdhesions.Add(typeAdhesion);
        await db.SaveChangesAsync();

        var cotisation = new CotisationAffilie
        {
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            Periodicite = "Mensuel",
            Montant = 28500m,
            DeviseId = cdf.IdDevise,
            Statut = true
        };
        db.CotisationsAffilie.Add(cotisation);
        await db.SaveChangesAsync();

        var conversion = new DeviseConversionService(db);
        var cotisationMetier = new CotisationAffilieMetierService(db);
        var multidevise = new CollecteMultideviseService(
            db, conversion, cotisationMetier,
            Options.Create(new MultideviseOptions { DeviseTarifCotisationCode = "CDF" }));

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Cotisation,
            CotisationAffilieId = cotisation.IdCotisationAffilie,
            AffilieId = 1,
            AgentId = 1,
            Montant = 10m,
            DeviseId = usd.IdDevise,
            DateCollecte = new DateTime(2020, 3, 1)
        };

        await multidevise.ValidateAndApplySnapshotAsync(
            collecte, 0, default, new DateTime(2026, 5, 1));

        Assert.Equal(10m, collecte.MontantDevisePrincipale);
    }

    [Fact]
    public async Task CollecteMultidevise_SouscriptionCdf_DateConversionPaiement_SnapshotDevisePrincipale()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var (db, usd, cdf) = await SeedDevisesAsync(connection);

        var produit = new ProduitMutuel
        {
            Nom = "Assistance",
            Montant = 5000m,
            EstGratuit = false,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = cdf.IdDevise,
            Statut = true
        };
        db.ProduitsMutuels.Add(produit);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Assistance Funéraire",
            Montant = 5000,
            DeviseId = cdf.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        var conversion = new DeviseConversionService(db);
        var cotisationMetier = new CotisationAffilieMetierService(db);
        var multidevise = new CollecteMultideviseService(
            db, conversion, cotisationMetier,
            Options.Create(new MultideviseOptions { DeviseTarifCotisationCode = "CDF" }));

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Souscription,
            SouscriptionPrestationId = prestation.IdPrestation,
            AffilieId = 1,
            AgentId = 1,
            Montant = 5000m,
            DeviseId = cdf.IdDevise,
            DateCollecte = new DateTime(2026, 3, 1)
        };

        await multidevise.ValidateAndApplySnapshotAsync(
            collecte, 0, default, new DateTime(2026, 6, 12));

        Assert.Equal(usd.IdDevise, collecte.DevisePrincipaleId);
        Assert.Equal(5000m, collecte.MontantTarifAttendu);
        Assert.True(collecte.MontantDevisePrincipale > 0);
    }

    [Fact]
    public async Task ConvertirAsync_ApresBackfillLegacy_ConversionUsdCdf()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await LegacyTauxChangeBackfill.EnsureLegacyColumnAsync(db);

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", EstDevisePrincipale = false, Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();

        await LegacyTauxChangeBackfill.SetLegacyTauxAsync(db, usd.IdDevise, 2850m);
        Assert.False(await db.TauxChangeDevises.AnyAsync());

        var inserted = await LegacyTauxChangeBackfill.ApplyAsync(db);
        Assert.Equal(1, inserted);

        var service = new DeviseConversionService(db);
        var (montant, taux) = await service.ConvertirAsync(10m, usd.IdDevise, cdf.IdDevise, new DateTime(2026, 5, 1));

        Assert.Equal(28500m, montant);
        Assert.Equal(2850m, taux);
    }

    [Fact]
    public async Task BackfillLegacy_Idempotent_NeDupliquePasTauxActif()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await LegacyTauxChangeBackfill.EnsureLegacyColumnAsync(db);

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", EstDevisePrincipale = false, Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();
        await LegacyTauxChangeBackfill.SetLegacyTauxAsync(db, usd.IdDevise, 2850m);

        Assert.Equal(1, await LegacyTauxChangeBackfill.ApplyAsync(db));
        Assert.Equal(0, await LegacyTauxChangeBackfill.ApplyAsync(db));
        Assert.Single(await db.TauxChangeDevises.Where(t => t.Statut).ToListAsync());
    }
}
