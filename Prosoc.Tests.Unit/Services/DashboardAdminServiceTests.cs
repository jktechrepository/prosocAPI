using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAdminServiceTests
{
    private static async Task RunAsync(Func<DashboardAdminService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new DashboardAdminService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<DashboardAdminService>>().Object);

        await test(service, db);
    }

    [Fact]
    public async Task GetKpisAsync_CollectesEnAttente_UniquementEnAttenteFlexPay()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);

            db.Collectes.AddRange(
                await CreateCollecteAsync(db, CollecteStatutPaiement.Valide, 100m, DateTime.Now),
                await CreateCollecteAsync(db, CollecteStatutPaiement.Valide, 200m, DateTime.Now),
                await CreateCollecteAsync(db, CollecteStatutPaiement.EnAttente, 50m, DateTime.Now));
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(1, kpis.CollectesEnAttente);

            var liste = await service.GetCollectesEnAttenteAsync();
            Assert.Single(liste);
            Assert.Equal(CollecteStatutPaiement.EnAttente, liste[0].StatutPaiement);
        });
    }

    [Fact]
    public async Task GetKpisAsync_ProgressionCollectesMois_100QuandPasDeMoisPrecedent()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);

            db.Collectes.Add(await CreateCollecteAsync(db, CollecteStatutPaiement.Valide, 500m, DateTime.Now));
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(100, kpis.ProgressionCollectesMois);
        });
    }

    [Fact]
    public async Task GetKpisAsync_ProgressionCollectesMois_CompareMtdVsMtd_IgnoreFinMoisPrecedent()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);

            var now = DateTime.Now;
            var debutMois = new DateTime(now.Year, now.Month, 1);
            var debutMoisPrecedent = debutMois.AddMonths(-1);
            var jourEquivalent = Math.Min(
                now.Day,
                DateTime.DaysInMonth(debutMoisPrecedent.Year, debutMoisPrecedent.Month));

            // MTD courant : 100
            db.Collectes.Add(await CreateCollecteAsync(
                db, CollecteStatutPaiement.Valide, 100m, debutMois.AddHours(12)));

            // Même fenêtre MTD mois précédent : 100
            db.Collectes.Add(await CreateCollecteAsync(
                db,
                CollecteStatutPaiement.Valide,
                100m,
                debutMoisPrecedent.AddDays(jourEquivalent - 1).AddHours(12)));

            // Après le jour équivalent du mois précédent (hors MTD) : 900
            // Ancien calcul (mois plein) aurait donné une forte baisse ; MTD vs MTD → 0 %
            var dernierJourMoisPrecedent = DateTime.DaysInMonth(
                debutMoisPrecedent.Year, debutMoisPrecedent.Month);
            if (jourEquivalent < dernierJourMoisPrecedent)
            {
                db.Collectes.Add(await CreateCollecteAsync(
                    db,
                    CollecteStatutPaiement.Valide,
                    900m,
                    new DateTime(
                        debutMoisPrecedent.Year,
                        debutMoisPrecedent.Month,
                        dernierJourMoisPrecedent,
                        12, 0, 0)));
            }

            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(100m, kpis.TotalCollectesMois);
            Assert.Equal(0, kpis.ProgressionCollectesMois);
        });
    }

    [Fact]
    public async Task GetKpisAsync_ProgressionCollectesMois_BaisseReelleSurFenetreMtd()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);

            var now = DateTime.Now;
            var debutMois = new DateTime(now.Year, now.Month, 1);
            var debutMoisPrecedent = debutMois.AddMonths(-1);
            var jourEquivalent = Math.Min(
                now.Day,
                DateTime.DaysInMonth(debutMoisPrecedent.Year, debutMoisPrecedent.Month));

            db.Collectes.Add(await CreateCollecteAsync(
                db, CollecteStatutPaiement.Valide, 100m, debutMois.AddHours(12)));
            db.Collectes.Add(await CreateCollecteAsync(
                db,
                CollecteStatutPaiement.Valide,
                200m,
                debutMoisPrecedent.AddDays(jourEquivalent - 1).AddHours(12)));
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(-50, kpis.ProgressionCollectesMois);
        });
    }

    [Fact]
    public async Task GetKpisAsync_NouvellesAdhesionsAujourdhui_IgnoreAdhesionsInactives()
    {
        await RunAsync(async (service, db) =>
        {
            var agent = new Agent
            {
                NomComplet = "Agent Adh",
                Matricule = "AG000000002",
                Phone = "0990000002",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilieActif = new Affilie
            {
                CodeAdhesion = "AFF-ADM-2",
                Nom = "Test",
                Prenom = "Actif",
                NomComplet = "Test Actif",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            var affilieInactif = new Affilie
            {
                CodeAdhesion = "AFF-ADM-3",
                Nom = "Test",
                Prenom = "Inactif",
                NomComplet = "Test Inactif",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.AddRange(affilieActif, affilieInactif);

            var typeId = await SeedTypeAdhesionAsync(db, skipAgent: true);
            var userId = await SeedUtilisateurAsync(db);
            await db.SaveChangesAsync();

            db.Adhesions.AddRange(
                new Adhesion
                {
                    AgentId = agent.IdAgent,
                    AffilieId = affilieActif.IdAffilie,
                    TypeAdhesionId = typeId,
                    UtilisateurId = userId,
                    StatutDossier = "EN ATTENTE",
                    Statut = true,
                    DateCreation = DateTime.Now
                },
                new Adhesion
                {
                    AgentId = agent.IdAgent,
                    AffilieId = affilieInactif.IdAffilie,
                    TypeAdhesionId = typeId,
                    UtilisateurId = userId,
                    StatutDossier = "EN ATTENTE",
                    Statut = false,
                    DateCreation = DateTime.Now
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(1, kpis.NouvellesAdhesionsAujourdhui);
        });
    }

    [Fact]
    public void EstEnAttente_ExclutValideEtLegacy()
    {
        Assert.True(CollecteStatutPaiementRegles.EstEnAttente(CollecteStatutPaiement.EnAttente));
        Assert.False(CollecteStatutPaiementRegles.EstEnAttente(CollecteStatutPaiement.Valide));
        Assert.False(CollecteStatutPaiementRegles.EstEnAttente("PAYE"));
    }

    [Fact]
    public async Task GetKpisAsync_TotalCollectesMois_SommeMontantDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);
            var ids = await GetCollecteIdsAsync(db);

            var usd = await db.Devises.FirstAsync(d => d.Code == "USD");
            usd.EstDevisePrincipale = true;
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                },
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(11m, kpis.TotalCollectesMois);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        });
    }

    [Fact]
    public async Task GetKpisAsync_TotalCommissionsMois_SommeEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);

            var usd = await db.Devises.FirstAsync(d => d.Code == "USD");
            usd.EstDevisePrincipale = true;

            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
            db.Devises.Add(cdf);
            await db.SaveChangesAsync();

            db.TauxChangeDevises.Add(new TauxChangeDevise
            {
                DeviseSourceId = usd.IdDevise,
                DeviseCibleId = cdf.IdDevise,
                Taux = 2850m,
                DateEffet = new DateTime(2026, 1, 1),
                Statut = true
            });

            var agentId = await db.Agents.Select(a => a.IdAgent).FirstAsync();
            var walletUsd = new WalletAgent { AgentId = agentId, DeviseId = usd.IdDevise, Statut = true };
            var walletCdf = new WalletAgent { AgentId = agentId, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.AddRange(walletUsd, walletCdf);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
            db.WalletMouvements.AddRange(
                new WalletMouvement
                {
                    WalletId = walletCdf.IdWalletAgent,
                    DeviseId = cdf.IdDevise,
                    Montant = 285m,
                    TypeOperation = "CREDIT",
                    Source = "COMM_COLLECTE",
                    DateOperation = now
                },
                new WalletMouvement
                {
                    WalletId = walletUsd.IdWalletAgent,
                    DeviseId = usd.IdDevise,
                    Montant = 2m,
                    TypeOperation = "CREDIT",
                    Source = "COMM_COLLECTE",
                    DateOperation = now
                });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync();
            Assert.Equal(2.10m, kpis.TotalCommissionsMois);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        });
    }

    [Fact]
    public async Task GetTopAgentsAsync_TotalCollectes_SommeMontantDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);
            var ids = await GetCollecteIdsAsync(db);

            var usd = await db.Devises.FirstAsync(d => d.Code == "USD");
            usd.EstDevisePrincipale = true;
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                },
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                });
            await db.SaveChangesAsync();

            var topAgents = await service.GetTopAgentsAsync(5);
            Assert.Single(topAgents);
            Assert.Equal(11m, topAgents[0].TotalCollectes);
        });
    }

    [Fact]
    public async Task GetTopAgentsAsync_IgnoreCollectesSansAgentId_NeCrashPas()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);
            var ids = await GetCollecteIdsAsync(db);

            var usd = await db.Devises.FirstAsync(d => d.Code == "USD");
            usd.EstDevisePrincipale = true;
            await db.SaveChangesAsync();

            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = null,
                    DeviseId = ids.DeviseId,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 999m,
                    MontantDevisePrincipale = 999m,
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                },
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 25m,
                    MontantDevisePrincipale = 25m,
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                });

            db.Adhesions.Add(new Adhesion
            {
                AffilieId = ids.AffilieId,
                AgentId = null,
                TypeAdhesionId = await SeedTypeAdhesionAsync(db, skipAgent: true),
                Statut = true,
                StatutDossier = "EN ATTENTE",
                DateCreation = DateTime.Now
            });
            await db.SaveChangesAsync();

            var topAgents = await service.GetTopAgentsAsync(5);

            Assert.NotEmpty(topAgents);
            var agent = Assert.Single(topAgents, a => a.AgentId == ids.AgentId);
            Assert.Equal(25m, agent.TotalCollectes);
            Assert.Equal(1, agent.NombreCollectes);
        });
    }

    [Fact]
    public async Task GetCollectesEnAttenteAsync_Montant_EnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);
            var ids = await GetCollecteIdsAsync(db);

            var usd = await db.Devises.FirstAsync(d => d.Code == "USD");
            usd.EstDevisePrincipale = true;
            await db.SaveChangesAsync();

            db.Collectes.Add(new Collecte
            {
                AffilieId = ids.AffilieId,
                AgentId = ids.AgentId,
                DeviseId = ids.DeviseId,
                DevisePrincipaleId = usd.IdDevise,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 2850m,
                MontantDevisePrincipale = 1m,
                Mois = DateTime.Now.Month,
                Annee = DateTime.Now.Year,
                StatutPaiement = CollecteStatutPaiement.EnAttente,
                Statut = true,
                DateCollecte = DateTime.Now,
                DateCreation = DateTime.Now
            });
            await db.SaveChangesAsync();

            var liste = await service.GetCollectesEnAttenteAsync();
            Assert.Single(liste);
            Assert.Equal(1m, liste[0].Montant);
        });
    }

    [Fact]
    public async Task GetTopAgentsAsync_ArronditMontantsEtScoreSurDeuxDecimales()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);
            var ids = await GetCollecteIdsAsync(db);

            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 1m,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                },
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2m,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                },
                new Collecte
                {
                    AffilieId = ids.AffilieId,
                    AgentId = ids.AgentId,
                    DeviseId = ids.DeviseId,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 7m,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    DateCollecte = DateTime.Now,
                    DateCreation = DateTime.Now
                });
            await db.SaveChangesAsync();

            var agents = await service.GetTopAgentsAsync(5);
            Assert.Single(agents);

            Assert.Equal(3.33m, agents[0].MontantMoyenCollecte);
            Assert.True(CountDecimalPlaces(agents[0].MontantMoyenCollecte) <= 2);
            Assert.True(CountDecimalPlaces(agents[0].TotalCollectes) <= 2);
            Assert.True(CountDecimalPlaces(agents[0].ScorePerformance) <= 2);
        });
    }

    [Fact]
    public async Task GetCollectesEnAttenteAsync_ArronditMontantEtHeuresAttenteSurDeuxDecimales()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedCollecteDependenciesAsync(db);
            var ids = await GetCollecteIdsAsync(db);

            db.Collectes.Add(new Collecte
            {
                AffilieId = ids.AffilieId,
                AgentId = ids.AgentId,
                DeviseId = ids.DeviseId,
                TypeCollecte = TypeCollecte.Cotisation,
                Montant = 1.239m,
                StatutPaiement = CollecteStatutPaiement.EnAttente,
                Statut = true,
                DateCollecte = DateTime.Now.AddMinutes(-92),
                DateCreation = DateTime.Now
            });
            await db.SaveChangesAsync();

            var collectes = await service.GetCollectesEnAttenteAsync();
            Assert.Single(collectes);

            Assert.Equal(1.24m, collectes[0].Montant);
            Assert.True(CountDecimalPlaces(collectes[0].Montant) <= 2);
            Assert.True(CountDecimalPlaces(collectes[0].HeuresAttente) <= 2);
        });
    }

    [Fact]
    public void CalculerProgressionCollectesMois_SuitReglesMetier()
    {
        Assert.Equal(100, CollecteStatutPaiementRegles.CalculerProgressionCollectesMois(100, 0));
        Assert.Equal(0, CollecteStatutPaiementRegles.CalculerProgressionCollectesMois(0, 0));
        Assert.Equal(100, CollecteStatutPaiementRegles.CalculerProgressionCollectesMois(200, 100));
    }

    private static async Task SeedCollecteDependenciesAsync(ProsocDbContext db)
    {
        var agent = new Agent
        {
            NomComplet = "Agent Test",
            Matricule = "AG000000001",
            Phone = "0990000001",
            Statut = true
        };
        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-ADM-1",
            Nom = "Test",
            Prenom = "Admin",
            NomComplet = "Test Admin",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        var devise = new Devise { Code = "USD", Nom = "Dollar", Statut = true };

        db.Agents.Add(agent);
        db.Affilies.Add(affilie);
        db.Devises.Add(devise);
        await db.SaveChangesAsync();
    }

    private static async Task<(int AgentId, int AffilieId, int DeviseId)> GetCollecteIdsAsync(ProsocDbContext db)
    {
        var agentId = await db.Agents.Select(a => a.IdAgent).FirstAsync();
        var affilieId = await db.Affilies.Select(a => a.IdAffilie).FirstAsync();
        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();
        return (agentId, affilieId, deviseId);
    }

    private static async Task<Collecte> CreateCollecteAsync(
        ProsocDbContext db,
        string statutPaiement,
        decimal montant,
        DateTime dateCollecte)
    {
        var ids = await GetCollecteIdsAsync(db);
        return new Collecte
        {
            AffilieId = ids.AffilieId,
            AgentId = ids.AgentId,
            DeviseId = ids.DeviseId,
            TypeCollecte = TypeCollecte.Cotisation,
            Montant = montant,
            Mois = dateCollecte.Month,
            Annee = dateCollecte.Year,
            StatutPaiement = statutPaiement,
            Statut = true,
            DateCollecte = dateCollecte,
            DateCreation = dateCollecte
        };
    }

    private static async Task<int> SeedTypeAdhesionAsync(ProsocDbContext db, bool skipAgent = false)
    {
        if (!skipAgent)
        {
            db.Agents.Add(new Agent
            {
                NomComplet = "Agent Adh",
                Matricule = "AG000000003",
                Phone = "0990000003",
                Statut = true
            });
        }

        var categorie = new CategorieAdhesion { Libelle = "Std", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var deviseId = await db.Devises.Select(d => d.IdDevise).FirstAsync();

        var type = new TypeAdhesion
        {
            Libelle = "Ind",
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            Montant = 10m,
            DeviseId = deviseId,
            Statut = true
        };
        db.TypeAdhesions.Add(type);
        await db.SaveChangesAsync();
        return type.IdTypeAdhesion;
    }

    private static async Task<int> SeedUtilisateurAsync(ProsocDbContext db)
    {
        var user = new ProsocAPI.Models.Authentication.Utilisateur
        {
            NomUtilisateur = "admin-kpi-test",
            MotDePasseHash = "hash",
            Statut = true
        };
        db.Utilisateurs.Add(user);
        await db.SaveChangesAsync();
        return user.IdUtilisateur;
    }

    private static int CountDecimalPlaces(decimal value)
    {
        var normalized = value / 1.0000000000000000000000000000m;
        return (decimal.GetBits(normalized)[3] >> 16) & 31;
    }
}
