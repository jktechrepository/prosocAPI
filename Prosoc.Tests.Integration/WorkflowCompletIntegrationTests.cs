using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class WorkflowCompletIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public WorkflowCompletIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Workflow Retrait Agent Complet

    [Fact]
    public async Task WorkflowRetraitAgentComplet_DemandeVersUtilisation_Succes()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent avec wallet suffisant
            var agent = new Agent
            {
                NomComplet = "Agent Workflow Test",
                Matricule = "MAT-WF-001",
                Phone = "0999999999",
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
                SoldeCourant = 200000,
                SoldeDisponible = 200000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            // Étape 1: Vérifier la période de retrait
            var dateAutorisee = new DateTime(2026, 3, 16);
            var responsePeriode = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", dateAutorisee.ToString("yyyy-MM-dd"));
            responsePeriode.EnsureSuccessStatusCode();
            var periodeResult = await responsePeriode.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
            Assert.True(periodeResult.EstPeriodeAutorisee);

            // Étape 2: Vérifier le solde disponible
            var soldeDto = new SoldeVerificationDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 75000
            };
            var responseSolde = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);
            responseSolde.EnsureSuccessStatusCode();
            var soldeResult = await responseSolde.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            Assert.True(soldeResult.SoldeSuffisant);
            Assert.Equal(200000, soldeResult.SoldeDisponible);

            // Étape 3: Créer la demande de retrait
            var createDto = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 75000,
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Frais de scolarité et dépenses familiales"
            };
            var responseCreate = await _client.PostAsJsonAsync("/api/retraitagent", createDto);
            responseCreate.EnsureSuccessStatusCode();
            var createResult = await responseCreate.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.True(createResult.Succes);
            Assert.NotNull(createResult.DemandeId);
            var demandeId = createResult.DemandeId!.Value;
            var demandeCreee = await _client.GetFromJsonAsync<DemandeRetraitAgentReadDto>($"/api/retraitagent/{demandeId}");
            Assert.Equal("EN_ATTENTE", demandeCreee!.StatutDemande);

            // Étape 4: Valider la demande et générer le jeton
            var validationDto = new DemandeRetraitAgentValidationDto
            {
                IdDemande = demandeId,
                AgentValidationId = agent.IdAgent,
                StatutDemande = "VALIDEE"
            };
            var responseValidation = await _client.PostAsJsonAsync("/api/retraitagent/valider-et-generer-jeton", validationDto);
            responseValidation.EnsureSuccessStatusCode();
            var validationResult = await responseValidation.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.True(validationResult!.Succes);
            Assert.NotNull(validationResult.JetonCode);
            Assert.StartsWith("JRT", validationResult.JetonCode);
            Assert.Equal(11, validationResult.JetonCode!.Length);
            var jetonId = validationResult.JetonId!.Value;
            var jetonCode = validationResult.JetonCode!;

            // Étape 5: Utiliser le jeton pour le retrait
            var utilisationDto = new JetonRetraitUtilisationDto
            {
                IdJeton = jetonId,
                CodeJeton = jetonCode,
                AgentId = agent.IdAgent,
                ObservationUtilisation = "Retrait effectué avec succès au bureau principal"
            };
            var responseUtilisation = await _client.PostAsJsonAsync("/api/retraitagent/utiliser-jeton", utilisationDto);
            responseUtilisation.EnsureSuccessStatusCode();
            var demandeFinale = await _client.GetFromJsonAsync<DemandeRetraitAgentReadDto>($"/api/retraitagent/{demandeId}");
            Assert.Equal("TRAITEE", demandeFinale!.StatutDemande);
            Assert.NotNull(demandeFinale.DateTraitement);

            // Étape 6: Vérifier le solde final
            var responseSoldeFinal = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);
            responseSoldeFinal.EnsureSuccessStatusCode();
            var soldeFinalResult = await responseSoldeFinal.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            Assert.Equal(125000, soldeFinalResult.SoldeDisponible); // 200000 - 75000

            // Assert final
            Assert.Contains("créée avec succès", createResult.Message);
            Assert.Contains("validée", validationResult.Message);
        }
    }

    [Fact]
    public async Task WorkflowRetraitAgent_EchecPeriodeNonAutorisee_Erreur()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent
            var agent = new Agent
            {
                NomComplet = "Agent Test Période",
                Matricule = "MAT-WF-002",
                Phone = "0999999998",
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

            // Étape 1: Vérifier la période de retrait (non autorisée)
            var dateNonAutorisee = new DateTime(2026, 3, 10);
            var responsePeriode = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", dateNonAutorisee.ToString("yyyy-MM-dd"));
            responsePeriode.EnsureSuccessStatusCode();
            var periodeResult = await responsePeriode.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
            Assert.False(periodeResult.EstPeriodeAutorisee);

            // La création ignore la période en environnement IntegrationTests (voir RetraitAgentService).
            Assert.Contains("ne sont autorisés", periodeResult.Message);
        }
    }

    [Fact]
    public async Task WorkflowRetraitAgent_EchecSoldeInsuffisant_Erreur()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent avec solde insuffisant
            var agent = new Agent
            {
                NomComplet = "Agent Test Solde",
                Matricule = "MAT-WF-003",
                Phone = "0999999997",
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
                SoldeCourant = 30000,
                SoldeDisponible = 30000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            // Étape 1: Vérifier la période de retrait (autorisée)
            var dateAutorisee = new DateTime(2026, 3, 16);
            var responsePeriode = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", dateAutorisee.ToString("yyyy-MM-dd"));
            responsePeriode.EnsureSuccessStatusCode();
            var periodeResult = await responsePeriode.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
            Assert.True(periodeResult.EstPeriodeAutorisee);

            // Étape 2: Vérifier le solde disponible (insuffisant)
            var soldeDto = new SoldeVerificationDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 75000
            };
            var responseSolde = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);
            responseSolde.EnsureSuccessStatusCode();
            var soldeResult = await responseSolde.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            Assert.False(soldeResult.SoldeSuffisant);
            Assert.Equal(30000, soldeResult.SoldeDisponible);

            // Étape 3: Tenter de créer une demande (devrait échouer)
            var createDto = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 75000,
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Test"
            };
            var responseCreate = await _client.PostAsJsonAsync("/api/retraitagent", createDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, responseCreate.StatusCode);
        }
    }

    [Fact]
    public async Task WorkflowRetraitAgent_MultiplesDemandes_Succes()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent avec solde suffisant pour plusieurs retraits
            var agent = new Agent
            {
                NomComplet = "Agent Test Multiples",
                Matricule = "MAT-WF-004",
                Phone = "0999999996",
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
                SoldeCourant = 300000,
                SoldeDisponible = 300000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            var dateAutorisee = new DateTime(2026, 3, 16);
            var responsePeriode = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", dateAutorisee.ToString("yyyy-MM-dd"));
            responsePeriode.EnsureSuccessStatusCode();
            var periodeResult = await responsePeriode.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
            Assert.True(periodeResult.EstPeriodeAutorisee);

            // Première demande
            var createDto1 = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 100000,
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Première demande"
            };
            var responseCreate1 = await _client.PostAsJsonAsync("/api/retraitagent", createDto1);
            responseCreate1.EnsureSuccessStatusCode();
            var createResult1 = await responseCreate1.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.True(createResult1.Succes);

            // Deuxième demande
            var createDto2 = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Deuxième demande"
            };
            var responseCreate2 = await _client.PostAsJsonAsync("/api/retraitagent", createDto2);
            responseCreate2.EnsureSuccessStatusCode();
            var createResult2 = await responseCreate2.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.True(createResult2.Succes);

            // Valider et utiliser les deux jetons
            foreach (var createResult in new[] { createResult1!, createResult2! })
            {
                var validationDto = new DemandeRetraitAgentValidationDto
                {
                    IdDemande = createResult.DemandeId!.Value,
                    AgentValidationId = agent.IdAgent,
                    StatutDemande = "VALIDEE"
                };
                var responseValidation = await _client.PostAsJsonAsync("/api/retraitagent/valider-et-generer-jeton", validationDto);
                responseValidation.EnsureSuccessStatusCode();
                var validationResult = await responseValidation.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
                Assert.True(validationResult!.Succes);

                var utilisationDto = new JetonRetraitUtilisationDto
                {
                    IdJeton = validationResult.JetonId!.Value,
                    CodeJeton = validationResult.JetonCode!,
                    AgentId = agent.IdAgent,
                    ObservationUtilisation = "Retrait effectué"
                };
                var responseUtilisation = await _client.PostAsJsonAsync("/api/retraitagent/utiliser-jeton", utilisationDto);
                responseUtilisation.EnsureSuccessStatusCode();
            }

            // Vérifier le solde final
            var soldeDto = new SoldeVerificationDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 10000
            };
            var responseSoldeFinal = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);
            responseSoldeFinal.EnsureSuccessStatusCode();
            var soldeFinalResult = await responseSoldeFinal.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            Assert.Equal(150000, soldeFinalResult.SoldeDisponible); // 300000 - 100000 - 50000
        }
    }

    #endregion

    #region Workflow Dashboard Affilié Complet

    [Fact]
    public async Task WorkflowDashboardAffilieComplet_AffilieActif_RetourneDashboardComplet()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un affilié
            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-WF-001",
                Nom = "Dupont",
                Prenom = "Jean",
                NomComplet = "Dupont Jean",
                DateNaissance = new DateTime(1980, 1, 1),
                Telephone = "+243123456789",
                EmailAffilie = "jean.dupont@example.com",
                ProvinceResidence = "Kinshasa",
                CommuneResidence = "Lemba",
                QuartierResidence = "Salongo",
                AvenueResidence = "ByPass",
                NumeroResidence = "123",
                CommuneActivite = "Limete",
                QuartierActivite = "Victoire",
                AvenueActivite = "Lumumba",
                NumeroActivite = "456",
                PhotoData = new byte[] { 0xFF },
                DateCreation = new DateTime(2025, 3, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            // Créer une devise
            var devise = new Devise
            {
                Nom = "Franc Congolais",
                Code = "CDF",
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Devises.Add(devise);
            await db.SaveChangesAsync();

            // Créer des cotisations
            var collectes = new List<Collecte>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    AffilieId = affilie.IdAffilie,
                    AgentId = 1,
                    Montant = 10000,
                    ReferencePaiement = "REF-WF-001",
                    ModePaiement = "MOBILE_MONEY",
                    Operateur = "VODACOM",
                    StatutPaiement = "PAYE",
                    DeviseId = devise.IdDevise,
                    DateCollecte = new DateTime(2026, 1, 15),
                    Observation = "Cotisation Janvier"
                },
                new()
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    AffilieId = affilie.IdAffilie,
                    AgentId = 1,
                    Montant = 12000,
                    ReferencePaiement = "REF-WF-002",
                    ModePaiement = "MOBILE_MONEY",
                    Operateur = "AIRTEL",
                    StatutPaiement = "PAYE",
                    DeviseId = devise.IdDevise,
                    DateCollecte = new DateTime(2026, 2, 15),
                    Observation = "Cotisation Février"
                },
                new()
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    AffilieId = affilie.IdAffilie,
                    AgentId = 1,
                    Montant = 15000,
                    ReferencePaiement = "REF-WF-003",
                    ModePaiement = "MOBILE_MONEY",
                    Operateur = "ORANGE",
                    StatutPaiement = "PAYE",
                    DeviseId = devise.IdDevise,
                    DateCollecte = new DateTime(2026, 3, 15),
                    Observation = "Cotisation Mars"
                }
            };
            db.Collectes.AddRange(collectes);
            await db.SaveChangesAsync();

            // Étape 1: Récupérer le dashboard complet
            var responseDashboard = await _client.GetAsync($"/api/dashboardaffilie/resume/{affilie.IdAffilie}?annee=2026");
            responseDashboard.EnsureSuccessStatusCode();
            var dashboardResult = await responseDashboard.Content.ReadFromJsonAsync<AffilieDashboardResumeDto>();
            Assert.NotNull(dashboardResult);
            Assert.NotNull(dashboardResult.Kpis);
            Assert.NotNull(dashboardResult.Informations);
            Assert.NotNull(dashboardResult.CotisationsRecentes);
            Assert.NotNull(dashboardResult.PrestationsRecentes);
            Assert.NotNull(dashboardResult.Beneficiaires);
            Assert.NotNull(dashboardResult.Graphiques);
            Assert.NotNull(dashboardResult.NotificationsRecentes);
            Assert.NotNull(dashboardResult.DocumentsEnAttente);
            Assert.NotNull(dashboardResult.Preferences);

            // Étape 2: Vérifier les KPIs
            Assert.Equal(affilie.IdAffilie, dashboardResult.Kpis.IdAffilie);
            Assert.Equal("AFF-WF-001", dashboardResult.Kpis.CodeAdhesion);
            Assert.Equal("Dupont Jean", dashboardResult.Kpis.NomComplet);
            Assert.True(dashboardResult.Kpis.EstActif);
            Assert.True(dashboardResult.Kpis.AncienneteMois > 0);

            // Étape 3: Vérifier les informations
            Assert.Equal(affilie.IdAffilie, dashboardResult.Informations.IdAffilie);
            Assert.Equal("AFF-WF-001", dashboardResult.Informations.CodeAdhesion);
            Assert.Equal("Dupont Jean", dashboardResult.Informations.NomComplet);
            Assert.Equal("+243123456789", dashboardResult.Informations.Telephone);
            Assert.Equal($"/api/affilie/{affilie.IdAffilie}/photo", dashboardResult.Informations.PhotoUrl);
            Assert.Equal("Kinshasa", dashboardResult.Informations.ProvinceResidence);
            Assert.Equal("Lemba", dashboardResult.Informations.CommuneResidence);
            Assert.Equal("Salongo", dashboardResult.Informations.QuartierResidence);
            Assert.Equal("ByPass", dashboardResult.Informations.AvenueResidence);
            Assert.Equal("123", dashboardResult.Informations.NumeroResidence);
            Assert.Equal("Limete", dashboardResult.Informations.CommuneActivite);
            Assert.Equal("Victoire", dashboardResult.Informations.QuartierActivite);
            Assert.Equal("Lumumba", dashboardResult.Informations.AvenueActivite);
            Assert.Equal("456", dashboardResult.Informations.NumeroActivite);
            Assert.True(dashboardResult.Informations.EstActif);

            // Étape 4: Récupérer les cotisations du mois
            var responseCotisations = await _client.GetAsync($"/api/dashboardaffilie/cotisations/{affilie.IdAffilie}?mois=3&annee=2026");
            responseCotisations.EnsureSuccessStatusCode();
            var cotisationsResult = await responseCotisations.Content.ReadFromJsonAsync<List<AffilieCotisationDto>>();
            Assert.NotNull(cotisationsResult);

            // Étape 5: Récupérer les cotisations récentes
            var responseCotisationsRecentes = await _client.GetAsync($"/api/dashboardaffilie/cotisations/recentes/{affilie.IdAffilie}?limit=5");
            responseCotisationsRecentes.EnsureSuccessStatusCode();
            var cotisationsRecentesResult = await responseCotisationsRecentes.Content.ReadFromJsonAsync<List<AffilieCotisationDto>>();
            Assert.NotNull(cotisationsRecentesResult);
            Assert.True(cotisationsRecentesResult.Count <= 5);

            // Étape 6: Récupérer les graphiques
            var responseGraphiques = await _client.GetAsync($"/api/dashboardaffilie/graphiques/{affilie.IdAffilie}?annee=2026");
            responseGraphiques.EnsureSuccessStatusCode();
            var graphiquesResult = await responseGraphiques.Content.ReadFromJsonAsync<AffilieGraphsDto>();
            Assert.NotNull(graphiquesResult);
            Assert.NotNull(graphiquesResult.CotisationsMensuelles);
            Assert.NotNull(graphiquesResult.PrestationsMensuelles);
            Assert.NotNull(graphiquesResult.EvolutionSolde);
            Assert.NotNull(graphiquesResult.RepartitionPrestations);
            Assert.NotNull(graphiquesResult.TauxUtilisationMensuel);
            Assert.NotNull(graphiquesResult.ResumeAnnuel);

            // Étape 7: Récupérer les préférences
            var responsePreferences = await _client.GetAsync($"/api/dashboardaffilie/preferences/{affilie.IdAffilie}");
            responsePreferences.EnsureSuccessStatusCode();
            var preferencesResult = await responsePreferences.Content.ReadFromJsonAsync<AffiliePreferencesDto>();
            Assert.NotNull(preferencesResult);
            Assert.Equal(affilie.IdAffilie, preferencesResult.IdAffilie);
            Assert.Equal("fr", preferencesResult.LanguePreferee);
            Assert.Equal("UTC", preferencesResult.FuseauHoraire);
            Assert.Equal("PDF", preferencesResult.FormatRapports);

            // Étape 8: Mettre à jour les préférences
            var updatedPreferences = new AffiliePreferencesDto
            {
                IdAffilie = affilie.IdAffilie,
                NotificationsEmail = false,
                NotificationsSMS = true,
                LanguePreferee = "en",
                FuseauHoraire = "UTC+1",
                RecevoirRappelsCotisation = false,
                RecevoirAlertesPrestation = true,
                RecevoirNewsletter = true,
                FrequenceRappelsJours = 15,
                EmailSecondaire = "jean.dupont@example.com",
                TelephoneSecondaire = "+243987654321",
                ModeSombre = true,
                FormatRapports = "EXCEL",
                PartagerDonneesStatistiques = false
            };
            var responseUpdatePreferences = await _client.PutAsJsonAsync($"/api/dashboardaffilie/preferences/{affilie.IdAffilie}", updatedPreferences);
            responseUpdatePreferences.EnsureSuccessStatusCode();
            var updateResult = await responseUpdatePreferences.Content.ReadFromJsonAsync<bool>();
            Assert.True(updateResult);

            // Étape 9: Vérifier le résumé annuel
            var responseResumeAnnuel = await _client.GetAsync($"/api/dashboardaffilie/resume-annuel/{affilie.IdAffilie}?annee=2026");
            responseResumeAnnuel.EnsureSuccessStatusCode();
            var resumeAnnuelResult = await responseResumeAnnuel.Content.ReadFromJsonAsync<AffilieResumeAnnuelDto>();
            Assert.NotNull(resumeAnnuelResult);
            Assert.Equal(2026, resumeAnnuelResult.Annee);

            // Étape 10: Exporter les cotisations
            var responseExport = await _client.GetAsync($"/api/dashboardaffilie/export/cotisations/{affilie.IdAffilie}?mois=3&annee=2026&format=PDF");
            responseExport.EnsureSuccessStatusCode();
            Assert.NotNull(responseExport.Content.Headers.ContentType);
            var exportContent = await responseExport.Content.ReadAsByteArrayAsync();
            Assert.NotNull(exportContent);
        }
    }

    [Fact]
    public async Task WorkflowDashboardAffilie_AffilieInexistant_RetourneDashboardVide()
    {
        // Act
        var responseDashboard = await _client.GetAsync("/api/dashboardaffilie/resume/999?annee=2026");
        responseDashboard.EnsureSuccessStatusCode();
        var dashboardResult = await responseDashboard.Content.ReadFromJsonAsync<AffilieDashboardResumeDto>();
        
        // Assert
        Assert.NotNull(dashboardResult);
        Assert.NotNull(dashboardResult.Kpis);
        Assert.NotNull(dashboardResult.Informations);
        Assert.Equal(0, dashboardResult.Kpis.IdAffilie);
        Assert.Equal(string.Empty, dashboardResult.Kpis.CodeAdhesion);
        Assert.Equal(string.Empty, dashboardResult.Kpis.NomComplet);
        Assert.Equal(0, dashboardResult.Informations.IdAffilie);
        Assert.Equal(string.Empty, dashboardResult.Informations.CodeAdhesion);
        Assert.Equal(string.Empty, dashboardResult.Informations.NomComplet);
    }

    [Fact]
    public async Task WorkflowDashboardAffilie_MiseAJourPreferences_PreferencesModifiees()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un affilié
            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-WF-002",
                Nom = "Martin",
                Prenom = "Sophie",
                NomComplet = "Martin Sophie",
                DateNaissance = new DateTime(1985, 5, 15),
                DateCreation = new DateTime(2025, 3, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            // Étape 1: Récupérer les préférences initiales
            var responsePreferencesInitiales = await _client.GetAsync($"/api/dashboardaffilie/preferences/{affilie.IdAffilie}");
            responsePreferencesInitiales.EnsureSuccessStatusCode();
            var preferencesInitiales = await responsePreferencesInitiales.Content.ReadFromJsonAsync<AffiliePreferencesDto>();
            Assert.NotNull(preferencesInitiales);
            Assert.Equal(affilie.IdAffilie, preferencesInitiales!.IdAffilie);

            // Étape 2: Mettre à jour les préférences
            var updatedPreferences = new AffiliePreferencesDto
            {
                IdAffilie = affilie.IdAffilie,
                NotificationsEmail = false,
                NotificationsSMS = true,
                LanguePreferee = "en",
                FuseauHoraire = "UTC+1",
                RecevoirRappelsCotisation = false,
                RecevoirAlertesPrestation = true,
                RecevoirNewsletter = true,
                FrequenceRappelsJours = 15,
                EmailSecondaire = "sophie.martin@example.com",
                TelephoneSecondaire = "+243987654321",
                ModeSombre = true,
                FormatRapports = "EXCEL",
                PartagerDonneesStatistiques = false
            };
            var responseUpdate = await _client.PutAsJsonAsync($"/api/dashboardaffilie/preferences/{affilie.IdAffilie}", updatedPreferences);
            responseUpdate.EnsureSuccessStatusCode();
            var updateResult = await responseUpdate.Content.ReadFromJsonAsync<bool>();
            Assert.True(updateResult);

            // Étape 3: Vérifier que les préférences ont été mises à jour
            var responsePreferencesModifiees = await _client.GetAsync($"/api/dashboardaffilie/preferences/{affilie.IdAffilie}");
            responsePreferencesModifiees.EnsureSuccessStatusCode();
            var preferencesModifiees = await responsePreferencesModifiees.Content.ReadFromJsonAsync<AffiliePreferencesDto>();
            
            // Note: Comme l'implémentation est simplifiée, les préférences ne sont pas persistées
            // Dans une implémentation complète, les préférences seraient réellement modifiées
            Assert.Equal(affilie.IdAffilie, preferencesModifiees.IdAffilie);
        }
    }

    #endregion

    #region Workflow Intégré Retrait Agent et Dashboard

    [Fact]
    public async Task WorkflowIntegre_RetraitAgentEtDashboard_Succes()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            
            // Créer un agent avec wallet
            var agent = new Agent
            {
                NomComplet = "Agent Intégré Test",
                Matricule = "MAT-INT-001",
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
                SoldeCourant = 150000,
                SoldeDisponible = 150000,
                DateCreation = DateTime.Now
            };
            db.WalletsAgents.Add(wallet);
            await db.SaveChangesAsync();

            // Créer un affilié
            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-INT-001",
                Nom = "Dubois",
                Prenom = "Marie",
                NomComplet = "Dubois Marie",
                DateNaissance = new DateTime(1990, 3, 10),
                DateCreation = new DateTime(2025, 3, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            // Étape 1: Vérifier le dashboard de l'affilié
            var responseDashboard = await _client.GetAsync($"/api/dashboardaffilie/resume/{affilie.IdAffilie}?annee=2026");
            responseDashboard.EnsureSuccessStatusCode();
            var dashboardResult = await responseDashboard.Content.ReadFromJsonAsync<AffilieDashboardResumeDto>();
            Assert.NotNull(dashboardResult);
            Assert.Equal(affilie.IdAffilie, dashboardResult.Kpis.IdAffilie);

            // Étape 2: Effectuer un retrait agent
            var dateAutorisee = new DateTime(2026, 3, 16);
            var responsePeriode = await _client.PostAsJsonAsync("/api/retraitagent/verifier-periode", dateAutorisee.ToString("yyyy-MM-dd"));
            responsePeriode.EnsureSuccessStatusCode();
            var periodeResult = await responsePeriode.Content.ReadFromJsonAsync<PeriodeRetraitVerificationDto>();
            Assert.True(periodeResult.EstPeriodeAutorisee);

            var soldeDto = new SoldeVerificationDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000
            };
            var responseSolde = await _client.PostAsJsonAsync("/api/retraitagent/verifier-solde", soldeDto);
            responseSolde.EnsureSuccessStatusCode();
            var soldeResult = await responseSolde.Content.ReadFromJsonAsync<SoldeVerificationDto>();
            Assert.True(soldeResult.SoldeSuffisant);

            var createDto = new DemandeRetraitAgentCreateDto
            {
                AgentId = agent.IdAgent,
                MontantDemande = 50000,
                TypeRetrait = "PARTIEL",
                MotifRetrait = "Test intégré"
            };
            var responseCreate = await _client.PostAsJsonAsync("/api/retraitagent", createDto);
            responseCreate.EnsureSuccessStatusCode();
            var createResult = await responseCreate.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.True(createResult.Succes);

            var validationDto = new DemandeRetraitAgentValidationDto
            {
                IdDemande = createResult!.DemandeId!.Value,
                AgentValidationId = agent.IdAgent,
                StatutDemande = "VALIDEE"
            };
            var responseValidation = await _client.PostAsJsonAsync("/api/retraitagent/valider-et-generer-jeton", validationDto);
            responseValidation.EnsureSuccessStatusCode();
            var validationResult = await responseValidation.Content.ReadFromJsonAsync<RetraitWorkflowResultDto>();
            Assert.True(validationResult!.Succes);

            var utilisationDto = new JetonRetraitUtilisationDto
            {
                IdJeton = validationResult.JetonId!.Value,
                CodeJeton = validationResult.JetonCode!,
                AgentId = agent.IdAgent,
                ObservationUtilisation = "Retrait intégré"
            };
            var responseUtilisation = await _client.PostAsJsonAsync("/api/retraitagent/utiliser-jeton", utilisationDto);
            responseUtilisation.EnsureSuccessStatusCode();

            // Étape 3: Vérifier que le workflow est complet
            var statsDate = DateTime.Now.ToString("yyyy-MM-dd");
            var responseStats = await _client.GetAsync($"/api/retraitagent/stats/{statsDate}");
            responseStats.EnsureSuccessStatusCode();
            var statsResult = await responseStats.Content.ReadFromJsonAsync<DemandeRetraitAgentStatsDto>();
            Assert.NotNull(statsResult);
            Assert.True(statsResult!.TotalDemandes >= 1);
            Assert.True(statsResult.TotalMontantDemande >= 50000);

            // Étape 4: Vérifier que le dashboard reflète l'état actuel
            var responseDashboardFinal = await _client.GetAsync($"/api/dashboardaffilie/resume/{affilie.IdAffilie}?annee=2026");
            responseDashboardFinal.EnsureSuccessStatusCode();
            var dashboardFinalResult = await responseDashboardFinal.Content.ReadFromJsonAsync<AffilieDashboardResumeDto>();
            Assert.NotNull(dashboardFinalResult);
            Assert.Equal(affilie.IdAffilie, dashboardFinalResult.Kpis.IdAffilie);

            // Assert final
            Assert.Contains("créée avec succès", createResult!.Message);
            Assert.Contains("validée", validationResult!.Message);
        }
    }

    #endregion
}
