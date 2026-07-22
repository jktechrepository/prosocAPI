using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class PerceptionVirtuelleServiceTests
{
    private static async Task<(PerceptionVirtuelleService Service, ProsocDbContext Db)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>().UseSqlite(connection).Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var pagination = new Mock<IPaginationService>();
        var service = new PerceptionVirtuelleService(
            db,
            pagination.Object,
            new DeviseConversionService(db),
            Mock.Of<ILogger<PerceptionVirtuelleService>>());

        return (service, db);
    }

    [Fact]
    public void ResolveAgentIdEffectif_PrefereCollecteAgentId()
    {
        var collecte = new Collecte { AgentId = 5, AffilieId = 1 };
        var map = new Dictionary<int, int?> { [1] = 9 };

        Assert.Equal(5, PerceptionVirtuelleService.ResolveAgentIdEffectif(collecte, map));
    }

    [Fact]
    public void ResolveAgentIdEffectif_UtiliseAdhesionSiCollecteSansAgent()
    {
        var collecte = new Collecte { AgentId = null, AffilieId = 1 };
        var map = new Dictionary<int, int?> { [1] = 9 };

        Assert.Equal(9, PerceptionVirtuelleService.ResolveAgentIdEffectif(collecte, map));
    }

    [Fact]
    public async Task ConfirmerPerception_CollecteEligible_MarquePercu()
    {
        var (service, db) = await CreateContextAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        var agent = new Agent { NomComplet = "AT Test", Matricule = "AT000000001", Phone = "0990000001", Statut = true };
        var percepteur = new Utilisateur { NomUtilisateur = "Percepteur", MotDePasseHash = "hash", Statut = true };
        db.Agents.Add(agent);
        db.Utilisateurs.Add(percepteur);
        var categorieAdhesion = new CategorieAdhesion { Libelle = "Cat", Statut = true };
        db.CategoriesAdhesions.Add(categorieAdhesion);
        await db.SaveChangesAsync();
        db.TypeAdhesions.Add(new TypeAdhesion { Libelle = "Standard", CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion, Statut = true });
        await db.SaveChangesAsync();
        var frais = new Frais { Libelle = "Frais test", Montant = 150, DeviseId = devise.IdDevise, Statut = true };
        db.Frais.Add(frais);
        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-PV-1",
            Nom = "Nom",
            Prenom = "Prenom",
            NomComplet = "Nom Prenom",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            TypeAdhesionId = typeAdhesionId,
            UtilisateurId = percepteur.IdUtilisateur,
            StatutDossier = "A",
            Statut = true,
            DateCreation = DateTime.Now
        });

        var wallet = new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            SoldeVirtuel = 1000m,
            Statut = true
        };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            Montant = 150m,
            MontantDevisePrincipale = 150m,
            TypeCollecte = TypeCollecte.Frais,
            FraisId = frais.IdFrais,
            ModePaiement = MethodePaiementHelper.VirtualAccount,
            StatutPaiement = CollecteStatutPaiement.Valide,
            StatutPerception = CollecteStatutPerception.NonPerçu,
            Statut = true,
            DateCollecte = DateTime.Now
        };
        db.Collectes.Add(collecte);
        await db.SaveChangesAsync();

        db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
        {
            WalletVirtuelId = wallet.IdWalletVirtuelAgent,
            Montant = 150m,
            TypeOperation = "DEBIT",
            Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
            ReferenceExterne = collecte.IdCollecte,
            Statut = true
        });
        await db.SaveChangesAsync();

        var result = await service.ConfirmerPerceptionAsync(
            percepteur.IdUtilisateur,
            new PerceptionVirtuelleConfirmerDto
            {
                AgentId = agent.IdAgent,
                CollecteIds = new List<int> { collecte.IdCollecte }
            });

        Assert.True(result.Succes);
        Assert.Equal(150m, result.MontantTotal);
        Assert.Equal(0m, result.SoldeRestantAgent);

        var updated = await db.Collectes.FindAsync(collecte.IdCollecte);
        Assert.Equal(CollecteStatutPerception.Perçu, updated!.StatutPerception);
        Assert.NotNull(updated.PerceptionVirtuelleId);

        var lignes = await db.PerceptionsVirtuellesLignes.CountAsync();
        Assert.Equal(1, lignes);
    }

    [Fact]
    public async Task ConfirmerPerception_CollecteDejaPercue_RetourneConflict()
    {
        var (service, db) = await CreateContextAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var agent = new Agent { NomComplet = "AT", Matricule = "AT000000002", Phone = "0990000002", Statut = true };
        var percepteur = new Utilisateur { NomUtilisateur = "Perc", MotDePasseHash = "hash", Statut = true };
        db.Devises.Add(devise);
        db.Agents.Add(agent);
        db.Utilisateurs.Add(percepteur);
        var categorieAdhesion = new CategorieAdhesion { Libelle = "Cat2", Statut = true };
        db.CategoriesAdhesions.Add(categorieAdhesion);
        await db.SaveChangesAsync();
        db.TypeAdhesions.Add(new TypeAdhesion { Libelle = "Std", CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion, Statut = true });
        await db.SaveChangesAsync();
        var frais = new Frais { Libelle = "F2", Montant = 50, DeviseId = devise.IdDevise, Statut = true };
        db.Frais.Add(frais);
        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-PV-2",
            Nom = "N",
            Prenom = "P",
            NomComplet = "N P",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
        db.Adhesions.Add(new Adhesion { AffilieId = affilie.IdAffilie, AgentId = agent.IdAgent, TypeAdhesionId = typeAdhesionId, UtilisateurId = percepteur.IdUtilisateur, StatutDossier = "A", Statut = true, DateCreation = DateTime.Now });
        var wallet = new WalletVirtuelAgent { AgentId = agent.IdAgent, DeviseId = devise.IdDevise, SoldeVirtuel = 500m, Statut = true };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            Montant = 50m,
            TypeCollecte = TypeCollecte.Frais,
            FraisId = frais.IdFrais,
            ModePaiement = "VIRTUAL_ACCOUNT",
            StatutPaiement = CollecteStatutPaiement.Valide,
            StatutPerception = CollecteStatutPerception.Perçu,
            Statut = true,
            DateCollecte = DateTime.Now
        };
        db.Collectes.Add(collecte);
        await db.SaveChangesAsync();

        db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
        {
            WalletVirtuelId = wallet.IdWalletVirtuelAgent,
            Montant = 50m,
            TypeOperation = "DEBIT",
            Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
            ReferenceExterne = collecte.IdCollecte,
            Statut = true
        });
        await db.SaveChangesAsync();

        var result = await service.ConfirmerPerceptionAsync(
            percepteur.IdUtilisateur,
            new PerceptionVirtuelleConfirmerDto { AgentId = agent.IdAgent, CollecteIds = new List<int> { collecte.IdCollecte } });

        Assert.False(result.Succes);
        Assert.Equal("COLLECTE_DEJA_PERCUE", result.CodeErreur);
    }

    [Fact]
    public async Task GetReconciliationAsync_SansCollecte_RetourneZeros()
    {
        var (service, db) = await CreateContextAsync();
        db.Devises.Add(new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true });
        await db.SaveChangesAsync();

        var reconciliation = await service.GetReconciliationAsync(null, null, null);

        Assert.Equal("USD", reconciliation.DeviseCode);
        Assert.Equal(0m, reconciliation.MontantDebitWallet);
        Assert.Equal(0m, reconciliation.MontantNonPerçu);
        Assert.Equal(0m, reconciliation.MontantPerçuTerrain);
    }
}
