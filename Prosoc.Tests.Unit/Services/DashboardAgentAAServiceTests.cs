using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAgentAAServiceTests
{
    private static async Task<(ProsocDbContext Db, DashboardAgentAAService Service)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new DashboardAgentAAService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<DashboardAgentAAService>>().Object);

        return (db, service);
    }

    private static async Task<(int AgentCibleId, int AutreAgentId)> SeedAgentAaDataAsync(ProsocDbContext db)
    {
        var categorie = new CategorieAdhesion { Libelle = "Standard", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var typeAdhesion = new TypeAdhesion
        {
            Libelle = "Individuel",
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            MaxDependants = 0,
            Montant = 10m,
            Statut = true
        };
        db.TypeAdhesions.Add(typeAdhesion);

        var agentCible = new Agent
        {
            NomComplet = "Encodeur A",
            Matricule = "AA000000001",
            Phone = "0990000001",
            Statut = true
        };
        var autreAgent = new Agent
        {
            NomComplet = "Encodeur B",
            Matricule = "AA000000002",
            Phone = "0990000002",
            Statut = true
        };
        db.Agents.AddRange(agentCible, autreAgent);
        await db.SaveChangesAsync();

        var affilieEnAttente = new Affilie
        {
            CodeAdhesion = "AFF-AA-1",
            Nom = "Marie",
            Prenom = "Dupont",
            NomComplet = "Marie Dupont",
            DateNaissance = new DateTime(1988, 2, 10),
            Statut = true
        };
        var affilieValide = new Affilie
        {
            CodeAdhesion = "AFF-AA-2",
            Nom = "Jean",
            Prenom = "Dupont",
            NomComplet = "Jean Dupont",
            DateNaissance = new DateTime(1985, 4, 12),
            Statut = true
        };
        var affilieAutre = new Affilie
        {
            CodeAdhesion = "AFF-AA-3",
            Nom = "Luc",
            Prenom = "Martin",
            NomComplet = "Luc Martin",
            DateNaissance = new DateTime(1992, 7, 20),
            Statut = true
        };
        db.Affilies.AddRange(affilieEnAttente, affilieValide, affilieAutre);
        await db.SaveChangesAsync();

        var utilisateurEnAttente = new Utilisateur
        {
            NomUtilisateur = "user-aa-1",
            MotDePasseHash = "hash",
            AgentId = agentCible.IdAgent,
            AffilieId = affilieEnAttente.IdAffilie,
            Statut = true
        };
        var utilisateurValide = new Utilisateur
        {
            NomUtilisateur = "user-aa-2",
            MotDePasseHash = "hash",
            AgentId = agentCible.IdAgent,
            AffilieId = affilieValide.IdAffilie,
            Statut = true
        };
        var utilisateurAutre = new Utilisateur
        {
            NomUtilisateur = "user-aa-3",
            MotDePasseHash = "hash",
            AgentId = autreAgent.IdAgent,
            AffilieId = affilieAutre.IdAffilie,
            Statut = true
        };
        db.Utilisateurs.AddRange(utilisateurEnAttente, utilisateurValide, utilisateurAutre);
        await db.SaveChangesAsync();

        db.Adhesions.AddRange(
            new Adhesion
            {
                AgentId = agentCible.IdAgent,
                AffilieId = affilieEnAttente.IdAffilie,
                TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
                UtilisateurId = utilisateurEnAttente.IdUtilisateur,
                StatutDossier = "EN ATTENTE",
                Statut = true
            },
            new Adhesion
            {
                AgentId = agentCible.IdAgent,
                AffilieId = affilieValide.IdAffilie,
                TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
                UtilisateurId = utilisateurValide.IdUtilisateur,
                StatutDossier = AdhesionNiveau2Regles.StatutValide,
                Statut = true,
                DateModification = DateTime.Now
            },
            new Adhesion
            {
                AgentId = autreAgent.IdAgent,
                AffilieId = affilieAutre.IdAffilie,
                TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
                UtilisateurId = utilisateurAutre.IdUtilisateur,
                StatutDossier = "EN ATTENTE",
                Statut = true
            });

        db.Dependants.AddRange(
            new Dependant
            {
                Nom = "Enfant Marie",
                LienParente = "Enfant",
                AffilieId = affilieEnAttente.IdAffilie,
                Statut = true
            },
            new Dependant
            {
                Nom = "Enfant Luc",
                LienParente = "Enfant",
                AffilieId = affilieAutre.IdAffilie,
                Statut = true
            });

        db.Antecedants.AddRange(
            new Antecedant
            {
                Description = "Asthme",
                AffilieId = affilieEnAttente.IdAffilie,
                Statut = true
            },
            new Antecedant
            {
                Description = "Diabète",
                AffilieId = affilieAutre.IdAffilie,
                Statut = true
            });

        await db.SaveChangesAsync();

        return (agentCible.IdAgent, autreAgent.IdAgent);
    }

    [Fact]
    public async Task GetKpisAsync_MontantsConsolidesEnDevisePrincipale()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentCibleId, _) = await SeedAgentAaDataAsync(db);
            var affilieId = await db.Adhesions
                .Where(a => a.AgentId == agentCibleId)
                .Select(a => a.AffilieId)
                .FirstAsync();

            var usd = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
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

            var walletUsd = new WalletAgent { AgentId = agentCibleId, DeviseId = usd.IdDevise, Statut = true };
            var walletCdf = new WalletAgent { AgentId = agentCibleId, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.AddRange(walletUsd, walletCdf);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = affilieId,
                    AgentId = agentCibleId,
                    DeviseId = cdf.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 2850m,
                    MontantDevisePrincipale = 1m,
                    Statut = true,
                    DateCollecte = now
                },
                new Collecte
                {
                    AffilieId = affilieId,
                    AgentId = agentCibleId,
                    DeviseId = usd.IdDevise,
                    DevisePrincipaleId = usd.IdDevise,
                    TypeCollecte = TypeCollecte.Cotisation,
                    Montant = 10m,
                    MontantDevisePrincipale = 10m,
                    Statut = true,
                    DateCollecte = now
                });

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

            var kpis = await service.GetKpisAsync(agentCibleId);

            Assert.Equal(11m, kpis.TotalCollectesMois);
            Assert.Equal(2.10m, kpis.TotalCommissionsMois);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        }
    }

    [Fact]
    public async Task GetKpisAsync_ScopeDossiersDependantsEtAntecedentsParAgent()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentCibleId, _) = await SeedAgentAaDataAsync(db);

            var kpis = await service.GetKpisAsync(agentCibleId);

            Assert.Equal(2, kpis.TotalDossiers);
            Assert.Equal(1, kpis.DossiersEnAttente);
            Assert.Equal(1, kpis.DossiersValides);
            Assert.Equal(1, kpis.DossiersValidesMois);
            Assert.Equal(50m, kpis.TauxCompletion);
            Assert.Equal(1, kpis.TotalDependants);
            Assert.Equal(1, kpis.TotalAntecedents);
            Assert.Equal(1, kpis.DependantsAjoutesMois);
            Assert.Equal(1, kpis.AntecedentsAjoutesMois);
        }
    }

    [Fact]
    public async Task GetDossiersATraiterAsync_RetourneUniquementDossiersNonValides()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentCibleId, _) = await SeedAgentAaDataAsync(db);

            var dossiers = await service.GetDossiersATraiterAsync(agentCibleId);

            Assert.Single(dossiers);
            Assert.Equal("EN ATTENTE", dossiers[0].StatutDossier);
            Assert.Equal("AFF-AA-1", dossiers[0].CodeAdhesion);
            Assert.Equal(1, dossiers[0].NombreDependants);
            Assert.Equal(1, dossiers[0].NombreAntecedents);
        }
    }

    [Fact]
    public async Task GetDependantsEtAntecedentsRecentsAsync_FiltrentParAgent()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentCibleId, _) = await SeedAgentAaDataAsync(db);

            var dependants = await service.GetDependantsRecentsAsync(agentCibleId);
            var antecedents = await service.GetAntecedentsRecentsAsync(agentCibleId);

            Assert.Single(dependants);
            Assert.Equal("Enfant Marie", dependants[0].Nom);

            Assert.Single(antecedents);
            Assert.Equal("Asthme", antecedents[0].Description);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_InclutFileEncodeur()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentCibleId, _) = await SeedAgentAaDataAsync(db);

            var summary = await service.GetDashboardSummaryAsync(agentCibleId);

            Assert.Equal("Encodeur A", summary.NomAgent);
            Assert.Single(summary.DossiersATraiter);
            Assert.Single(summary.DependantsRecents);
            Assert.Single(summary.AntecedentsRecents);
            Assert.Equal(1, summary.Kpis.DossiersEnAttente);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ExposeDevisePrincipaleCode()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (agentCibleId, _) = await SeedAgentAaDataAsync(db);
            db.Devises.Add(new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true });
            await db.SaveChangesAsync();

            var summary = await service.GetDashboardSummaryAsync(agentCibleId);

            Assert.Equal("USD", summary.Kpis.DevisePrincipaleCode);
            Assert.Equal("USD", summary.DevisePrincipaleCode);
        }
    }
}
