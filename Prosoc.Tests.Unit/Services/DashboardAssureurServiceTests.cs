using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAssureurServiceTests
{
    private static async Task<(ProsocDbContext Db, DashboardAssureurService Service)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new DashboardAssureurService(
            db,
            new Mock<ILogger<DashboardAssureurService>>().Object);

        return (db, service);
    }

    private static async Task<(int AssureurCibleId, int AutreAssureurId)> SeedAssureurScopeDataAsync(ProsocDbContext db)
    {
        var devise = new Devise { Code = "USD", Nom = "Dollar", Statut = true };
        db.Devises.Add(devise);

        var assureurCible = new Assureur { Nom = "Assureur A", Statut = true };
        var autreAssureur = new Assureur { Nom = "Assureur B", Statut = true };
        db.Assureurs.AddRange(assureurCible, autreAssureur);
        await db.SaveChangesAsync();

        var produitCible = new ProduitAssureur
        {
            Nom = "Produit A",
            Montant = 50m,
            Periodicite = "Mensuel",
            AssureurId = assureurCible.IdAssureur,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        var produitAutre = new ProduitAssureur
        {
            Nom = "Produit B",
            Montant = 60m,
            Periodicite = "Mensuel",
            AssureurId = autreAssureur.IdAssureur,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.ProduitsAssureurs.AddRange(produitCible, produitAutre);
        await db.SaveChangesAsync();

        var prestationCible = new Prestation
        {
            NomPrestation = "Presta A",
            Montant = 50m,
            DeviseId = devise.IdDevise,
            ProduitAssureurId = produitCible.IdProduit,
            Statut = true
        };
        var prestationAutre = new Prestation
        {
            NomPrestation = "Presta B",
            Montant = 60m,
            DeviseId = devise.IdDevise,
            ProduitAssureurId = produitAutre.IdProduit,
            Statut = true
        };
        db.Prestations.AddRange(prestationCible, prestationAutre);

        var affilieCible = new Affilie
        {
            CodeAdhesion = "AFF-A",
            Nom = "Jean",
            Prenom = "A",
            NomComplet = "Jean A",
            DateNaissance = new DateTime(1985, 5, 1),
            Statut = true
        };
        var affilieAutre = new Affilie
        {
            CodeAdhesion = "AFF-B",
            Nom = "Paul",
            Prenom = "B",
            NomComplet = "Paul B",
            DateNaissance = new DateTime(1990, 3, 15),
            Statut = true
        };
        db.Affilies.AddRange(affilieCible, affilieAutre);
        await db.SaveChangesAsync();

        db.SouscriptionsPrestations.AddRange(
            new SouscriptionPrestation
            {
                AffilieId = affilieCible.IdAffilie,
                PrestationId = prestationCible.IdPrestation,
                Statut = true
            },
            new SouscriptionPrestation
            {
                AffilieId = affilieAutre.IdAffilie,
                PrestationId = prestationAutre.IdPrestation,
                Statut = true
            });

        db.Dependants.AddRange(
            new Dependant
            {
                Nom = "Enfant A",
                LienParente = "Enfant",
                AffilieId = affilieCible.IdAffilie,
                Statut = true
            },
            new Dependant
            {
                Nom = "Enfant B",
                LienParente = "Enfant",
                AffilieId = affilieAutre.IdAffilie,
                Statut = true
            });

        db.Antecedants.AddRange(
            new Antecedant
            {
                Description = "Allergie A",
                AffilieId = affilieCible.IdAffilie,
                Statut = true
            },
            new Antecedant
            {
                Description = "Allergie B",
                AffilieId = affilieAutre.IdAffilie,
                Statut = true
            });

        await db.SaveChangesAsync();

        return (assureurCible.IdAssureur, autreAssureur.IdAssureur);
    }

    [Fact]
    public async Task GetKpisAsync_ScopeAffiliesDependantsEtAntecedentsParAssureur()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (assureurCibleId, _) = await SeedAssureurScopeDataAsync(db);

            var kpis = await service.GetKpisAsync(assureurCibleId);

            Assert.Equal(1, kpis.NombreAffilies);
            Assert.Equal(1, kpis.NombreDependants);
            Assert.Equal(1, kpis.NombreAntecedents);
            Assert.Equal(1, kpis.NombreProduitsActifs);
            Assert.Equal(1, kpis.NombreSouscriptionsActives);
        }
    }

    [Fact]
    public async Task GetAffiliesAsync_RetourneCompteursDependantsEtAntecedents()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (assureurCibleId, _) = await SeedAssureurScopeDataAsync(db);

            var affilies = await service.GetAffiliesAsync(assureurCibleId);

            Assert.Single(affilies);
            Assert.Equal("AFF-A", affilies[0].CodeAdhesion);
            Assert.Equal(1, affilies[0].NombreDependants);
            Assert.Equal(1, affilies[0].NombreAntecedents);
            Assert.Equal(1, affilies[0].NombreSouscriptionsActives);
        }
    }

    [Fact]
    public async Task GetDependantsEtAntecedentsAsync_FiltrentParAssureur()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (assureurCibleId, _) = await SeedAssureurScopeDataAsync(db);

            var dependants = await service.GetDependantsAsync(assureurCibleId);
            var antecedents = await service.GetAntecedentsAsync(assureurCibleId);

            Assert.Single(dependants);
            Assert.Equal("Enfant A", dependants[0].Nom);
            Assert.Equal("AFF-A", dependants[0].CodeAdhesion);

            Assert.Single(antecedents);
            Assert.Equal("Allergie A", antecedents[0].Description);
            Assert.Equal("Jean A", antecedents[0].AffilieNomComplet);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_InclutAffiliesDependantsEtAntecedents()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (assureurCibleId, _) = await SeedAssureurScopeDataAsync(db);

            var summary = await service.GetDashboardSummaryAsync(assureurCibleId);

            Assert.Equal("Assureur A", summary.NomAssureur);
            Assert.Single(summary.AffiliesRecents);
            Assert.Single(summary.Dependants);
            Assert.Single(summary.Antecedents);
            Assert.Equal(1, summary.Kpis.NombreAffilies);
        }
    }
}
