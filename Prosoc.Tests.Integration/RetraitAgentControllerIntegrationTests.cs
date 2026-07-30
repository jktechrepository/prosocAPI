using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using Xunit;

namespace Prosoc.Tests.Integration;

public class RetraitAgentControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RetraitAgentControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Vérification Période Tests

    [Fact]
    public async Task VerifierPeriodeRetrait_DateAutorisee_RetourneVrai()
    {
        // Arrange
        var date = "2026-03-16";

        // Act
        var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", date);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
        
        Assert.NotNull(result);
        Assert.True(result.EstPeriodeAutorisee);
        Assert.Equal("15-20", result.PeriodeInfo);
        Assert.Equal(16, result.JourDuMois);
        Assert.Equal("Période de retrait autorisée", result.Message);
    }

    [Fact]
    public async Task VerifierPeriodeRetrait_DateNonAutorisee_RetourneFaux()
    {
        // Arrange
        var date = "2026-03-10";

        // Act
        var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", date);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
        
        Assert.NotNull(result);
        Assert.False(result.EstPeriodeAutorisee);
        Assert.Equal("Hors période", result.PeriodeInfo);
        Assert.Equal(10, result.JourDuMois);
        Assert.Contains("ne sont autorisés", result.Message);
    }

    [Fact]
    public async Task VerifierPeriodeRetrait_DateAutorisee_30Plus_RetourneVrai()
    {
        // Arrange
        var date = "2026-03-31";

        // Act
        var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", date);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
        
        Assert.NotNull(result);
        Assert.True(result.EstPeriodeAutorisee);
        Assert.Equal("25-31", result.PeriodeInfo);
        Assert.Equal(31, result.JourDuMois);
        Assert.Equal("Période de retrait autorisée", result.Message);
    }

    [Fact]
    public async Task VerifierPeriodeRetrait_Fevrier_DerniersJours_RetourneVrai()
    {
        var date = "2026-02-27";

        var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", date);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();

        Assert.NotNull(result);
        Assert.True(result.EstPeriodeAutorisee);
        Assert.Equal("22-28", result.PeriodeInfo);
        Assert.Equal(27, result.JourDuMois);
    }

    [Fact]
    public async Task GetPeriodeCourante_RetourneFenetresDuMois()
    {
        var response = await _client.GetAsync("/api/retraitagent/periode-courante");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PeriodeRetraitCouranteDto>();

        Assert.NotNull(result);
        Assert.Equal(15, result.Fenetre1Debut);
        Assert.Equal(20, result.Fenetre1Fin);
        Assert.Equal(5m, result.MontantMinimumPartiel);

        var now = DateTime.Now;
        const int fenetre2DerniersJours = 7;
        Assert.Equal(DateTime.DaysInMonth(now.Year, now.Month) - fenetre2DerniersJours + 1, result.Fenetre2Debut);
        Assert.Equal(DateTime.DaysInMonth(now.Year, now.Month), result.Fenetre2Fin);
        Assert.Equal(now.Day, result.JourDuMois);

        if (result.EstPeriodeAutorisee)
        {
            Assert.NotNull(result.FenetreActive);
            Assert.NotNull(result.TypeRetraitAutorise);
            Assert.True(
                result.TypeRetraitAutorise is "PARTIEL" or "TOTAL");
            Assert.Equal(result.TypeRetraitAutorise == "PARTIEL", result.MontantDemandeRequis);
        }
    }

    [Fact]
    public async Task GetPeriodeCourante_ExposeTypeRetraitAutorise()
    {
        var response = await _client.GetAsync("/api/retraitagent/periode-courante");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PeriodeRetraitCouranteDto>();

        Assert.NotNull(result);
        if (result.FenetreActive == "Fenetre1")
            Assert.Equal("PARTIEL", result.TypeRetraitAutorise);
        else if (result.FenetreActive == "Fenetre2")
            Assert.Equal("TOTAL", result.TypeRetraitAutorise);
    }

    #endregion

    #region Vérification Solde Tests

    [Fact]
    public async Task VerifierSoldeDisponible_SoldeSuffisant_RetourneVrai()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-001",
                Phone = "0999999999",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer un wallet avec solde suffisant
            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 100000,
                SoldeDisponible = 100000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var soldeDto = new SoldeVerificationDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            
            Assert.NotNull(result);
            Assert.True(result.SoldeSuffisant);
            Assert.Equal(agent.IdAgent, result.AgentId);
            Assert.Equal(50000, result.MontantDemande);
            Assert.Equal(100000, result.SoldeDisponible);
            Assert.Equal(50000, result.Difference);
            Assert.Contains("Solde suffisant pour le retrait", result.Message);
            Assert.Equal("USD", result.DeviseCode);
        }
    }

    [Fact]
    public async Task VerifierSoldeDisponible_SoldeInsuffisant_RetourneFaux()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-002",
                Phone = "0999999998",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer un wallet avec solde insuffisant
            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 50000,
                SoldeDisponible = 50000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var soldeDto = new SoldeVerificationDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 75000
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            
            Assert.NotNull(result);
            Assert.False(result.SoldeSuffisant);
            Assert.Equal(agent.IdAgent, result.AgentId);
            Assert.Equal(75000, result.MontantDemande);
            Assert.Equal(50000, result.SoldeDisponible);
            Assert.Equal(25000, result.Difference);
            Assert.Contains("Solde insuffisant", result.Message);
        }
    }

    [Fact]
    public async Task VerifierSoldeDisponible_AgentNonTrouve_RetourneFaux()
    {
        // Arrange
        var soldeDto = new SoldeVerificationDto
        {
            AgentId = 999, // Agent qui n'existe pas
            MontantDemande = 50000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SoldeVerificationDto>();
        
        Assert.NotNull(result);
        Assert.False(result.SoldeSuffisant);
        Assert.Equal(999, result.AgentId);
        Assert.Equal(50000, result.MontantDemande);
        Assert.Equal(0, result.SoldeDisponible);
        Assert.Equal(50000, result.Difference);
        Assert.Contains("devise principale", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Création Demande Tests

    [Fact]
    public async Task CreerDemandeRetrait_DemandeValide_CreeDemande()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-003",
                Phone = "0999999997",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer un wallet avec solde suffisant
            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 100000,
                SoldeDisponible = 100000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var createDto = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Frais de scolarité"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent", createDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            
            Assert.NotNull(result);
            Assert.True(result.Succes);
            Assert.NotNull(result.DemandeId);
            Assert.Equal(50000, result.MontantRetrait);
            var demande = await _client.GetFromJsonAsync<DemandeRetraitAgentReadDto>($"/api/retraitagent/{result.DemandeId}");
            Assert.NotNull(demande);
            Assert.Equal(agent.IdAgent, demande.AgentId);
            Assert.Equal(50000, demande.MontantDemande);
            Assert.Equal("PARTIEL", demande.TypeRetrait);
            Assert.Equal("EN_ATTENTE", demande.StatutDemande);
            Assert.Equal("Frais de scolarité", demande.MotifRetrait);
            Assert.Contains("créée avec succès", result.Message);
        }
    }

    [Fact]
    public async Task CreerDemandeRetrait_MontantInvalide_Erreur()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-004",
                Phone = "0999999996",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer un wallet
            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 100000,
                SoldeDisponible = 100000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var createDto = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 1m, // En dessous du minimum (5 en devise principale)
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent", createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task CreerDemandeRetrait_TypeRetraitIncompatible_RemplaceParTypeValide()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-005",
                Phone = "0999999995",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 100000,
                SoldeDisponible = 100000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var createDto = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "INVALIDE",
                MotifRetrait = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent", createDto);

            // Assert — type incompatible remplacé (PARTIEL par défaut en mode test)
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.NotNull(result);
            Assert.True(result!.Succes);
            Assert.Equal("PARTIEL", result.TypeRetrait);

            var demande = await db.DemandesRetraitAgents.FirstAsync(d => d.AgentId == agent.IdAgent);
            Assert.Equal("PARTIEL", demande.TypeRetrait);
        }
    }

    #endregion

    #region Validation et Génération Jeton Tests

    [Fact]
    public async Task ValiderEtGenererJeton_DemandeExistante_GenereJeton()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-006",
                Phone = "0999999994",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer un wallet
            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 100000,
                SoldeDisponible = 100000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            // Créer une demande
            var demande = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test",
                DateDemande = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();

            var validationDto = new DemandeRetraitAgentValidationDto
            {
                IdDemande = demande.IdDemande,
                AgentValidationId = agent.IdAgent,
                StatutDemande = "VALIDEE"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent/valider-et-generer-jeton", validationDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            
            Assert.NotNull(result);
            Assert.True(result.Succes);
            Assert.NotNull(result.JetonCode);
            Assert.StartsWith("JRT", result.JetonCode);
            Assert.Equal(11, result.JetonCode!.Length);
            Assert.NotNull(result.JetonId);
            var demandeValidee = await _client.GetFromJsonAsync<DemandeRetraitAgentReadDto>($"/api/retraitagent/{demande.IdDemande}");
            Assert.Equal("VALIDEE", demandeValidee!.StatutDemande);
            Assert.NotNull(demandeValidee.DateValidation);
            Assert.Equal(result.JetonCode, demandeValidee.JetonRetraitCode);

            var jetonDb = await db.JetonsRetraits.FindAsync(result.JetonId);
            Assert.NotNull(jetonDb);
            Assert.True(jetonDb!.EstValide);
            Assert.False(jetonDb.EstUtilise);
            Assert.Contains("validée", result.Message);
        }
    }

    [Fact]
    public async Task ValiderEtGenererJeton_DemandeNonTrouve_Erreur()
    {
        // Arrange
        var validationDto = new DemandeRetraitAgentValidationDto
        {
            IdDemande = 999, // Demande qui n'existe pas
            AgentValidationId = 1,
            StatutDemande = "VALIDEE"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/retraitagent/valider-et-generer-jeton", validationDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Utilisation Jeton Tests

    [Fact]
    public async Task UtiliserJetonRetrait_JetonValide_UtiliseJeton()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-007",
                Phone = "0999999993",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer un wallet
            var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
            var wallet = new WalletAgent
            {
                AgentId = agent.IdAgent,
                DeviseId = deviseId,
                SoldeCourant = 100000,
                SoldeDisponible = 100000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            // Créer une demande et un jeton
            var demande = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "VALIDEE",
                MotifRetrait = "Test",
                DateDemande = DateTime.Now,
                DateValidation = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();

            var jeton = new JetonRetrait
            {
                AgentId = agent.IdAgent,
                DemandeRetraitId = demande.IdDemande,
                CodeJeton = "JRTTEST123",
                DateEmission = DateTime.Now,
                MontantRetrait = 50000,
                DateExpiration = DateTime.Now.AddDays(7),
                EstValide = true,
                EstUtilise = false
            };
            db.JetonsRetraits.Add(jeton);
            await db.SaveChangesAsync();

            var utilisationDto = new JetonRetraitUtilisationDto
            {
                IdJeton = jeton.IdJeton,
                CodeJeton = "JRTTEST123",
                AgentId = agent.IdAgent,
                ObservationUtilisation = "Retrait effectué avec succès"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/retraitagent/utiliser-jeton", utilisationDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var paiement = await response.Content.ReadFromJsonAsync<RetraitPaiementResultDto>();
            Assert.NotNull(paiement);
            Assert.True(paiement!.Succes);

            var demandeApres = await _client.GetFromJsonAsync<DemandeRetraitAgentReadDto>($"/api/retraitagent/{demande.IdDemande}");
            Assert.NotNull(demandeApres);
            Assert.Equal("TRAITEE", demandeApres!.StatutDemande);
            Assert.NotNull(demandeApres.DateTraitement);

            using (var jetonScope = _factory.Services.CreateScope())
            {
                var jetonDbContext = jetonScope.ServiceProvider.GetRequiredService<ProsocDbContext>();
                var jetonDb = await jetonDbContext.JetonsRetraits.FindAsync(jeton.IdJeton);
                Assert.NotNull(jetonDb);
                Assert.True(jetonDb!.EstUtilise);
                Assert.NotNull(jetonDb.DateUtilisation);
                Assert.Equal("Retrait effectué avec succès", jetonDb.ObservationUtilisation);

                var walletMouvement = await jetonDbContext.WalletMouvements
                    .FirstOrDefaultAsync(w => w.Source == "RETRAIT_JETON");
                Assert.NotNull(walletMouvement);
                Assert.Equal(50000m, walletMouvement!.Montant);
            }
        }
    }

    [Fact]
    public async Task UtiliserJetonRetrait_AsPercepteur_AutorisePaiement()
    {
        var previousRoles = TestAuthHandler.Roles;
        try
        {
            TestAuthHandler.Roles = new[] { "Percepteur" };

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

                var agent = new Agent
                {
                    NomComplet = "Agent Percepteur Test",
                    Matricule = "MAT-PERCEPT-07",
                    Phone = "0999999988",
                    ZoneSocialeId = 1,
                    Statut = true,
                    DateCreation = DateTime.Now
                };
                db.Agents.Add(agent);
                await db.SaveChangesAsync();

                var deviseId = await IntegrationTestDbHelper.GetPrincipalDeviseIdAsync(db);
                db.WalletsAgents.Add(new WalletAgent
                {
                    AgentId = agent.IdAgent,
                    DeviseId = deviseId,
                    SoldeCourant = 100000,
                    SoldeDisponible = 100000,
                    DateCreation = DateTime.Now
                });
                await db.SaveChangesAsync();

                var demande = new DemandeRetraitAgent
                {
                    AgentId = agent.IdAgent,
                    MontantDemande = 40000,
                    TypeRetrait = "PARTIEL",
                    StatutDemande = "VALIDEE",
                    MotifRetrait = "Test percepteur",
                    DateDemande = DateTime.Now,
                    DateValidation = DateTime.Now
                };
                db.DemandesRetraitAgents.Add(demande);
                await db.SaveChangesAsync();

                var jeton = new JetonRetrait
                {
                    AgentId = agent.IdAgent,
                    DemandeRetraitId = demande.IdDemande,
                    CodeJeton = "JRTPERCEPT1",
                    DateEmission = DateTime.Now,
                    MontantRetrait = 40000,
                    DateExpiration = DateTime.Now.AddDays(7),
                    EstValide = true,
                    EstUtilise = false
                };
                db.JetonsRetraits.Add(jeton);
                await db.SaveChangesAsync();

                var utilisationDto = new JetonRetraitUtilisationDto
                {
                    IdJeton = jeton.IdJeton,
                    CodeJeton = "JRTPERCEPT1",
                    AgentId = agent.IdAgent,
                    ObservationUtilisation = "Paiement par percepteur terrain"
                };

                var response = await _client.PostAsJsonAsync("/api/retraitagent/utiliser-jeton", utilisationDto);

                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                response.EnsureSuccessStatusCode();
                var paiement = await response.Content.ReadFromJsonAsync<RetraitPaiementResultDto>();
                Assert.NotNull(paiement);
                Assert.True(paiement!.Succes);
            }
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
        }
    }

    [Fact]
    public async Task UtiliserJetonRetrait_JetonInvalide_Erreur()
    {
        // Arrange
        var utilisationDto = new JetonRetraitUtilisationDto
        {
            IdJeton = 999, // Jeton qui n'existe pas
            CodeJeton = "INVALIDE",
            AgentId = 1,
            ObservationUtilisation = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/retraitagent/utiliser-jeton", utilisationDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Récupération Demandes Tests

    [Fact]
    public async Task GetAll_AvecDemandes_RetourneListe()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-008",
                Phone = "0999999992",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer des demandes
            var demande1 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test 1",
                DateDemande = DateTime.Now
            };
            var demande2 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 30000,
                TypeRetrait = "TOTAL",
                StatutDemande = "VALIDEE",
                MotifRetrait = "Test 2",
                DateDemande = DateTime.Now
            };
            db.DemandesRetraitAgents.AddRange(new[] { demande1, demande2 });
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/retraitagent");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<DemandeRetraitAgentReadDto>>();
            
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Count >= 2);
            Assert.Contains(result.Data, d => d.MontantDemande == 50000 && d.StatutDemande == "EN_ATTENTE");
            Assert.Contains(result.Data, d => d.MontantDemande == 30000 && d.StatutDemande == "VALIDEE");
        }
    }

    [Fact]
    public async Task GetAll_FiltreStatutDemandeEnAttente_RetourneUniquementEnAttente()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var agent = new Agent
            {
                NomComplet = "Agent Filtre Statut",
                Matricule = $"MAT-FS-{Guid.NewGuid():N}"[..11],
                Phone = $"099{Random.Shared.Next(1000000, 9999999)}",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            db.DemandesRetraitAgents.AddRange(
                new DemandeRetraitAgent
                {
                    AgentId = agent.IdAgent,
                    MontantDemande = 111,
                    TypeRetrait = "PARTIEL",
                    StatutDemande = "EN_ATTENTE",
                    MotifRetrait = "Filtre EN_ATTENTE",
                    DateDemande = DateTime.Now
                },
                new DemandeRetraitAgent
                {
                    AgentId = agent.IdAgent,
                    MontantDemande = 222,
                    TypeRetrait = "PARTIEL",
                    StatutDemande = "VALIDEE",
                    MotifRetrait = "Filtre VALIDEE",
                    DateDemande = DateTime.Now
                });
            await db.SaveChangesAsync();
        }

        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

        var response = await _client.GetAsync("/api/retraitagent?statutDemande=EN_ATTENTE&pageSize=100");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<DemandeRetraitAgentReadDto>>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.All(result.Data, d => Assert.Equal("EN_ATTENTE", d.StatutDemande));
        Assert.Contains(result.Data, d => d.MontantDemande == 111);
        Assert.DoesNotContain(result.Data, d => d.MontantDemande == 222);
    }

    [Fact]
    public async Task GetAll_FiltreStatutDemandeInvalide_RetourneBadRequest()
    {
        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

        var response = await _client.GetAsync("/api/retraitagent?statutDemande=INCONNU");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("statutDemande", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByAgent_FiltreStatutDemandeEnAttente_RetourneUniquementEnAttente()
    {
        int agentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var agent = new Agent
            {
                NomComplet = "Agent ByAgent Filtre",
                Matricule = $"MAT-BA-{Guid.NewGuid():N}"[..11],
                Phone = $"098{Random.Shared.Next(1000000, 9999999)}",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();
            agentId = agent.IdAgent;

            db.DemandesRetraitAgents.AddRange(
                new DemandeRetraitAgent
                {
                    AgentId = agentId,
                    MontantDemande = 333,
                    TypeRetrait = "PARTIEL",
                    StatutDemande = "EN_ATTENTE",
                    MotifRetrait = "ByAgent EN_ATTENTE",
                    DateDemande = DateTime.Now
                },
                new DemandeRetraitAgent
                {
                    AgentId = agentId,
                    MontantDemande = 444,
                    TypeRetrait = "PARTIEL",
                    StatutDemande = "VALIDEE",
                    MotifRetrait = "ByAgent VALIDEE",
                    DateDemande = DateTime.Now
                });
            await db.SaveChangesAsync();
        }

        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

        var response = await _client.GetAsync($"/api/retraitagent/by-agent/{agentId}?statutDemande=EN_ATTENTE");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<DemandeRetraitAgentReadDto>>();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EN_ATTENTE", result[0].StatutDemande);
        Assert.Equal(333, result[0].MontantDemande);
    }

    [Fact]
    public async Task GetByAgent_FiltreStatutDemandeInvalide_RetourneBadRequest()
    {
        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = new[] { "READ_DEMANDE_RETRAIT_AGENT" };

        var response = await _client.GetAsync("/api/retraitagent/by-agent/1?statutDemande=INCONNU");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("statutDemande", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarquerPaye_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Agent (AT)" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync(
                "/api/RetraitAgent/marquer-paye",
                new JetonRetraitUtilisationDto
                {
                    CodeJeton = "JETON-FAKE",
                    AgentId = 1,
                    IdJeton = 0
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("MARQUER_PAYER_RETRAIT_AGENT", body, StringComparison.Ordinal);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task GetById_DemandeExistante_RetourneDemande()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-009",
                Phone = "0999999991",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer une demande
            var demande = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test",
                DateDemande = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/retraitagent/{demande.IdDemande}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DemandeRetraitAgentReadDto>();
            
            Assert.NotNull(result);
            Assert.Equal(demande.IdDemande, result.IdDemande);
            Assert.Equal(agent.IdAgent, result.AgentId);
            Assert.Equal(50000, result.MontantDemande);
            Assert.Equal("PARTIEL", result.TypeRetrait);
            Assert.Equal("EN_ATTENTE", result.StatutDemande);
            Assert.Equal("Test", result.MotifRetrait);
        }
    }

    [Fact]
    public async Task GetById_DemandeNonTrouve_RetourneNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/retraitagent/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Tests par Statut

    [Fact]
    public async Task GetByStatut_StatutEnAttente_RetourneDemandesEnAttente()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-010",
                Phone = "0999999990",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer des demandes avec différents statuts
            var demande1 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test 1",
                DateDemande = DateTime.Now
            };
            var demande2 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 30000,
                TypeRetrait = "TOTAL",
                StatutDemande = "VALIDEE",
                MotifRetrait = "Test 2",
                DateDemande = DateTime.Now
            };
            db.DemandesRetraitAgents.AddRange(new[] { demande1, demande2 });
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/retraitagent/by-statut/EN_ATTENTE");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<DemandeRetraitAgentReadDto>>();
            
            Assert.NotNull(result);
            Assert.All(result, d => Assert.Equal("EN_ATTENTE", d.StatutDemande));
            Assert.Contains(result, d => d.IdDemande == demande1.IdDemande);
        }
    }

    [Fact]
    public async Task GetEnAttente_RetourneDemandesEnAttente()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-011",
                Phone = "0999999989",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer une demande en attente
            var demande = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test",
                DateDemande = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/retraitagent/en-attente");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<DemandeRetraitAgentReadDto>>();
            
            Assert.NotNull(result);
            Assert.All(result, d => Assert.Equal("EN_ATTENTE", d.StatutDemande));
            Assert.Contains(result, d => d.IdDemande == demande.IdDemande);
        }
    }

    #endregion

    #region Tests de Statistiques

    [Fact]
    public async Task GetStats_DateValide_RetourneStats()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-012",
                Phone = "0999999988",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer des demandes
            var demande1 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test 1",
                DateDemande = new DateTime(2026, 3, 15)
            };
            var demande2 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 30000,
                TypeRetrait = "TOTAL",
                StatutDemande = "VALIDEE",
                MotifRetrait = "Test 2",
                DateDemande = new DateTime(2026, 3, 16)
            };
            var demande3 = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 20000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "TRAITEE",
                MotifRetrait = "Test 3",
                DateDemande = new DateTime(2026, 3, 17)
            };
            db.DemandesRetraitAgents.AddRange(new[] { demande1, demande2, demande3 });
            await db.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/retraitagent/stats/2026-03-15");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<DemandeRetraitAgentStatsDto>();
            
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalDemandes);
            Assert.Equal(100000, result.TotalMontantDemande);
            Assert.Equal(1, result.DemandesEnAttente);
            Assert.Equal(1, result.DemandesValidees);
            Assert.Equal(1, result.DemandesTraitees);
            Assert.Equal(20000, result.TotalMontantTraite);
            Assert.True(result.TauxValidation > 0);
        }
    }

    #endregion

    #region Tests de Suppression

    [Fact]
    public async Task Delete_DemandeExistante_SupprimeDemande()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-013",
                Phone = "0999999987",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            // Créer une demande
            var demande = new DemandeRetraitAgent
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                StatutDemande = "EN_ATTENTE",
                MotifRetrait = "Test",
                DateDemande = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/retraitagent/{demande.IdDemande}");

            // Assert
            response.EnsureSuccessStatusCode();
            
            using (var verifyScope = _factory.Services.CreateScope())
            {
                var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ProsocDbContext>();
                var deletedDemande = await verifyDb.DemandesRetraitAgents.FindAsync(demande.IdDemande);
                Assert.Null(deletedDemande);
            }
        }
    }

    [Fact]
    public async Task Delete_DemandeNonTrouve_RetourneNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/retraitagent/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}
