using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class ProduitEligibiliteRulesTests
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
    public void ValidateAgeEligibilite_HorsTranche_LeveException()
    {
        var produit = new ProduitMutuel { Nom = "Junior", AgeMin = 0, AgeMax = 18 };
        Assert.Throws<ArgumentException>(() =>
            ProduitEligibiliteRules.ValidateAgeEligibilite(new DateTime(1980, 1, 1), produit));
    }

    [Fact]
    public void CalculerAgeAns_CalculeCorrectement()
    {
        var age = ProduitEligibiliteRules.CalculerAgeAns(
            new DateTime(2010, 6, 15),
            new DateTime(2026, 5, 20));
        Assert.Equal(15, age);
    }

    [Fact]
    public async Task ValidateCotisationAJour_SansPaiement_LeveException()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            var categorie = new CategorieAdhesion { Libelle = "Cat", Statut = true };
            db.CategoriesAdhesions.Add(categorie);
            await db.SaveChangesAsync();

            var typeAdhesion = new TypeAdhesion
            {
                Libelle = "Famille",
                CategorieAdhesionId = categorie.IdCategorieAdhesion,
                MaxDependants = 3,
                Montant = 1m,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.TypeAdhesions.Add(typeAdhesion);
            await db.SaveChangesAsync();

            var cotisation = new CotisationAffilie
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.CotisationsAffilie.Add(cotisation);

            var affilie = new Affilie
            {
                CodeAdhesion = "ADH-ELIG",
                Nom = "Test",
                Prenom = "User",
                NomComplet = "Test User",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                ProduitEligibiliteRules.ValidateCotisationAJourAsync(
                    db, affilie.IdAffilie, typeAdhesionIdOverride: typeAdhesion.IdTypeAdhesion));
        }
    }

    [Fact]
    public async Task ValidateCotisationAJour_AvecPaiementMoisCourant_Accepte()
    {
        var (connection, db) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            var categorie = new CategorieAdhesion { Libelle = "Cat", Statut = true };
            db.CategoriesAdhesions.Add(categorie);
            await db.SaveChangesAsync();

            var typeAdhesion = new TypeAdhesion
            {
                Libelle = "Famille",
                CategorieAdhesionId = categorie.IdCategorieAdhesion,
                MaxDependants = 3,
                Montant = 1m,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.TypeAdhesions.Add(typeAdhesion);
            await db.SaveChangesAsync();

            var cotisation = new CotisationAffilie
            {
                Montant = 5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.CotisationsAffilie.Add(cotisation);

            var affilie = new Affilie
            {
                CodeAdhesion = "ADH-OK",
                Nom = "Test",
                Prenom = "Ok",
                NomComplet = "Test Ok",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            var agent = new Agent { NomComplet = "Agent", Matricule = "AG-ELIG-01", Phone = "099", Statut = true };
            db.Agents.Add(agent);
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            db.Collectes.Add(new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                CotisationAffilieId = cotisation.IdCotisationAffilie,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                Montant = 5m,
                DeviseId = devise.IdDevise,
                DateCollecte = DateTime.Now,
                StatutPaiement = "Validé",
                Statut = true
            });
            await db.SaveChangesAsync();

            var ex = await Record.ExceptionAsync(() =>
                ProduitEligibiliteRules.ValidateCotisationAJourAsync(
                    db, affilie.IdAffilie, typeAdhesionIdOverride: typeAdhesion.IdTypeAdhesion));
            Assert.Null(ex);
        }
    }

    [Fact]
    public async Task ValidateAchatProduit_CotisationDansLot_SansAffilieId_Accepte()
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
                Nom = "Maash",
                Montant = 0m,
                EstGratuit = true,
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
                NomPrestation = "Maash",
                Montant = 0m,
                DeviseId = devise.IdDevise,
                ProduitMutuelId = produit.IdProduit,
                Statut = true
            };
            db.Prestations.Add(prestation);
            await db.SaveChangesAsync();

            var ex = await Record.ExceptionAsync(() =>
                ProduitEligibiliteRules.ValidateAchatProduitAsync(
                    db,
                    affilieId: 0,
                    prestation.IdPrestation,
                    dateNaissanceOverride: new DateTime(1985, 3, 10),
                    typeAdhesionIdOverride: 1,
                    cotisationPayeeDansLot: true));
            Assert.Null(ex);
        }
    }

    [Fact]
    public async Task ValidateAchatProduit_NouvelleAdhesionSansCotisationDansLot_Accepte()
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
                Nom = "Assistance",
                Montant = 10m,
                EstGratuit = false,
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
                NomPrestation = "Assistance",
                Montant = 10m,
                DeviseId = devise.IdDevise,
                ProduitMutuelId = produit.IdProduit,
                Statut = true
            };
            db.Prestations.Add(prestation);
            await db.SaveChangesAsync();

            var ex = await Record.ExceptionAsync(() =>
                ProduitEligibiliteRules.ValidateAchatProduitAsync(
                    db,
                    affilieId: 0,
                    prestation.IdPrestation,
                    dateNaissanceOverride: new DateTime(1991, 2, 27),
                    typeAdhesionIdOverride: 2,
                    cotisationPayeeDansLot: false,
                    nouvelleAdhesionNiveau1: true));
            Assert.Null(ex);
        }
    }
}
