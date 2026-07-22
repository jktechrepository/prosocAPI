using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using ProsocAPI.Helpers;
using System.Security.Claims;

namespace Prosoc.Tests.Unit.Services;

public class SouscriptionPrestationAchatServiceTests
{
    private sealed class SeedContext
    {
        public ProsocDbContext Db { get; init; } = null!;
        public int AffilieId { get; init; }
        public int AgentId { get; init; }
        public int PrestationId { get; init; }
        public int DeviseCdfId { get; init; }
        public int DeviseUsdId { get; init; }
    }

    private static async Task<SeedContext> SeedEligibleAffilieAsync(
        SqliteConnection connection,
        decimal produitMontant = 5000m,
        decimal walletUsdSolde = 500m)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc", EstDevisePrincipale = false, Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();

        db.TauxChangeDevises.Add(new TauxChangeDevise
        {
            DeviseSourceId = usd.IdDevise,
            DeviseCibleId = cdf.IdDevise,
            Taux = 2850m,
            DateEffet = new DateTime(2020, 1, 1),
            Statut = true
        });
        await db.SaveChangesAsync();

        var categorie = new CategorieAdhesion { Libelle = "CatAch", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var typeAdhesion = new TypeAdhesion
        {
            Libelle = "Solo",
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            MaxDependants = 0,
            Montant = 1m,
            DeviseId = usd.IdDevise,
            Statut = true
        };
        db.TypeAdhesions.Add(typeAdhesion);
        await db.SaveChangesAsync();

        var cotisation = new CotisationAffilie
        {
            Montant = 5m,
            Periodicite = "Mensuel",
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            DeviseId = cdf.IdDevise,
            Statut = true
        };
        db.CotisationsAffilie.Add(cotisation);
        await db.SaveChangesAsync();

        var agent = new Agent
        {
            NomComplet = "Agent Achat",
            Matricule = "AG-ACH",
            Phone = "0990000100",
            Statut = true
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.WalletsVirtuelsAgents.Add(new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = usd.IdDevise,
            SoldeVirtuel = walletUsdSolde,
            Statut = true
        });

        var affilie = new Affilie
        {
            CodeAdhesion = "ADH-ACH-01",
            Nom = "Test",
            Prenom = "Achat",
            NomComplet = "Test Achat",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = $"user-{affilie.CodeAdhesion}",
            MotDePasseHash = "hash",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            Statut = true
        };
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            UtilisateurId = utilisateur.IdUtilisateur,
            StatutDossier = "COMPLET",
            Statut = true
        });

        var periodeCourante = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15);
        db.Collectes.Add(new Collecte
        {
            TypeCollecte = TypeCollecte.Cotisation,
            CotisationAffilieId = cotisation.IdCotisationAffilie,
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            Montant = 5m,
            DeviseId = cdf.IdDevise,
            Mois = periodeCourante.Month,
            Annee = periodeCourante.Year,
            StatutPaiement = "PAYE",
            ModePaiement = "ESPECE",
            DateCollecte = periodeCourante,
            DateCreation = periodeCourante,
            Statut = true
        });

        var produit = new ProduitMutuel
        {
            Nom = "Assistance test",
            Montant = produitMontant,
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
            NomPrestation = "Assistance Funéraire Test",
            Montant = (int)produitMontant,
            DeviseId = cdf.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        return new SeedContext
        {
            Db = db,
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            PrestationId = prestation.IdPrestation,
            DeviseCdfId = cdf.IdDevise,
            DeviseUsdId = usd.IdDevise
        };
    }

    private static SouscriptionPrestationAchatService CreateService(
        ProsocDbContext db,
        Mock<ICommissionService>? commissionMock = null,
        string roleJwt = "Agent (AT)")
    {
        var commission = commissionMock ?? new Mock<ICommissionService>();
        commission.Setup(x => x.ProcessCommissionAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var conversion = new DeviseConversionService(db);
        var cotisationMetier = new CotisationAffilieMetierService(db);
        var multidevise = new CollecteMultideviseService(
            db, conversion, cotisationMetier,
            Options.Create(new MultideviseOptions { DeviseTarifCotisationCode = "CDF" }));

        var http = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim> { new(ClaimTypes.Role, roleJwt) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        http.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = principal });

        return new SouscriptionPrestationAchatService(
            db,
            multidevise,
            new WalletVirtuelPaymentService(
                db, conversion, new WalletVirtuelMouvementService(db)),
            commission.Object,
            http.Object,
            NullLogger<SouscriptionPrestationAchatService>.Instance);
    }

    private static SouscriptionPrestationAchatCreateDto BuildDto(
        SeedContext seed,
        decimal montant = 5000m,
        int mois = 3,
        int annee = 2026) => new()
    {
        PrestationId = seed.PrestationId,
        Statut = true,
        Collecte = new SouscriptionPrestationCollecteCreateDto
        {
            AgentId = seed.AgentId,
            Montant = montant,
            DeviseId = seed.DeviseCdfId,
            ModePaiement = "VIRTUAL_ACCOUNT",
            Mois = mois,
            Annee = annee
        }
    };

    [Fact]
    public async Task CreateWithCollecteAsync_VirtualAccountCrossDevise_Reussit()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedEligibleAffilieAsync(connection);
        var service = CreateService(seed.Db);

        var (souscription, collecte) = await service.CreateWithCollecteAsync(
            seed.AffilieId, BuildDto(seed));

        Assert.True(souscription.IdSouscriptionPrestation > 0);
        Assert.True(collecte.IdCollecte > 0);
        Assert.Equal(souscription.IdSouscriptionPrestation, collecte.SouscriptionPrestationId);
        Assert.Equal(TypeCollecte.Souscription, collecte.TypeCollecte);
        Assert.Equal(new DateTime(2026, 3, 1), collecte.DateCollecte.Date);

        var count = await seed.Db.SouscriptionsPrestations.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateWithCollecteAsync_VirtualAccount_Caissier_Refuse()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedEligibleAffilieAsync(connection);
        var service = CreateService(seed.Db, roleJwt: "Caissier");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateWithCollecteAsync(seed.AffilieId, BuildDto(seed)));

        Assert.Equal(WalletVirtuelPaiementAutorisation.MessageNonAutorise, ex.Message);
        Assert.Equal(0, await seed.Db.SouscriptionsPrestations.CountAsync());
    }

    [Fact]
    public async Task CreateWithCollecteAsync_MontantIncorrect_RollbackSansSouscription()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedEligibleAffilieAsync(connection);
        var service = CreateService(seed.Db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateWithCollecteAsync(seed.AffilieId, BuildDto(seed, montant: 1m)));

        Assert.Equal(0, await seed.Db.SouscriptionsPrestations.CountAsync());
        Assert.Equal(1, await seed.Db.Collectes.CountAsync(c => c.TypeCollecte == TypeCollecte.Cotisation));
    }

    [Fact]
    public async Task CreateWithCollecteAsync_FlexPay_Rejete()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedEligibleAffilieAsync(connection);
        var service = CreateService(seed.Db);

        var dto = BuildDto(seed);
        dto.Collecte.ModePaiement = "MOBILE_MONEY";

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateWithCollecteAsync(seed.AffilieId, dto));

        Assert.Contains("paiement-electronique", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWithCollecteAsync_DoublonSouscriptionActive_Conflict()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var seed = await SeedEligibleAffilieAsync(connection);
        var service = CreateService(seed.Db);

        await service.CreateWithCollecteAsync(seed.AffilieId, BuildDto(seed, mois: 4));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateWithCollecteAsync(seed.AffilieId, BuildDto(seed, mois: 5)));
    }
}
