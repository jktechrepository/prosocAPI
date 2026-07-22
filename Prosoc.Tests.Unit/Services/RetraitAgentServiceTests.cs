using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;

namespace Prosoc.Tests.Unit.Services;

public class RetraitAgentServiceTests
{
    private static Mock<IWebHostEnvironment> CreateHostEnvironmentMock(bool integrationTests = false)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(integrationTests ? "IntegrationTests" : "UnitTest");
        return env;
    }

    private static Mock<IDeviseConversionService> CreateDevisePrincipaleMock(string code = "USD", string symbole = "$")
    {
        var mock = new Mock<IDeviseConversionService>();
        mock.Setup(x => x.GetDevisePrincipaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Devise
            {
                IdDevise = 1,
                Code = code,
                Symbole = symbole,
                EstDevisePrincipale = true,
                Statut = true
            });
        return mock;
    }

    private static Mock<IParametresMetierProvider> CreateParametresProviderMock(RetraitAgentOptions? options = null)
    {
        var opts = options ?? new RetraitAgentOptions();
        var mock = new Mock<IParametresMetierProvider>();
        mock.Setup(p => p.GetRetraitAgentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(opts);
        return mock;
    }

    private static Mock<ICaisseService> CreateCaisseServiceMock(bool integrationTests = true)
    {
        var mock = new Mock<ICaisseService>();
        mock.Setup(x => x.ResolveSessionPourOperationAsync(
                It.IsAny<int>(),
                It.IsAny<int?>(),
                integrationTests,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionCaisse?)null);
        mock.Setup(x => x.BuildMouvementSortieRetrait(
                It.IsAny<SessionCaisse>(),
                It.IsAny<int>(),
                It.IsAny<JetonRetrait>(),
                It.IsAny<DemandeRetraitAgent>(),
                It.IsAny<WalletMouvement>()))
            .Returns((SessionCaisse s, int u, JetonRetrait j, DemandeRetraitAgent d, WalletMouvement w) =>
                new MouvementCaisse
                {
                    SessionCaisseId = s.IdSessionCaisse,
                    UtilisateurId = u,
                    TypeOperation = MouvementCaisseTypes.Sortie,
                    Source = MouvementCaisseSources.RetraitAgent,
                    Montant = j.MontantRetrait,
                    DeviseId = s.DeviseId,
                    DemandeRetraitId = d.IdDemande,
                    JetonRetraitId = j.IdJeton,
                    WalletMouvementId = w.IdWalletMouvement,
                    DateOperation = DateTime.Now,
                    DateCreation = DateTime.Now,
                    Statut = true
                });
        return mock;
    }

    private static RetraitAgentService CreateService(
        ProsocDbContext db,
        Mock<IWalletAgentRepository> walletRepo,
        Mock<IDeviseConversionService>? deviseConversion = null,
        bool integrationTests = false,
        RetraitAgentOptions? retraitOptions = null,
        Mock<ICaisseService>? caisseService = null)
    {
        return new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo.Object,
            (deviseConversion ?? CreateDevisePrincipaleMock()).Object,
            CreateHostEnvironmentMock(integrationTests).Object,
            CreateParametresProviderMock(retraitOptions).Object,
            (caisseService ?? CreateCaisseServiceMock(integrationTests)).Object);
    }

    private static async Task SeedPrincipalWalletAsync(ProsocDbContext db, int agentId, decimal soldeCourant, decimal soldeDisponible)
    {
        if (!await db.Utilisateurs.AnyAsync(u => u.IdUtilisateur == 1))
        {
            db.Utilisateurs.Add(new Utilisateur
            {
                IdUtilisateur = 1,
                NomUtilisateur = "Caissier Test",
                MotDePasseHash = "hash",
                Statut = true
            });
        }

        var devise = new Devise
        {
            IdDevise = 1,
            Code = "USD",
            Nom = "Dollar",
            Symbole = "$",
            EstDevisePrincipale = true,
            Statut = true
        };
        db.Devises.Add(devise);
        db.Agents.Add(new Agent
        {
            IdAgent = agentId,
            NomComplet = "Agent Test",
            Matricule = "AG000000001",
            Phone = "0990000001",
            Statut = true
        });
        await db.SaveChangesAsync();

        db.WalletsAgents.Add(new WalletAgent
        {
            AgentId = agentId,
            DeviseId = devise.IdDevise,
            SoldeCourant = soldeCourant,
            SoldeDisponible = soldeDisponible,
            Statut = true
        });
        await db.SaveChangesAsync();
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

    [Theory]
    [InlineData(15, true, "15-20")]
    [InlineData(20, true, "15-20")]
    [InlineData(30, true, "30-31")]
    [InlineData(10, false, "Hors période")]
    public async Task VerifierPeriodeRetrait_ReturnsExpectedPeriodInfo(int day, bool expectedAuthorized, string expectedPeriodeInfo)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        var service = CreateService(db, walletRepo);

        var result = await service.VerifierPeriodeRetraitAsync(new DateTime(2026, 3, day));

        Assert.Equal(expectedAuthorized, result.EstPeriodeAutorisee);
        Assert.Equal(expectedPeriodeInfo, result.PeriodeInfo);
        Assert.Equal(day, result.JourDuMois);
    }

    [Theory]
    [InlineData(2026, 2, 27, true, "27-28")]
    [InlineData(2026, 2, 26, false, "Hors période")]
    [InlineData(2026, 4, 29, true, "29-30")]
    public async Task VerifierPeriodeRetrait_Fenetre2DerniersJours_ReturnsExpected(
        int year, int month, int day, bool expectedAuthorized, string expectedPeriodeInfo)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        var service = CreateService(db, walletRepo);

        var result = await service.VerifierPeriodeRetraitAsync(new DateTime(year, month, day));

        Assert.Equal(expectedAuthorized, result.EstPeriodeAutorisee);
        Assert.Equal(expectedPeriodeInfo, result.PeriodeInfo);
    }

    [Fact]
    public async Task GetPeriodeCourante_ReturnsFenetresDuMoisCourant()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        var service = CreateService(db, walletRepo);

        var result = await service.GetPeriodeCouranteAsync();
        var now = DateTime.Now;

        Assert.Equal(15, result.Fenetre1Debut);
        Assert.Equal(20, result.Fenetre1Fin);
        Assert.Equal(DateTime.DaysInMonth(now.Year, now.Month) - 1, result.Fenetre2Debut);
        Assert.Equal(DateTime.DaysInMonth(now.Year, now.Month), result.Fenetre2Fin);
        Assert.Equal(now.Day, result.JourDuMois);
        Assert.Equal(1000m, result.MontantMinimumPartiel);

        if (result.EstPeriodeAutorisee)
        {
            Assert.NotNull(result.FenetreActive);
            Assert.NotNull(result.TypeRetraitAutorise);
            Assert.Equal(
                result.TypeRetraitAutorise == RetraitAgentPeriodeHelper.TypePartiel,
                result.MontantDemandeRequis);
        }
    }

    [Fact]
    public async Task UtiliserJetonRetrait_OutsidePeriod_ReturnsFalse()
    {
        if (DateTime.Now.Day == DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
            return;

        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 60000m);

        var demande = new DemandeRetraitAgent
        {
            AgentId = 1,
            MontantDemande = 40000m,
            TypeRetrait = "PARTIEL",
            StatutDemande = "VALIDEE",
            DateDemande = DateTime.Now
        };
        db.DemandesRetraitAgents.Add(demande);
        await db.SaveChangesAsync();

        db.JetonsRetraits.Add(new JetonRetrait
        {
            AgentId = 1,
            DemandeRetraitId = demande.IdDemande,
            CodeJeton = "JRTHORSPE",
            MontantRetrait = 40000m,
            DateEmission = DateTime.Now,
            DateExpiration = DateTime.Now.AddDays(7),
            EstValide = true
        });
        await db.SaveChangesAsync();

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var retraitOptions = new RetraitAgentOptions { Fenetre1Debut = 50, Fenetre1Fin = 60, Fenetre2DerniersJours = 1 };
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            new DeviseConversionService(db),
            CreateHostEnvironmentMock(integrationTests: false).Object,
            CreateParametresProviderMock(retraitOptions).Object,
            CreateCaisseServiceMock(integrationTests: false).Object);

        var result = await service.UtiliserJetonRetraitAsync(new JetonRetraitUtilisationDto
        {
            CodeJeton = "JRTHORSPE",
            AgentId = 1
        }, operateurUtilisateurId: 1);

        Assert.False(result.Succes);
    }

    [Fact]
    public async Task VerifierSoldeDisponible_WhenPrincipalWalletExistsAndEnough_ReturnsSufficient()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        walletRepo.Setup(x => x.GetPrincipalWalletByAgentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WalletAgent
            {
                AgentId = 1,
                DeviseId = 1,
                SoldeDisponible = 100000m,
                SoldeCourant = 100000m
            });

        var service = CreateService(db, walletRepo);
        var result = await service.VerifierSoldeDisponible(1, 50000m);

        Assert.True(result.SoldeSuffisant);
        Assert.Equal(50000m, result.Difference);
        Assert.Equal("USD", result.DeviseCode);
        Assert.Equal("$", result.DeviseSymbole);
    }

    [Fact]
    public async Task VerifierSoldeDisponible_WhenPrincipalWalletMissing_ReturnsInsufficient()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        walletRepo.Setup(x => x.GetPrincipalWalletByAgentIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WalletAgent?)null);

        var service = CreateService(db, walletRepo);
        var result = await service.VerifierSoldeDisponible(99, 50000m);

        Assert.False(result.SoldeSuffisant);
        Assert.Contains("devise principale", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("USD", result.DeviseCode);
    }

    [Fact]
    public async Task VerifierSoldeDisponible_WhenSoldeDisponibleZeroButCourantPositive_ReturnsInsufficient()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        walletRepo.Setup(x => x.GetPrincipalWalletByAgentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WalletAgent
            {
                AgentId = 1,
                DeviseId = 1,
                SoldeDisponible = 0m,
                SoldeCourant = 49995m
            });

        var service = CreateService(db, walletRepo);
        var result = await service.VerifierSoldeDisponible(1, 1000m);

        Assert.False(result.SoldeSuffisant);
        Assert.Contains("$", result.Message);
        Assert.Equal("USD", result.DeviseCode);
    }

    [Fact]
    public async Task GetStatsAsync_ComputesExpectedAggregates()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var walletRepo = new Mock<IWalletAgentRepository>();
        var service = CreateService(db, walletRepo);

        var date = new DateTime(2026, 3, 15);

        db.Agents.Add(new Agent
        {
            IdAgent = 1,
            NomComplet = "Agent Test",
            Matricule = "AG000000099",
            Phone = "0990000099",
            Statut = true
        });
        await db.SaveChangesAsync();

        db.DemandesRetraitAgents.AddRange(
            new DemandeRetraitAgent { AgentId = 1, MontantDemande = 50000m, StatutDemande = "EN_ATTENTE", DateDemande = date },
            new DemandeRetraitAgent { AgentId = 1, MontantDemande = 30000m, StatutDemande = "VALIDEE", DateDemande = date },
            new DemandeRetraitAgent { AgentId = 1, MontantDemande = 20000m, StatutDemande = "TRAITEE", DateDemande = date },
            new DemandeRetraitAgent { AgentId = 1, MontantDemande = 10000m, StatutDemande = "REJETEE", DateDemande = date }
        );
        await db.SaveChangesAsync();

        var result = await service.GetStatsAsync(date);

        Assert.Equal(4, result.TotalDemandes);
        Assert.Equal(1, result.DemandesEnAttente);
        Assert.Equal(1, result.DemandesValidees);
        Assert.Equal(1, result.DemandesTraitees);
        Assert.Equal(1, result.DemandesRejetees);
        Assert.Equal(100000m, result.TotalMontantDemande);
        Assert.Equal(20000m, result.TotalMontantTraite);
    }

    [Fact]
    public async Task CreerDemandeRetraitAsync_ReservesSoldeDisponible()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 100000m);

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            new DeviseConversionService(db),
            CreateHostEnvironmentMock(integrationTests: true).Object,
            CreateParametresProviderMock().Object,
            CreateCaisseServiceMock(integrationTests: true).Object);

        var result = await service.CreerDemandeRetraitAsync(new DemandeRetraitAgentCreateDto
        {
            AgentId = 1,
            MontantDemande = 40000m,
            TypeRetrait = "PARTIEL"
        });

        Assert.True(result.Succes);
        var wallet = await db.WalletsAgents.FirstAsync(w => w.AgentId == 1);
        Assert.Equal(100000m, wallet.SoldeCourant);
        Assert.Equal(60000m, wallet.SoldeDisponible);
    }

    [Fact]
    public async Task UpdateAsync_ToRejetee_ReleasesSoldeDisponible()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 60000m);

        db.DemandesRetraitAgents.Add(new DemandeRetraitAgent
        {
            AgentId = 1,
            MontantDemande = 40000m,
            TypeRetrait = "PARTIEL",
            StatutDemande = "EN_ATTENTE",
            DateDemande = DateTime.Now
        });
        await db.SaveChangesAsync();

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            new DeviseConversionService(db),
            CreateHostEnvironmentMock(integrationTests: true).Object,
            CreateParametresProviderMock().Object,
            CreateCaisseServiceMock(integrationTests: true).Object);

        var demande = await db.DemandesRetraitAgents.FirstAsync();
        demande.StatutDemande = "REJETEE";
        await service.UpdateAsync(demande.IdDemande, demande);

        var wallet = await db.WalletsAgents.FirstAsync(w => w.AgentId == 1);
        Assert.Equal(100000m, wallet.SoldeCourant);
        Assert.Equal(100000m, wallet.SoldeDisponible);
    }

    [Fact]
    public async Task UtiliserJetonRetrait_WhenReserved_OnlyDebitsSoldeCourant()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 60000m);

        var demande = new DemandeRetraitAgent
        {
            AgentId = 1,
            MontantDemande = 40000m,
            TypeRetrait = "PARTIEL",
            StatutDemande = "VALIDEE",
            DateDemande = DateTime.Now
        };
        db.DemandesRetraitAgents.Add(demande);
        await db.SaveChangesAsync();

        db.JetonsRetraits.Add(new JetonRetrait
        {
            AgentId = 1,
            DemandeRetraitId = demande.IdDemande,
            CodeJeton = "JRTUNIT01",
            MontantRetrait = 40000m,
            DateEmission = DateTime.Now,
            DateExpiration = DateTime.Now.AddDays(7),
            EstValide = true
        });
        await db.SaveChangesAsync();

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            new DeviseConversionService(db),
            CreateHostEnvironmentMock(integrationTests: true).Object,
            CreateParametresProviderMock().Object,
            CreateCaisseServiceMock(integrationTests: true).Object);

        var result = await service.UtiliserJetonRetraitAsync(new JetonRetraitUtilisationDto
        {
            CodeJeton = "JRTUNIT01",
            AgentId = 1
        }, operateurUtilisateurId: 1);

        Assert.True(result.Succes);
        var wallet = await db.WalletsAgents.FirstAsync(w => w.AgentId == 1);
        Assert.Equal(60000m, wallet.SoldeCourant);
        Assert.Equal(60000m, wallet.SoldeDisponible);
    }

    [Fact]
    public async Task UtiliserJetonRetrait_LegacyWithoutReservation_DebitsBothSoldes()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 100000m);

        var demande = new DemandeRetraitAgent
        {
            AgentId = 1,
            MontantDemande = 40000m,
            TypeRetrait = "PARTIEL",
            StatutDemande = "VALIDEE",
            DateDemande = DateTime.Now
        };
        db.DemandesRetraitAgents.Add(demande);
        await db.SaveChangesAsync();

        db.JetonsRetraits.Add(new JetonRetrait
        {
            AgentId = 1,
            DemandeRetraitId = demande.IdDemande,
            CodeJeton = "JRTUNIT02",
            MontantRetrait = 40000m,
            DateEmission = DateTime.Now,
            DateExpiration = DateTime.Now.AddDays(7),
            EstValide = true
        });
        await db.SaveChangesAsync();

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            new DeviseConversionService(db),
            CreateHostEnvironmentMock(integrationTests: true).Object,
            CreateParametresProviderMock().Object,
            CreateCaisseServiceMock(integrationTests: true).Object);

        var result = await service.UtiliserJetonRetraitAsync(new JetonRetraitUtilisationDto
        {
            CodeJeton = "JRTUNIT02",
            AgentId = 1
        }, operateurUtilisateurId: 1);

        Assert.True(result.Succes);
        var wallet = await db.WalletsAgents.FirstAsync(w => w.AgentId == 1);
        Assert.Equal(60000m, wallet.SoldeCourant);
        Assert.Equal(60000m, wallet.SoldeDisponible);
    }

    [Fact]
    public async Task UtiliserJetonRetrait_WhenExpired_ReleasesSoldeDisponible()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 60000m);

        var demande = new DemandeRetraitAgent
        {
            AgentId = 1,
            MontantDemande = 40000m,
            TypeRetrait = "PARTIEL",
            StatutDemande = "VALIDEE",
            DateDemande = DateTime.Now
        };
        db.DemandesRetraitAgents.Add(demande);
        await db.SaveChangesAsync();

        db.JetonsRetraits.Add(new JetonRetrait
        {
            AgentId = 1,
            DemandeRetraitId = demande.IdDemande,
            CodeJeton = "JRTEXPIRED",
            MontantRetrait = 40000m,
            DateEmission = DateTime.Now.AddDays(-10),
            DateExpiration = DateTime.Now.AddDays(-1),
            EstValide = true
        });
        await db.SaveChangesAsync();

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            new DeviseConversionService(db),
            CreateHostEnvironmentMock(integrationTests: true).Object,
            CreateParametresProviderMock().Object,
            CreateCaisseServiceMock(integrationTests: true).Object);

        var result = await service.UtiliserJetonRetraitAsync(new JetonRetraitUtilisationDto
        {
            CodeJeton = "JRTEXPIRED",
            AgentId = 1
        }, operateurUtilisateurId: 1);

        Assert.False(result.Succes);
        var wallet = await db.WalletsAgents.FirstAsync(w => w.AgentId == 1);
        Assert.Equal(100000m, wallet.SoldeCourant);
        Assert.Equal(100000m, wallet.SoldeDisponible);

        var demandeApres = await db.DemandesRetraitAgents.FirstAsync();
        Assert.Equal("REJETEE", demandeApres.StatutDemande);
        Assert.Equal("Jeton de retrait expiré", demandeApres.MotifRejet);
    }

    [Fact]
    public async Task UtiliserJetonRetrait_SansSessionOuverte_Production_RetourneSessionCaisseRequise()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);
        await SeedPrincipalWalletAsync(db, agentId: 1, soldeCourant: 100000m, soldeDisponible: 100000m);

        var demande = new DemandeRetraitAgent
        {
            AgentId = 1,
            MontantDemande = 30000m,
            TypeRetrait = "PARTIEL",
            StatutDemande = "VALIDEE",
            DateDemande = DateTime.Now
        };
        db.DemandesRetraitAgents.Add(demande);
        await db.SaveChangesAsync();

        db.JetonsRetraits.Add(new JetonRetrait
        {
            AgentId = 1,
            DemandeRetraitId = demande.IdDemande,
            CodeJeton = "JRTNOSESSION",
            MontantRetrait = 30000m,
            DateEmission = DateTime.Now,
            DateExpiration = DateTime.Now.AddDays(7),
            EstValide = true
        });
        await db.SaveChangesAsync();

        var caisseMock = new Mock<ICaisseService>();
        caisseMock.Setup(x => x.ResolveSessionPourOperationAsync(
                It.IsAny<int>(),
                It.IsAny<int?>(),
                false,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SESSION_CAISSIER_REQUISE"));

        var walletRepo = new WalletAgentService(db, Mock.Of<ILogger<WalletAgentService>>());
        var service = new RetraitAgentService(
            db,
            Mock.Of<ILogger<RetraitAgentService>>(),
            walletRepo,
            CreateDevisePrincipaleMock().Object,
            CreateHostEnvironmentMock(integrationTests: false).Object,
            CreateParametresProviderMock().Object,
            caisseMock.Object);

        var result = await service.UtiliserJetonRetraitAsync(new JetonRetraitUtilisationDto
        {
            CodeJeton = "JRTNOSESSION",
            AgentId = 1
        }, operateurUtilisateurId: 1);

        Assert.False(result.Succes);
        Assert.Equal("SESSION_CAISSIER_REQUISE", result.CodeErreur);
        Assert.Contains("session de caisse ouverte", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
