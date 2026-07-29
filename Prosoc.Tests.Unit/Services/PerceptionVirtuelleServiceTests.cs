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
        var deviseConversion = new DeviseConversionService(db);
        var caisse = new CaisseService(
            db,
            deviseConversion,
            pagination.Object,
            Mock.Of<ILogger<CaisseService>>());
        var walletMvt = new WalletVirtuelMouvementService(db);
        var service = new PerceptionVirtuelleService(
            db,
            pagination.Object,
            deviseConversion,
            caisse,
            walletMvt,
            Mock.Of<ILogger<PerceptionVirtuelleService>>());

        return (service, db);
    }

    private static async Task OuvrirSessionAsync(ProsocDbContext db, int utilisateurId, int deviseId)
    {
        db.SessionsCaisses.Add(new SessionCaisse
        {
            UtilisateurId = utilisateurId,
            SoldeOuverture = 100000m,
            DeviseId = deviseId,
            Statut = SessionCaisseStatut.Ouverte,
            DateOuverture = DateTime.Now,
            DateCreation = DateTime.Now
        });
        await db.SaveChangesAsync();
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
        db.TypeAdhesions.Add(new TypeAdhesion
        {
            Libelle = "Standard",
            CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        });
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

        await OuvrirSessionAsync(db, percepteur.IdUtilisateur, devise.IdDevise);

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

        var perception = await db.PerceptionsVirtuelles.FirstAsync();
        Assert.Equal(PerceptionVirtuelleStatuts.Confirmee, perception.StatutMetier);

        var entreeCaisse = await db.MouvementsCaisses.SingleAsync(m =>
            m.PerceptionVirtuelleId == perception.IdPerceptionVirtuelle && m.Statut);
        Assert.Equal(MouvementCaisseTypes.Entree, entreeCaisse.TypeOperation);
        Assert.Equal(MouvementCaisseSources.PerceptionVirtuelle, entreeCaisse.Source);
        Assert.Equal(150m, entreeCaisse.Montant);

        var credit = await db.WalletVirtuelMouvements.SingleAsync(m =>
            m.Source == WalletVirtuelMouvementSources.RemisePerceptionVirtuelle && m.Statut);
        Assert.Equal("CREDIT", credit.TypeOperation);
        Assert.Equal(150m, credit.Montant);

        var walletApres = await db.WalletsVirtuelsAgents.FindAsync(wallet.IdWalletVirtuelAgent);
        Assert.Equal(1150m, walletApres!.SoldeVirtuel);
    }

    [Fact]
    public async Task ConfirmerPerception_SansSessionCaisse_RetourneSessionRequise()
    {
        var (service, db) = await CreateContextAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        var agent = new Agent { NomComplet = "AT Sess", Matricule = "AT000000088", Phone = "0990000088", Statut = true };
        var percepteur = new Utilisateur { NomUtilisateur = "PercSess", MotDePasseHash = "hash", Statut = true };
        db.Agents.Add(agent);
        db.Utilisateurs.Add(percepteur);
        var categorieAdhesion = new CategorieAdhesion { Libelle = "CatSess", Statut = true };
        db.CategoriesAdhesions.Add(categorieAdhesion);
        await db.SaveChangesAsync();
        db.TypeAdhesions.Add(new TypeAdhesion
        {
            Libelle = "Std",
            CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        });
        await db.SaveChangesAsync();
        var frais = new Frais { Libelle = "Fsess", Montant = 10, DeviseId = devise.IdDevise, Statut = true };
        db.Frais.Add(frais);
        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-PV-SESS",
            Nom = "N",
            Prenom = "P",
            NomComplet = "N P",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();
        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            TypeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync(),
            UtilisateurId = percepteur.IdUtilisateur,
            StatutDossier = "A",
            Statut = true,
            DateCreation = DateTime.Now
        });
        var wallet = new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            SoldeVirtuel = 100m,
            Statut = true
        };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();
        var collecte = new Collecte
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            Montant = 10m,
            MontantDevisePrincipale = 10m,
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
            Montant = 10m,
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

        Assert.False(result.Succes);
        Assert.Equal("SESSION_CAISSIER_REQUISE", result.CodeErreur);
    }

    [Fact]
    public async Task AnnulerPerception_RemetCollecteNonPercu_EtPermetReconfirmation()
    {
        var (service, db) = await CreateContextAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        var agent = new Agent { NomComplet = "AT Annul", Matricule = "AT000000099", Phone = "0990000099", Statut = true };
        var percepteur = new Utilisateur { NomUtilisateur = "PercepteurAnnul", MotDePasseHash = "hash", Statut = true };
        var financier = new Utilisateur { NomUtilisateur = "FinancierAnnul", MotDePasseHash = "hash", Statut = true };
        db.Agents.Add(agent);
        db.Utilisateurs.AddRange(percepteur, financier);
        var categorieAdhesion = new CategorieAdhesion { Libelle = "CatAnnul", Statut = true };
        db.CategoriesAdhesions.Add(categorieAdhesion);
        await db.SaveChangesAsync();
        db.TypeAdhesions.Add(new TypeAdhesion
        {
            Libelle = "Std",
            CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        });
        await db.SaveChangesAsync();
        var frais = new Frais { Libelle = "Frais annul", Montant = 80, DeviseId = devise.IdDevise, Statut = true };
        db.Frais.Add(frais);
        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-PV-ANN",
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
            Montant = 80m,
            MontantDevisePrincipale = 80m,
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
            Montant = 80m,
            TypeOperation = "DEBIT",
            Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
            ReferenceExterne = collecte.IdCollecte,
            Statut = true
        });
        await db.SaveChangesAsync();

        await OuvrirSessionAsync(db, percepteur.IdUtilisateur, devise.IdDevise);

        var confirm = await service.ConfirmerPerceptionAsync(
            percepteur.IdUtilisateur,
            new PerceptionVirtuelleConfirmerDto
            {
                AgentId = agent.IdAgent,
                CollecteIds = new List<int> { collecte.IdCollecte }
            });
        Assert.True(confirm.Succes);
        var perceptionId = confirm.PerceptionVirtuelleId!.Value;

        var walletApresConfirm = await db.WalletsVirtuelsAgents.FindAsync(wallet.IdWalletVirtuelAgent);
        Assert.Equal(1080m, walletApresConfirm!.SoldeVirtuel);

        var annul = await service.AnnulerPerceptionAsync(
            financier.IdUtilisateur,
            perceptionId,
            new PerceptionVirtuelleAnnulerDto { Motif = "Erreur de lot" });

        Assert.True(annul.Succes);

        var perception = await db.PerceptionsVirtuelles.FindAsync(perceptionId);
        Assert.Equal(PerceptionVirtuelleStatuts.Annulee, perception!.StatutMetier);
        Assert.Equal("Erreur de lot", perception.MotifAnnulation);
        Assert.Equal(financier.IdUtilisateur, perception.AnnuleParUtilisateurId);

        var updated = await db.Collectes.FindAsync(collecte.IdCollecte);
        Assert.Equal(CollecteStatutPerception.NonPerçu, updated!.StatutPerception);
        Assert.Null(updated.PerceptionVirtuelleId);

        Assert.False(await db.MouvementsCaisses.AnyAsync(m =>
            m.PerceptionVirtuelleId == perceptionId && m.Statut));
        Assert.True(await db.WalletVirtuelMouvements.AnyAsync(m =>
            m.Source == WalletVirtuelMouvementSources.AnnulRemisePerceptionVirtuelle && m.Statut));

        var walletApresAnnul = await db.WalletsVirtuelsAgents.FindAsync(wallet.IdWalletVirtuelAgent);
        Assert.Equal(1000m, walletApresAnnul!.SoldeVirtuel);

        var reconfirm = await service.ConfirmerPerceptionAsync(
            percepteur.IdUtilisateur,
            new PerceptionVirtuelleConfirmerDto
            {
                AgentId = agent.IdAgent,
                CollecteIds = new List<int> { collecte.IdCollecte }
            });
        Assert.True(reconfirm.Succes);
        Assert.NotEqual(perceptionId, reconfirm.PerceptionVirtuelleId);
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
        db.TypeAdhesions.Add(new TypeAdhesion
        {
            Libelle = "Std",
            CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        });
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
