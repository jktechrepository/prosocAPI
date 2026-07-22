using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Queue;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class CommissionServiceTests
{
    private static CommissionService CreateService(
        ProsocDbContext db,
        Mock<INotificationQueueService> queue,
        Mock<IArrieresAffilieService> arrieres,
        Mock<IPenaliteAffilieService> penalite,
        IWalletVirtuelPaymentService walletVirtuelPayment)
    {
        return new CommissionService(
            db,
            Mock.Of<ILogger<CommissionService>>(),
            Mock.Of<ICommissionNotificationService>(),
            queue.Object,
            arrieres.Object,
            penalite.Object,
            walletVirtuelPayment,
            new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>()),
            new DeviseConversionService(db));
    }

    private static async Task<ProsocDbContext> CreateDbContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    [Fact]
    public async Task ProcessCommissionAsync_TypeFrais_UsesFraisTauxCommission()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var devise = new Devise { Code = "CDF", Nom = "Franc Congolais", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);

        var agent = new Agent { NomComplet = "Agent A", Matricule = "AG000000001", Phone = "0990000001", Statut = true };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var wallet = new WalletAgent { AgentId = agent.IdAgent, DeviseId = devise.IdDevise, SoldeCourant = 0m, SoldeDisponible = 0m, Statut = true };
        db.WalletsAgents.Add(wallet);

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
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

        var affilie = new Affilie
        {
            CodeAdhesion = "ADH-001",
            Nom = "Doe",
            Prenom = "John",
            NomComplet = "Doe John",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = "user-frais",
            MotDePasseHash = "hash",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            Statut = true
        };
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            StatutDossier = "A",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            UtilisateurId = utilisateur.IdUtilisateur,
            Statut = true
        });

        var frais = new Frais
        {
            Libelle = "Frais Adhesion",
            Montant = 100,
            DeviseId = devise.IdDevise,
            TauxCommission = 30m,
            Statut = true
        };
        db.Frais.Add(frais);
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Frais,
            FraisId = frais.IdFrais,
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            Montant = 100m,
            DeviseId = devise.IdDevise,
            Statut = true,
            StatutPaiement = "Valide"
        };

        var logger = new Mock<ILogger<CommissionService>>();
        var notificationService = new Mock<ICommissionNotificationService>();
        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);

        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var walletReloaded = await db.WalletsAgents.FirstAsync(x => x.IdWalletAgent == wallet.IdWalletAgent);
        Assert.Equal(30m, walletReloaded.SoldeCourant);
        Assert.Equal(30m, walletReloaded.SoldeDisponible);

        var mouvement = await db.WalletMouvements.FirstAsync();
        Assert.Equal(30m, mouvement.Montant);
        Assert.Equal("COMM_COLLECTE", mouvement.Source);

        queue.Verify(x => x.QueueCommissionNotificationAsync(agent.IdAgent, 30m, collecte.IdCollecte, 0m, 30m), Times.Once);
    }

    [Fact]
    public async Task ProcessCommissionAsync_TypeSouscription_UsesProduitTauxCommission()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var devise = new Devise { Code = "CDF", Nom = "Franc Congolais", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);

        var agent = new Agent { NomComplet = "Agent B", Matricule = "AG000000002", Phone = "0990000002", Statut = true };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.WalletsAgents.Add(new WalletAgent { AgentId = agent.IdAgent, DeviseId = devise.IdDevise, SoldeCourant = 0m, SoldeDisponible = 0m, Statut = true });

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
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

        var affilie = new Affilie
        {
            CodeAdhesion = "ADH-002",
            Nom = "Smith",
            Prenom = "Jane",
            NomComplet = "Smith Jane",
            DateNaissance = new DateTime(1991, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = "user-souscription",
            MotDePasseHash = "hash",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            Statut = true
        };
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            StatutDossier = "A",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            UtilisateurId = utilisateur.IdUtilisateur,
            Statut = true
        });

        var produitMutuel = new ProduitMutuel
        {
            Nom = "PM Test",
            Montant = 200m,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = devise.IdDevise,
            TauxCommissionAT = 15m,
            Statut = true
        };
        db.ProduitsMutuels.Add(produitMutuel);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Presta PM",
            Montant = 200m,
            DeviseId = devise.IdDevise,
            ProduitMutuelId = produitMutuel.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        var souscription = new SouscriptionPrestation
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            Statut = true
        };
        db.SouscriptionsPrestations.Add(souscription);
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Souscription,
            SouscriptionPrestationId = souscription.IdSouscriptionPrestation,
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            Montant = 200m,
            DeviseId = devise.IdDevise,
            Statut = true,
            StatutPaiement = "Valide"
        };

        var logger = new Mock<ILogger<CommissionService>>();
        var notificationService = new Mock<ICommissionNotificationService>();
        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);

        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var wallet = await db.WalletsAgents.FirstAsync(x => x.AgentId == agent.IdAgent);
        Assert.Equal(30m, wallet.SoldeCourant); // 200 * 15%

        var mouvement = await db.WalletMouvements.FirstAsync();
        Assert.Equal(30m, mouvement.Montant);
    }

    [Theory]
    [InlineData("AT", 10)]
    [InlineData("Agent (AT)", 10)]
    [InlineData("Superviseur", 10)]
    [InlineData("Chef d'équipe", 10)]
    [InlineData("AA", 20)]
    [InlineData("Agent (AA)", 20)]
    [InlineData("Percepteur", 20)]
    [InlineData("Caissier", 20)]
    [InlineData("Financier", 20)]
    [InlineData("IT", 20)]
    [InlineData("Admin", 20)]
    [InlineData("AAMash", 30)]
    [InlineData("AAStructure", 40)]
    [InlineData("RoleInconnu", 10)]
    public async Task ProcessCommissionAsync_ProduitMutuel_AppliqueTauxSelonRoleAgent(string roleAgent, decimal tauxAttendu)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var (agent, _, _, collecte) = await SeedSouscriptionCollecteAsync(
            db,
            matricule: "AG000000101",
            codeAdhesion: "ADH-PM-101",
            configureProduit: (devise, prestation) =>
            {
                var produitMutuel = new ProduitMutuel
                {
                    Nom = "PM Matrice Role",
                    Montant = 200m,
                    EstGratuit = false,
                    Periodicite = "Mensuel",
                    AgeMin = 0,
                    AgeMax = 120,
                    DeviseId = devise.IdDevise,
                    TauxCommissionAT = 10m,
                    TauxCommissionAA = 20m,
                    TauxCommissionAAMash = 30m,
                    TauxCommissionAAStructure = 40m,
                    Statut = true
                };
                db.ProduitsMutuels.Add(produitMutuel);
                db.SaveChanges();
                prestation.ProduitMutuelId = produitMutuel.IdProduit;
            },
            roleAgent: roleAgent);

        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);
        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var wallet = await db.WalletsAgents.FirstAsync(x => x.AgentId == agent.IdAgent);
        Assert.Equal(collecte.Montant * (tauxAttendu / 100m), wallet.SoldeCourant);
    }

    [Theory]
    [InlineData("AT", 11)]
    [InlineData("Chef d'équipe", 11)]
    [InlineData("Admin", 21)]
    [InlineData("AAMash", 31)]
    [InlineData("AAStructure", 41)]
    [InlineData("RoleInconnu", 11)]
    public async Task ProcessCommissionAsync_ProduitAssureur_AppliqueTauxSelonRoleAgent(string roleAgent, decimal tauxAttendu)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var (agent, _, _, collecte) = await SeedSouscriptionCollecteAsync(
            db,
            matricule: "AG000000201",
            codeAdhesion: "ADH-PA-201",
            configureProduit: (devise, prestation) =>
            {
                var assureur = new Assureur { Nom = "Assureur Matrice Role", Statut = true };
                db.Assureurs.Add(assureur);
                db.SaveChanges();

                var produitAssureur = new ProduitAssureur
                {
                    Nom = "PA Matrice Role",
                    Montant = 200m,
                    EstGratuit = false,
                    AssureurId = assureur.IdAssureur,
                    Periodicite = "Mensuel",
                    AgeMin = 0,
                    AgeMax = 120,
                    DeviseId = devise.IdDevise,
                    TauxCommissionAT = 11m,
                    TauxCommissionAA = 21m,
                    TauxCommissionAAMash = 31m,
                    TauxCommissionAAStructure = 41m,
                    Statut = true
                };
                db.ProduitsAssureurs.Add(produitAssureur);
                db.SaveChanges();
                prestation.ProduitAssureurId = produitAssureur.IdProduit;
            },
            roleAgent: roleAgent);

        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);
        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var wallet = await db.WalletsAgents.FirstAsync(x => x.AgentId == agent.IdAgent);
        Assert.Equal(collecte.Montant * (tauxAttendu / 100m), wallet.SoldeCourant);
    }

    [Fact]
    public async Task ProcessCommissionAsync_WhenRateCannotBeResolved_UsesFallback25Percent()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var devise = new Devise { Code = "CDF", Nom = "Franc Congolais", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);

        var agent = new Agent { NomComplet = "Agent C", Matricule = "AG000000003", Phone = "0990000003", Statut = true };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.WalletsAgents.Add(new WalletAgent { AgentId = agent.IdAgent, DeviseId = devise.IdDevise, SoldeCourant = 0m, SoldeDisponible = 0m, Statut = true });

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
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

        var affilie = new Affilie
        {
            CodeAdhesion = "ADH-003",
            Nom = "Fallback",
            Prenom = "Case",
            NomComplet = "Fallback Case",
            DateNaissance = new DateTime(1992, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = "user-fallback",
            MotDePasseHash = "hash",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            Statut = true
        };
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            StatutDossier = "A",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            UtilisateurId = utilisateur.IdUtilisateur,
            Statut = true
        });
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Souscription,
            SouscriptionPrestationId = 99999, // inexistant => fallback
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            Montant = 100m,
            DeviseId = devise.IdDevise,
            Statut = true,
            StatutPaiement = "Valide"
        };

        var logger = new Mock<ILogger<CommissionService>>();
        var notificationService = new Mock<ICommissionNotificationService>();
        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);

        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var wallet = await db.WalletsAgents.FirstAsync(x => x.AgentId == agent.IdAgent);
        Assert.Equal(25m, wallet.SoldeCourant);

        var mouvement = await db.WalletMouvements.FirstAsync();
        Assert.Equal(25m, mouvement.Montant);
    }

    [Fact]
    public async Task ProcessCommissionAsync_ProduitMutuelEstGratuit_AppliqueTauxZero()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var (agent, affilie, souscription, collecte) = await SeedSouscriptionCollecteAsync(
            db,
            matricule: "AG000000004",
            codeAdhesion: "ADH-004",
            configureProduit: (devise, prestation) =>
            {
                var produitMutuel = new ProduitMutuel
                {
                    Nom = "PM Gratuit",
                    Montant = 0m,
                    EstGratuit = true,
                    Periodicite = "Mensuel",
                    AgeMin = 0,
                    AgeMax = 120,
                    DeviseId = devise.IdDevise,
                    TauxCommissionAT = 15m,
                    Statut = true
                };
                db.ProduitsMutuels.Add(produitMutuel);
                db.SaveChanges();
                prestation.ProduitMutuelId = produitMutuel.IdProduit;
            });

        var logger = new Mock<ILogger<CommissionService>>();
        var notificationService = new Mock<ICommissionNotificationService>();
        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);

        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var wallet = await db.WalletsAgents.FirstAsync(x => x.AgentId == agent.IdAgent);
        Assert.Equal(0m, wallet.SoldeCourant);

        var mouvement = await db.WalletMouvements.SingleAsync();
        Assert.Equal(0m, mouvement.Montant);

        queue.Verify(
            x => x.QueueCommissionNotificationAsync(agent.IdAgent, 0m, collecte.IdCollecte, 0m, 0m),
            Times.Once);
    }

    [Fact]
    public async Task ProcessCommissionAsync_ProduitAssureurEstGratuit_AppliqueTauxZero()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var (agent, affilie, souscription, collecte) = await SeedSouscriptionCollecteAsync(
            db,
            matricule: "AG000000005",
            codeAdhesion: "ADH-005",
            configureProduit: (devise, prestation) =>
            {
                var assureur = new Assureur { Nom = "Assureur Test", Statut = true };
                db.Assureurs.Add(assureur);
                db.SaveChanges();

                var produitAssureur = new ProduitAssureur
                {
                    Nom = "PA Gratuit",
                    Montant = 0m,
                    EstGratuit = true,
                    AssureurId = assureur.IdAssureur,
                    Periodicite = "Mensuel",
                    AgeMin = 0,
                    AgeMax = 120,
                    DeviseId = devise.IdDevise,
                    TauxCommissionAT = 20m,
                    Statut = true
                };
                db.ProduitsAssureurs.Add(produitAssureur);
                db.SaveChanges();
                prestation.ProduitAssureurId = produitAssureur.IdProduit;
            });

        var logger = new Mock<ILogger<CommissionService>>();
        var notificationService = new Mock<ICommissionNotificationService>();
        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);

        var walletVirtuelPayment = new Mock<IWalletVirtuelPaymentService>();
        var service = CreateService(db, queue, arrieres, penalite, walletVirtuelPayment.Object);

        await service.ProcessCommissionAsync(collecte);

        var wallet = await db.WalletsAgents.FirstAsync(x => x.AgentId == agent.IdAgent);
        Assert.Equal(0m, wallet.SoldeCourant);

        var mouvement = await db.WalletMouvements.SingleAsync();
        Assert.Equal(0m, mouvement.Montant);

        queue.Verify(
            x => x.QueueCommissionNotificationAsync(agent.IdAgent, 0m, collecte.IdCollecte, 0m, 0m),
            Times.Once);
    }

    [Fact]
    public async Task ProcessCommissionAsync_VirtualAccountCrossDevise_DebiteMontantConverti()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var db = await CreateDbContextAsync(connection);

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc Congolais", EstDevisePrincipale = false, Statut = true };
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

        var (agent, affilie, souscription, collecte) = await SeedSouscriptionCollecteAsync(
            db,
            matricule: "AG000000006",
            codeAdhesion: "ADH-006",
            configureProduit: (devise, prestation) =>
            {
                var assureur = new Assureur { Nom = "Assureur Test", Statut = true };
                db.Assureurs.Add(assureur);
                db.SaveChanges();

                var produitAssureur = new ProduitAssureur
                {
                    Nom = "PA Virtuel",
                    Montant = 28500m,
                    EstGratuit = false,
                    AssureurId = assureur.IdAssureur,
                    Periodicite = "Mensuel",
                    AgeMin = 0,
                    AgeMax = 120,
                    DeviseId = devise.IdDevise,
                    TauxCommissionAT = 25m,
                    Statut = true
                };
                db.ProduitsAssureurs.Add(produitAssureur);
                db.SaveChanges();
                prestation.ProduitAssureurId = produitAssureur.IdProduit;
            });

        db.WalletsVirtuelsAgents.Add(new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = usd.IdDevise,
            SoldeVirtuel = 100m,
            Statut = true
        });

        collecte.Montant = 28500m;
        collecte.DeviseId = cdf.IdDevise;
        collecte.ModePaiement = "VIRTUAL_ACCOUNT";
        collecte.DateCollecte = new DateTime(2026, 5, 1);
        await db.SaveChangesAsync();

        var logger = new Mock<ILogger<CommissionService>>();
        var notificationService = new Mock<ICommissionNotificationService>();
        var queue = new Mock<INotificationQueueService>();
        var arrieres = new Mock<IArrieresAffilieService>();
        arrieres.Setup(x => x.ProcessCollecteForArrieresAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArrieresAffilie)null!);
        var penalite = new Mock<IPenaliteAffilieService>();
        penalite.Setup(x => x.ProcessCollecteForPenaliteAsync(It.IsAny<Collecte>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PenaliteAffilie)null!);

        var walletVirtuelPayment = new WalletVirtuelPaymentService(
            db,
            new DeviseConversionService(db),
            new WalletVirtuelMouvementService(db));

        var service = new CommissionService(
            db,
            logger.Object,
            notificationService.Object,
            queue.Object,
            arrieres.Object,
            penalite.Object,
            walletVirtuelPayment,
            new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>()),
            new DeviseConversionService(db));

        await service.ProcessCommissionAsync(collecte);

        var walletVirtuel = await db.WalletsVirtuelsAgents.SingleAsync(w => w.AgentId == agent.IdAgent);
        Assert.Equal(90m, walletVirtuel.SoldeVirtuel);

        var mouvementVirtuel = await db.WalletVirtuelMouvements.SingleAsync();
        Assert.Equal(10m, mouvementVirtuel.Montant);
    }

    private static async Task<(Agent Agent, Affilie Affilie, SouscriptionPrestation Souscription, Collecte Collecte)>
        SeedSouscriptionCollecteAsync(
            ProsocDbContext db,
            string matricule,
            string codeAdhesion,
            Action<Devise, Prestation> configureProduit,
            string? roleAgent = null)
    {
        var devise = new Devise { Code = "CDF", Nom = "Franc Congolais", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);

        var agent = new Agent
        {
            NomComplet = "Agent",
            Matricule = matricule,
            Phone = "0990000000",
            RoleAgent = roleAgent,
            Statut = true
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.WalletsAgents.Add(new WalletAgent { AgentId = agent.IdAgent, DeviseId = devise.IdDevise, SoldeCourant = 0m, SoldeDisponible = 0m, Statut = true });

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
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

        var affilie = new Affilie
        {
            CodeAdhesion = codeAdhesion,
            Nom = "Test",
            Prenom = "User",
            NomComplet = "Test User",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = $"user-{codeAdhesion}",
            MotDePasseHash = "hash",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            Statut = true
        };
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            StatutDossier = "A",
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            UtilisateurId = utilisateur.IdUtilisateur,
            Statut = true
        });

        var prestation = new Prestation
        {
            NomPrestation = "Presta test",
            Montant = 200m,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        configureProduit(devise, prestation);
        await db.SaveChangesAsync();

        var souscription = new SouscriptionPrestation
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            Statut = true
        };
        db.SouscriptionsPrestations.Add(souscription);
        await db.SaveChangesAsync();

        var collecte = new Collecte
        {
            TypeCollecte = TypeCollecte.Souscription,
            SouscriptionPrestationId = souscription.IdSouscriptionPrestation,
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            Montant = 200m,
            DeviseId = devise.IdDevise,
            Statut = true,
            StatutPaiement = "Valide"
        };
        db.Collectes.Add(collecte);
        await db.SaveChangesAsync();

        return (agent, affilie, souscription, collecte);
    }
}
