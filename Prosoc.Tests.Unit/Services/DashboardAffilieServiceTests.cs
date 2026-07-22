using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAffilieServiceTests
{
    private static async Task<ProsocDbContext> CreateDbContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static DashboardAffilieService CreateService(ProsocDbContext db) =>
        new(db, new Mock<IAffilieConformiteService>().Object, new Mock<ILogger<DashboardAffilieService>>().Object);

    private static async Task RunAsync(Func<DashboardAffilieService, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await test(CreateService(db));
    }

    private static async Task<(Agent Agent, Affilie Affilie)> SeedAgentEtAffilieAsync(ProsocDbContext db)
    {
        var agent = new Agent
        {
            NomComplet = "Agent Test",
            Matricule = "AG000000099",
            Phone = "0990000099",
            Statut = true
        };
        db.Agents.Add(agent);

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF001",
            Nom = "Jean",
            Prenom = "Dupont",
            NomComplet = "Jean Dupont",
            DateNaissance = new DateTime(1980, 1, 1),
            DateCreation = new DateTime(2025, 3, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();
        return (agent, affilie);
    }

    private static async Task<Affilie> SeedAffilieAvecKpisAsync(ProsocDbContext db)
    {
        var (agent, affilie) = await SeedAgentEtAffilieAsync(db);
        var year = DateTime.Now.Year;

        var devise = new Devise { Code = "CDF", Nom = "Franc Congolais", Statut = true };
        db.Devises.Add(devise);
        await db.SaveChangesAsync();

        var produitMutuel = new ProduitMutuel
        {
            Nom = "Produit test",
            Montant = 100m,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = devise.IdDevise,
            TauxCommissionAT = 10m,
            Statut = true
        };
        db.ProduitsMutuels.Add(produitMutuel);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Prestation test",
            Montant = 100m,
            DeviseId = devise.IdDevise,
            ProduitMutuelId = produitMutuel.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        db.Collectes.AddRange(
            new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                Montant = 10000,
                DateCollecte = new DateTime(year, 1, 15),
                StatutPaiement = "PAYE",
                Statut = true
            },
            new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                Montant = 15000,
                DateCollecte = new DateTime(year, 2, 15),
                StatutPaiement = "PAYE",
                Statut = true
            },
            new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                Montant = 10000,
                DateCollecte = new DateTime(year, 3, 15),
                StatutPaiement = "PAYE",
                Statut = true
            });

        var souscription1 = new SouscriptionPrestation
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            DateCreation = new DateTime(year, 2, 10)
        };
        var souscription2 = new SouscriptionPrestation
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            DateCreation = new DateTime(year, 3, 5)
        };
        db.SouscriptionsPrestations.AddRange(souscription1, souscription2);
        await db.SaveChangesAsync();

        db.Collectes.AddRange(
            new Collecte
            {
                TypeCollecte = TypeCollecte.Souscription,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                SouscriptionPrestationId = souscription1.IdSouscriptionPrestation,
                Montant = 20000,
                DateCollecte = new DateTime(year, 2, 15),
                StatutPaiement = "PAYE",
                Statut = true
            },
            new Collecte
            {
                TypeCollecte = TypeCollecte.Souscription,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                SouscriptionPrestationId = souscription2.IdSouscriptionPrestation,
                Montant = 10000,
                DateCollecte = new DateTime(year, 3, 10),
                StatutPaiement = "PAYE",
                Statut = true
            });
        await db.SaveChangesAsync();

        return affilie;
    }

    #region KPIs Tests

    [Fact]
    public async Task GetAffilieKpisAsync_AffilieExistant_RetourneKPIs()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var service = CreateService(db);
        var affilie = await SeedAffilieAvecKpisAsync(db);

        var result = await service.GetAffilieKpisAsync(affilie.IdAffilie);

        Assert.NotNull(result);
        Assert.Equal(affilie.IdAffilie, result.IdAffilie);
        Assert.Equal("AFF001", result.CodeAdhesion);
        Assert.Equal("Jean Dupont", result.NomComplet);
        Assert.Equal(35000, result.TotalCotisations);
        Assert.Equal(30000, result.TotalPrestations);
        Assert.Equal(5000, result.SoldeTotal);
        Assert.Equal(5000, result.SoldeDisponible);
        Assert.Equal(2, result.NombrePrestations);
        Assert.Equal(10000, result.MontantDerniereCotisation);
        Assert.True(result.EstActif);
        Assert.True(result.AncienneteMois > 0);
    }

    [Fact]
    public async Task GetAffilieKpisAsync_MontantsConsolidesEnDevisePrincipale()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        var service = CreateService(db);
        var (agent, affilie) = await SeedAgentEtAffilieAsync(db);
        var year = DateTime.Now.Year;

        var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
        db.Devises.AddRange(usd, cdf);
        await db.SaveChangesAsync();

        db.Collectes.AddRange(
            new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = cdf.IdDevise,
                DevisePrincipaleId = usd.IdDevise,
                Montant = 2850m,
                MontantDevisePrincipale = 1m,
                DateCollecte = new DateTime(year, 1, 15),
                StatutPaiement = "PAYE",
                Statut = true
            },
            new Collecte
            {
                TypeCollecte = TypeCollecte.Cotisation,
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = usd.IdDevise,
                DevisePrincipaleId = usd.IdDevise,
                Montant = 10m,
                MontantDevisePrincipale = 10m,
                DateCollecte = new DateTime(year, 2, 15),
                StatutPaiement = "PAYE",
                Statut = true
            });
        await db.SaveChangesAsync();

        var result = await service.GetAffilieKpisAsync(affilie.IdAffilie);

        Assert.Equal(11m, result.TotalCotisations);
        Assert.Equal(10m, result.MontantDerniereCotisation);
        Assert.Equal("USD", result.DevisePrincipaleCode);
    }

    [Fact]
    public async Task GetAffilieKpisAsync_AffilieNonTrouve_RetourneKPIsVides() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAffilieKpisAsync(999);

            Assert.NotNull(result);
            Assert.Equal(0, result.IdAffilie);
            Assert.Equal(string.Empty, result.CodeAdhesion);
            Assert.Equal(string.Empty, result.NomComplet);
            Assert.Equal(0, result.TotalCotisations);
            Assert.Equal(0, result.TotalPrestations);
            Assert.Equal(0, result.NombrePrestations);
        });

    [Fact]
    public async Task GetAffilieKpisAsync_AucuneCotisation_RetourneKPIsZero()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        var service = CreateService(db);
        var (_, affilie) = await SeedAgentEtAffilieAsync(db);

        var result = await service.GetAffilieKpisAsync(affilie.IdAffilie);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCotisations);
        Assert.Equal(0, result.TotalPrestations);
        Assert.Equal(0, result.NombrePrestations);
        Assert.Equal(0, result.SoldeTotal);
        Assert.Equal(0, result.SoldeDisponible);
    }

    #endregion

    #region Informations Tests

    [Fact]
    public async Task GetAffilieInfoAsync_AffilieExistant_RetourneInfos()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        var service = CreateService(db);

        var agent = new Agent
        {
            NomComplet = "Agent Test",
            Matricule = "AG000000098",
            Phone = "0990000098",
            Statut = true
        };
        db.Agents.Add(agent);

        var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var typeAdhesion = new TypeAdhesion
        {
            Libelle = "Premium",
            MaxDependants = 0,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            Montant = 1m,
            Statut = true
        };
        db.TypeAdhesions.Add(typeAdhesion);
        await db.SaveChangesAsync();

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF001",
            Nom = "Jean",
            Prenom = "Dupont",
            NomComplet = "Jean Dupont",
            Telephone = "+243123456789",
            DateNaissance = new DateTime(1980, 1, 1),
            PhotoData = new byte[] { 0xFF },
            DateCreation = new DateTime(2025, 3, 1),
            Statut = true,
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Lemba",
            QuartierResidence = "Salongo",
            AvenueResidence = "ByPass",
            NumeroResidence = "123",
            CommuneActivite = "Limete",
            QuartierActivite = "Victoire",
            AvenueActivite = "Lumumba",
            NumeroActivite = "456"
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = "affilie-dashboard-test",
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
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            AgentId = agent.IdAgent,
            UtilisateurId = utilisateur.IdUtilisateur,
            StatutDossier = "A",
            Statut = true
        });
        await db.SaveChangesAsync();

        var result = await service.GetAffilieInfoAsync(affilie.IdAffilie);

        Assert.NotNull(result);
        Assert.Equal(affilie.IdAffilie, result.IdAffilie);
        Assert.Equal("AFF001", result.CodeAdhesion);
        Assert.Equal("Jean Dupont", result.NomComplet);
        Assert.Equal("+243123456789", result.Telephone);
        Assert.Equal($"/api/affilie/{affilie.IdAffilie}/photo", result.PhotoUrl);
        Assert.Equal("Kinshasa", result.ProvinceResidence);
        Assert.Equal("Lemba", result.CommuneResidence);
        Assert.Equal("Salongo", result.QuartierResidence);
        Assert.Equal("ByPass", result.AvenueResidence);
        Assert.Equal("123", result.NumeroResidence);
        Assert.Equal("Limete", result.CommuneActivite);
        Assert.Equal("Victoire", result.QuartierActivite);
        Assert.Equal("Lumumba", result.AvenueActivite);
        Assert.Equal("456", result.NumeroActivite);
        Assert.Equal("Premium", result.TypeAdhesion);
        Assert.True(result.EstActif);
    }

    [Fact]
    public async Task GetAffilieInfoAsync_AffilieNonTrouve_RetourneInfosVides() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAffilieInfoAsync(999);

            Assert.NotNull(result);
            Assert.Equal(0, result.IdAffilie);
            Assert.Equal(string.Empty, result.CodeAdhesion);
            Assert.Equal(string.Empty, result.NomComplet);
        });

    #endregion

    #region Dashboard Complet Tests

    [Fact]
    public async Task GetDashboardResumeAsync_AffilieExistant_RetourneDashboardComplet()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        var service = CreateService(db);
        var affilie = await SeedAffilieAvecKpisAsync(db);
        var annee = DateTime.Now.Year;

        var result = await service.GetDashboardResumeAsync(affilie.IdAffilie, annee);

        Assert.NotNull(result);
        Assert.NotNull(result.Kpis);
        Assert.NotNull(result.Informations);
        Assert.NotNull(result.CotisationsRecentes);
        Assert.NotNull(result.PrestationsRecentes);
        Assert.NotNull(result.Beneficiaires);
        Assert.NotNull(result.Graphiques);
        Assert.NotNull(result.NotificationsRecentes);
        Assert.NotNull(result.DocumentsEnAttente);
        Assert.NotNull(result.Preferences);
        Assert.Equal(affilie.IdAffilie, result.Kpis.IdAffilie);
        Assert.Equal("AFF001", result.Kpis.CodeAdhesion);
        Assert.Equal("Jean Dupont", result.Kpis.NomComplet);
        Assert.Equal(affilie.IdAffilie, result.Informations.IdAffilie);
        Assert.Equal("AFF001", result.Informations.CodeAdhesion);
    }

    #endregion

    #region Tests de Préférences

    [Fact]
    public async Task GetPreferencesAsync_RetournePreferencesDefaut() =>
        await RunAsync(async service =>
        {
            var affilieId = 1;
            var result = await service.GetPreferencesAsync(affilieId);

            Assert.NotNull(result);
            Assert.Equal(affilieId, result.IdAffilie);
            Assert.Equal("fr", result.LanguePreferee);
            Assert.Equal("UTC", result.FuseauHoraire);
            Assert.Equal("PDF", result.FormatRapports);
        });

    [Fact]
    public async Task UpdatePreferencesAsync_RetourneVrai() =>
        await RunAsync(async service =>
        {
            var affilieId = 1;
            var preferences = new AffiliePreferencesDto
            {
                IdAffilie = affilieId,
                NotificationsEmail = false,
                NotificationsSMS = true,
                LanguePreferee = "en",
                FuseauHoraire = "UTC+1",
                RecevoirRappelsCotisation = false,
                RecevoirAlertesPrestation = true,
                RecevoirNewsletter = true,
                FrequenceRappelsJours = 15,
                EmailSecondaire = "test@example.com",
                TelephoneSecondaire = "+243987654321",
                ModeSombre = true,
                FormatRapports = "EXCEL",
                PartagerDonneesStatistiques = false
            };

            var result = await service.UpdatePreferencesAsync(affilieId, preferences);
            Assert.True(result);
        });

    #endregion

    #region Tests de Notifications

    [Fact]
    public async Task GetNotificationsAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetNotificationsAsync(1, 20);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task GetNotificationsNonLuesCountAsync_RetourneZero() =>
        await RunAsync(async service =>
        {
            var result = await service.GetNotificationsNonLuesCountAsync(1);
            Assert.Equal(0, result);
        });

    [Fact]
    public async Task MarquerNotificationLueAsync_RetourneVrai() =>
        await RunAsync(async service =>
        {
            var result = await service.MarquerNotificationLueAsync(1);
            Assert.True(result);
        });

    #endregion

    #region Tests de Documents

    [Fact]
    public async Task GetDocumentsAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetDocumentsAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task GetDocumentsEnAttenteAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetDocumentsEnAttenteAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task TelechargerDocumentAsync_RetourneVrai() =>
        await RunAsync(async service =>
        {
            var result = await service.TelechargerDocumentAsync(1);
            Assert.True(result);
        });

    #endregion

    #region Tests de Bénéficiaires

    [Fact]
    public async Task GetBeneficiairesAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetBeneficiairesAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    #endregion

    #region Tests de Graphiques

    [Fact]
    public async Task GetGraphiquesAsync_RetourneGraphiquesVides() =>
        await RunAsync(async service =>
        {
            var result = await service.GetGraphiquesAsync(1, 2026);

            Assert.NotNull(result);
            Assert.NotNull(result.CotisationsMensuelles);
            Assert.NotNull(result.PrestationsMensuelles);
            Assert.NotNull(result.EvolutionSolde);
            Assert.NotNull(result.RepartitionPrestations);
            Assert.NotNull(result.TauxUtilisationMensuel);
            Assert.NotNull(result.ResumeAnnuel);
            Assert.Empty(result.CotisationsMensuelles);
            Assert.Empty(result.PrestationsMensuelles);
            Assert.Empty(result.EvolutionSolde);
            Assert.Empty(result.RepartitionPrestations);
            Assert.Empty(result.TauxUtilisationMensuel);
        });

    #endregion

    #region Tests de Résumé Annuel

    [Fact]
    public async Task GetResumeAnnuelAsync_RetourneResumeDefaut() =>
        await RunAsync(async service =>
        {
            var annee = 2026;
            var result = await service.GetResumeAnnuelAsync(1, annee);

            Assert.NotNull(result);
            Assert.Equal(annee, result.Annee);
            Assert.Equal(0, result.TotalCotisations);
            Assert.Equal(0, result.TotalPrestations);
            Assert.Equal(0, result.SoldeFinAnnee);
            Assert.Equal(0, result.SoldeDebutAnnee);
            Assert.Equal(0, result.VariationAnnuelle);
            Assert.Equal(0, result.VariationPourcentage);
            Assert.Equal(0, result.TotalCotisationsEffectuees);
            Assert.Equal(0, result.TotalPrestationsRecues);
            Assert.Equal(0, result.TauxUtilisationMoyen);
            Assert.Equal(0, result.TauxRemboursementMoyen);
        });

    #endregion

    #region Tests d'Export

    [Fact]
    public async Task ExporterCotisationsAsync_RetourneByteArrayVide() =>
        await RunAsync(async service =>
        {
            var result = await service.ExporterCotisationsAsync(1, 3, 2026, "PDF");
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task ExporterPrestationsAsync_RetourneByteArrayVide() =>
        await RunAsync(async service =>
        {
            var result = await service.ExporterPrestationsAsync(1, 3, 2026, "EXCEL");
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task ExporterDashboardAsync_RetourneByteArrayVide() =>
        await RunAsync(async service =>
        {
            var result = await service.ExporterDashboardAsync(1, 2026, "CSV");
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    #endregion

    #region Tests d'Alertes

    [Fact]
    public async Task GetAlertesCotisationAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAlertesCotisationAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task GetAlertesPrestationAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAlertesPrestationAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task GetAlertesDocumentAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAlertesDocumentAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    [Fact]
    public async Task GetAlertesExpirationAsync_RetourneListeVide() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAlertesExpirationAsync(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        });

    #endregion

    #region Tests Utilitaires

    [Fact]
    public async Task GetAgeAffilieAsync_RetourneZero() =>
        await RunAsync(async service =>
        {
            var result = await service.GetAgeAffilieAsync(1);
            Assert.Equal(0, result);
        });

    [Fact]
    public async Task EstAffilieActifAsync_RetourneVrai() =>
        await RunAsync(async service =>
        {
            var result = await service.EstAffilieActifAsync(1);
            Assert.True(result);
        });

    [Fact]
    public async Task GetPlafondRestantAsync_RetourneZero() =>
        await RunAsync(async service =>
        {
            var result = await service.GetPlafondRestantAsync(1, 2026);
            Assert.Equal(0, result);
        });

    [Fact]
    public async Task GetDateDerniereActiviteAsync_RetourneDateActuelle() =>
        await RunAsync(async service =>
        {
            var result = await service.GetDateDerniereActiviteAsync(1);
            Assert.True(result > DateTime.MinValue);
        });

    #endregion
}
