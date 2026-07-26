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

public class DashboardFinancierServiceTests
{
    private static async Task RunAsync(Func<DashboardFinancierService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new DashboardFinancierService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<DashboardFinancierService>>().Object);

        await test(service, db);
    }

    [Fact]
    public async Task GetKpisFinanciersAsync_MontantsConsolidesEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
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

            var agent = new Agent
            {
                NomComplet = "Agent Fin",
                Matricule = "AG000000050",
                Phone = "0990000050",
                Statut = true
            };
            db.Agents.Add(agent);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-FIN-1",
                Nom = "Test",
                Prenom = "Fin",
                NomComplet = "Test Fin",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var walletUsd = new WalletAgent { AgentId = agent.IdAgent, DeviseId = usd.IdDevise, Statut = true };
            var walletCdf = new WalletAgent { AgentId = agent.IdAgent, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.AddRange(walletUsd, walletCdf);
            await db.SaveChangesAsync();

            var now = DateTime.Now;
            db.Collectes.AddRange(
                new Collecte
                {
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
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
                    AffilieId = affilie.IdAffilie,
                    AgentId = agent.IdAgent,
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

            var kpis = await service.GetKpisFinanciersAsync();

            Assert.Equal(11m, kpis.MontantTotalCollectes);
            Assert.Equal(11m, kpis.ChiffreAffairesTotal);
            Assert.Equal(2.10m, kpis.MontantTotalCommissions);
            Assert.Equal("USD", kpis.CodeDeviseConsolidation);
        });
    }

    [Fact]
    public async Task GetCommissionsAgentsAsync_SommeCommissionsEnDevisePrincipale()
    {
        await RunAsync(async (service, db) =>
        {
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

            var agent = new Agent
            {
                NomComplet = "Agent Comm",
                Matricule = "AG000000051",
                Phone = "0990000051",
                Statut = true
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            var walletCdf = new WalletAgent { AgentId = agent.IdAgent, DeviseId = cdf.IdDevise, Statut = true };
            db.WalletsAgents.Add(walletCdf);
            await db.SaveChangesAsync();

            db.WalletMouvements.Add(new WalletMouvement
            {
                WalletId = walletCdf.IdWalletAgent,
                DeviseId = cdf.IdDevise,
                Montant = 2850m,
                TypeOperation = "CREDIT",
                Source = "COMM_COLLECTE",
                DateOperation = DateTime.Now
            });
            await db.SaveChangesAsync();

            var commissions = await service.GetCommissionsAgentsAsync();
            var agentCommission = commissions.Single(c => c.AgentId == agent.IdAgent);

            Assert.Equal(1m, agentCommission.MontantCommission);
        });
    }

    [Fact]
    public async Task GetObjectifsAgentsAsync_SyntheseEtDetail_ExclutSansTargetEtHorsPeriode()
    {
        await RunAsync(async (service, db) =>
        {
            var atRole = new Role { Nom = "Agent (AT)", Code = "AT", Statut = true };
            var caRole = new Role { Nom = "Caissier", Code = "CA", Statut = true };
            db.Roles.AddRange(atRole, caRole);
            await db.SaveChangesAsync();

            db.TargetsAgents.Add(new TargetAgent
            {
                RoleId = atRole.IdRole,
                LibelleTarget = "Objectif mensuel AT",
                Periodicite = PeriodiciteTarget.Mensuelle,
                Nombre = 10,
                Statut = true,
                DateCreation = new DateTime(2026, 1, 1)
            });
            // Target non mensuel / autre rôle : ne doit pas inclure le caissier
            db.TargetsAgents.Add(new TargetAgent
            {
                RoleId = caRole.IdRole,
                LibelleTarget = "Objectif journalier CA",
                Periodicite = PeriodiciteTarget.Journaliere,
                Nombre = 50,
                Statut = true,
                DateCreation = new DateTime(2026, 1, 1)
            });
            await db.SaveChangesAsync();

            var agentAt1 = new Agent
            {
                NomComplet = "AT Alpha",
                Matricule = "AT000000201",
                Phone = "0990000201",
                RoleAgent = "Agent (AT)",
                Statut = true
            };
            var agentAt2 = new Agent
            {
                NomComplet = "AT Beta",
                Matricule = "AT000000202",
                Phone = "0990000202",
                RoleAgent = "Agent (AT)",
                Statut = true
            };
            var agentSansTarget = new Agent
            {
                NomComplet = "CA Sans Target Mensuel",
                Matricule = "CA000000203",
                Phone = "0990000203",
                RoleAgent = "Caissier",
                Statut = true
            };
            var agentInactif = new Agent
            {
                NomComplet = "AT Inactif",
                Matricule = "AT000000204",
                Phone = "0990000204",
                RoleAgent = "Agent (AT)",
                Statut = false
            };
            db.Agents.AddRange(agentAt1, agentAt2, agentSansTarget, agentInactif);
            await db.SaveChangesAsync();

            db.Utilisateurs.AddRange(
                new Utilisateur
                {
                    NomUtilisateur = "at1",
                    EmailUtilisateur = "at1@local.test",
                    PhoneUtilisateur = "0880000201",
                    MotDePasseHash = "hash",
                    RoleId = atRole.IdRole,
                    AgentId = agentAt1.IdAgent,
                    Statut = true
                },
                new Utilisateur
                {
                    NomUtilisateur = "at2",
                    EmailUtilisateur = "at2@local.test",
                    PhoneUtilisateur = "0880000202",
                    MotDePasseHash = "hash",
                    RoleId = atRole.IdRole,
                    AgentId = agentAt2.IdAgent,
                    Statut = true
                });

            var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
            db.Devises.Add(devise);
            var categorie = new CategorieAdhesion { Libelle = "Standard", Statut = true };
            db.CategoriesAdhesions.Add(categorie);
            await db.SaveChangesAsync();

            var type = new TypeAdhesion
            {
                Libelle = "Solo",
                MaxDependants = 0,
                Montant = 10m,
                DeviseId = devise.IdDevise,
                CategorieAdhesionId = categorie.IdCategorieAdhesion,
                Statut = true
            };
            db.TypeAdhesions.Add(type);

            var affilies = Enumerable.Range(1, 5).Select(i => new Affilie
            {
                CodeAdhesion = $"AFF-OBJ-{i}",
                Nom = "Test",
                Prenom = $"A{i}",
                NomComplet = $"Test A{i}",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = true
            }).ToList();
            db.Affilies.AddRange(affilies);
            await db.SaveChangesAsync();

            // 3 adhésions dans juin 2026 pour AT1, 1 hors période, 1 pour AT2
            db.Adhesions.AddRange(
                new Adhesion
                {
                    AgentId = agentAt1.IdAgent,
                    AffilieId = affilies[0].IdAffilie,
                    TypeAdhesionId = type.IdTypeAdhesion,
                    StatutDossier = "A",
                    Statut = true,
                    DateCreation = new DateTime(2026, 6, 5)
                },
                new Adhesion
                {
                    AgentId = agentAt1.IdAgent,
                    AffilieId = affilies[1].IdAffilie,
                    TypeAdhesionId = type.IdTypeAdhesion,
                    StatutDossier = "A",
                    Statut = true,
                    DateCreation = new DateTime(2026, 6, 15)
                },
                new Adhesion
                {
                    AgentId = agentAt1.IdAgent,
                    AffilieId = affilies[2].IdAffilie,
                    TypeAdhesionId = type.IdTypeAdhesion,
                    StatutDossier = "A",
                    Statut = true,
                    DateCreation = new DateTime(2026, 6, 20)
                },
                new Adhesion
                {
                    AgentId = agentAt1.IdAgent,
                    AffilieId = affilies[3].IdAffilie,
                    TypeAdhesionId = type.IdTypeAdhesion,
                    StatutDossier = "A",
                    Statut = true,
                    DateCreation = new DateTime(2026, 5, 31) // hors fenêtre
                },
                new Adhesion
                {
                    AgentId = agentAt2.IdAgent,
                    AffilieId = affilies[4].IdAffilie,
                    TypeAdhesionId = type.IdTypeAdhesion,
                    StatutDossier = "A",
                    Statut = true,
                    DateCreation = new DateTime(2026, 6, 10)
                });
            await db.SaveChangesAsync();

            var report = await service.GetObjectifsAgentsAsync(mois: 6, annee: 2026);

            Assert.Equal(6, report.Mois);
            Assert.Equal(2026, report.Annee);

            Assert.Single(report.SyntheseParRole);
            var syn = report.SyntheseParRole[0];
            Assert.Equal(atRole.IdRole, syn.RoleId);
            Assert.Equal("Agent (AT)", syn.RoleNom);
            Assert.Equal(10, syn.ObjectifUnitaire);
            Assert.Equal(2, syn.NombreAgents);
            Assert.Equal(20, syn.ObjectifTotal);
            Assert.Equal(4, syn.RealiseTotal); // 3 + 1
            Assert.Equal(20m, syn.Progression);

            Assert.Equal(2, report.DetailParAgent.Count);
            Assert.DoesNotContain(report.DetailParAgent, d => d.AgentId == agentSansTarget.IdAgent);
            Assert.DoesNotContain(report.DetailParAgent, d => d.AgentId == agentInactif.IdAgent);

            var detailAt1 = report.DetailParAgent.Single(d => d.AgentId == agentAt1.IdAgent);
            Assert.Equal(10, detailAt1.ObjectifAdhesions);
            Assert.Equal(3, detailAt1.RealiseAdhesions);
            Assert.Equal(30m, detailAt1.Progression);

            var detailAt2 = report.DetailParAgent.Single(d => d.AgentId == agentAt2.IdAgent);
            Assert.Equal(1, detailAt2.RealiseAdhesions);
            Assert.Equal(10m, detailAt2.Progression);

            // Tri par progression décroissante
            Assert.Equal(agentAt1.IdAgent, report.DetailParAgent[0].AgentId);
            Assert.Equal(agentAt2.IdAgent, report.DetailParAgent[1].AgentId);
        });
    }

    [Fact]
    public async Task GetObjectifsAgentsAsync_FallbackRoleAgent_SansUtilisateur()
    {
        await RunAsync(async (service, db) =>
        {
            var atRole = new Role { Nom = "Agent (AT)", Code = "AT", Statut = true };
            db.Roles.Add(atRole);
            await db.SaveChangesAsync();

            db.TargetsAgents.Add(new TargetAgent
            {
                RoleId = atRole.IdRole,
                LibelleTarget = "Mensuel AT",
                Periodicite = PeriodiciteTarget.Mensuelle,
                Nombre = 5,
                Statut = true
            });

            var agent = new Agent
            {
                NomComplet = "AT Fallback",
                Matricule = "AT000000210",
                Phone = "0990000210",
                RoleAgent = "Agent (AT)",
                Statut = true
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            var report = await service.GetObjectifsAgentsAsync(mois: 7, annee: 2026);

            Assert.Single(report.DetailParAgent);
            Assert.Equal(agent.IdAgent, report.DetailParAgent[0].AgentId);
            Assert.Equal(5, report.DetailParAgent[0].ObjectifAdhesions);
            Assert.Equal(0, report.DetailParAgent[0].RealiseAdhesions);
            Assert.Equal(0m, report.DetailParAgent[0].Progression);
            Assert.Equal(5, report.SyntheseParRole[0].ObjectifTotal);
        });
    }
}
