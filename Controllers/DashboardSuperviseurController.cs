using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Superviseur")]
    public class DashboardSuperviseurController : ControllerBase
    {
        private readonly ISuperviseurRepository _superviseurService;
        private readonly ILogger<DashboardSuperviseurController> _logger;

        public DashboardSuperviseurController(ISuperviseurRepository superviseurService, ILogger<DashboardSuperviseurController> logger)
        {
            _superviseurService = superviseurService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère le dashboard complet du superviseur
        /// </summary>
        [HttpGet("dashboard/{superviseurId}")]
        public async Task<ActionResult<DashboardSuperviseurDto>> GetDashboardSuperviseur(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard complet pour le superviseur {SuperviseurId}", superviseurId);

                var dashboard = await _superviseurService.GetDashboardSuperviseurAsync(superviseurId, ct);
                
                _logger.LogInformation("Dashboard superviseur récupéré avec succès");
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard superviseur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard superviseur", ex);
            }
        }

        /// <summary>
        /// Récupère les KPIs principaux du superviseur
        /// </summary>
        [HttpGet("kpis/{superviseurId}")]
        public async Task<ActionResult<SuperviseurStatsDto>> GetKpisSuperviseur(int superviseurId, CancellationToken ct, [FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs pour le superviseur {SuperviseurId}", superviseurId);

                var kpis = await _superviseurService.GetStatsSuperviseurAsync(superviseurId, ct);
                
                _logger.LogInformation("KPIs du superviseur récupérés avec succès");
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs du superviseur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs du superviseur", ex);
            }
        }

        /// <summary>
        /// Récupère le top 5 des agents par performance
        /// </summary>
        [HttpGet("top-agents/{superviseurId}")]
        public async Task<ActionResult<List<AgentPerformanceHierarchieDto>>> GetTopAgentsDashboard(int superviseurId, CancellationToken ct, [FromQuery] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Récupération du top agents pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var topAgents = await _superviseurService.GetTopAgentsAsync(superviseurId, 5, ct);
                
                _logger.LogInformation("Top agents du dashboard récupérés: {Count} agents", topAgents.Count);
                return Ok(topAgents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du top agents du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du top agents du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère les tendances des 6 derniers mois
        /// </summary>
        [HttpGet("tendances/{superviseurId}")]
        public async Task<ActionResult<List<TendanceEquipeDto>>> GetTendancesDashboard(int superviseurId, CancellationToken ct, [FromQuery] int mois = 6)
        {
            try
            {
                _logger.LogInformation("Récupération des tendances pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var tendances = await _superviseurService.GetTendancesEquipeAsync(superviseurId, 6, ct);
                
                _logger.LogInformation("Tendances du dashboard récupérées: {Count} mois", tendances.Count);
                return Ok(tendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des tendances du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère les objectifs actifs de l'équipe
        /// </summary>
        [HttpGet("objectifs/{superviseurId}")]
        public async Task<ActionResult<List<ObjectifEquipeDto>>> GetObjectifsDashboard(int superviseurId, CancellationToken ct, [FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Récupération des objectifs pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var objectifs = await _superviseurService.GetObjectifsEquipeAsync(superviseurId, ct);
                
                _logger.LogInformation("Objectifs du dashboard récupérés: {Count} objectifs", objectifs.Count);
                return Ok(objectifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des objectifs du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère le rapport de performance du mois en cours
        /// </summary>
        [HttpGet("rapport-mois/{superviseurId}")]
        public async Task<ActionResult<RapportPerformanceEquipeDto>> GetRapportMois(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du rapport du mois pour le superviseur {SuperviseurId}", superviseurId);

                var debut = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                var rapport = await _superviseurService.GetRapportPerformanceAsync(superviseurId, debut, fin, ct);
                
                _logger.LogInformation("Rapport du mois récupéré avec succès");
                return Ok(rapport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du rapport du mois");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du rapport du mois", ex);
            }
        }

        /// <summary>
        /// Récupère la hiérarchie complète pour le dashboard
        /// </summary>
        [HttpGet("hierarchie-dashboard/{superviseurId}")]
        public async Task<ActionResult<HierarchieSuperviseurDto>> GetHierarchieDashboard(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de la hiérarchie pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var hierarchie = await _superviseurService.GetHierarchieCompleteAsync(superviseurId, ct);
                
                _logger.LogInformation("Hiérarchie du dashboard récupérée avec succès");
                return Ok(hierarchie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la hiérarchie du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la hiérarchie du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère l'activité journalière du superviseur
        /// </summary>
        [HttpGet("activite-journaliere/{superviseurId}")]
        public async Task<ActionResult<ActiviteSuperviseurDto>> GetActiviteJournaliereDashboard(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de l'activité journalière pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var activite = await _superviseurService.GetActiviteJournaliereAsync(superviseurId, ct);
                
                _logger.LogInformation("Activité journalière du dashboard récupérée avec succès");
                return Ok(activite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'activité journalière du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'activité journalière du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère les performances détaillées de tous les agents
        /// </summary>
        [HttpGet("performances-detaillees/{superviseurId}")]
        public async Task<ActionResult<List<AgentPerformanceHierarchieDto>>> GetPerformancesDetaillees(int superviseurId, CancellationToken ct, [FromQuery] int limit = 50)
        {
            try
            {
                _logger.LogInformation("Récupération des performances détaillées pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var performances = await _superviseurService.GetPerformancesAgentsAsync(superviseurId, ct);
                
                _logger.LogInformation("Performances détaillées du dashboard récupérées: {Count} agents", performances.Count);
                return Ok(performances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances détaillées du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des performances détaillées du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère le résumé des alertes de l'équipe
        /// </summary>
        [HttpGet("alertes-resume/{superviseurId}")]
        public async Task<ActionResult<List<string>>> GetAlertesResume(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du résumé des alertes pour le dashboard du superviseur {SuperviseurId}", superviseurId);

                var alertes = await _superviseurService.GetAlertesEquipeAsync(superviseurId, ct);
                
                _logger.LogInformation("Alertes du dashboard récupérées: {Count} alertes", alertes.Count);
                return Ok(alertes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé des alertes du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du résumé des alertes du dashboard", ex);
            }
        }

        /// <summary>
        /// Exporte le dashboard complet en Excel
        /// </summary>
        [HttpGet("export-dashboard/{superviseurId}")]
        public async Task<ActionResult<byte[]>> ExportDashboard(int superviseurId, CancellationToken ct, [FromQuery] string format = "Excel")
        {
            try
            {
                _logger.LogInformation("Export du dashboard pour le superviseur {SuperviseurId}", superviseurId);

                var debut = DateTime.Now.AddMonths(-1);
                var fin = DateTime.Now;
                var data = await _superviseurService.ExporterDonneesEquipeAsync(superviseurId, debut, fin, "Excel", ct);
                
                _logger.LogInformation("Dashboard exporté avec succès");
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"dashboard_superviseur_{superviseurId}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de l'export du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère les métriques clés pour les widgets
        /// </summary>
        [HttpGet("metriques-widgets/{superviseurId}")]
        public async Task<ActionResult<object>> GetMetriquesWidgets(int superviseurId, CancellationToken ct, [FromQuery] int jours = 30)
        {
            try
            {
                _logger.LogInformation("Récupération des métriques pour widgets du superviseur {SuperviseurId}", superviseurId);

                var stats = await _superviseurService.GetStatsSuperviseurAsync(superviseurId, ct);
                var tendances = await _superviseurService.GetTendancesEquipeAsync(superviseurId, 3, ct);

                var metriques = new
                {
                    // Métriques principales
                    totalAgents = stats.NombreAgentsTotal,
                    montantTotalEquipe = stats.MontantTotalEquipe,
                    performanceMoyenne = stats.PerformanceMoyenneEquipe,
                    tauxSucces = stats.TauxSuccesEquipe,
                    objectifAtteint = stats.AtteinteObjectifEquipe >= 100,
                    
                    // Métriques de tendance
                    croissanceMensuelle = tendances.Count > 1 ? tendances.Last().Croissance : 0,
                    progressionMensuelle = tendances.Count > 1 ? tendances.Last().AtteinteObjectifPeriode : 0,
                    
                    // Métriques d'activité
                    agentsActifs = stats.NombreAgentsDirects,
                    transactionsAujourdhui = stats.NombreTransactionsSuperviseur,
                    montantAujourdhui = stats.MontantTotalSuperviseur,
                    
                    // Alertes
                    nombreAlertes = 0, // Simulé
                    dernieresAlertes = new List<string>() // Simulé
                };

                _logger.LogInformation("Métriques pour widgets récupérées avec succès");
                return Ok(metriques);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des métriques pour widgets");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des métriques pour widgets", ex);
            }
        }

        /// <summary>
        /// Récupère le classement des agents de l'équipe
        /// </summary>
        [HttpGet("classement-agents/{superviseurId}")]
        public async Task<ActionResult<List<AgentPerformanceHierarchieDto>>> GetClassementAgents(int superviseurId, CancellationToken ct, [FromQuery] int limit = 20)
        {
            try
            {
                _logger.LogInformation("Récupération du classement des agents pour le superviseur {SuperviseurId}", superviseurId);

                var performances = await _superviseurService.GetPerformancesAgentsAsync(superviseurId, ct);
                var classement = performances
                    .OrderByDescending(p => p.MontantTotal)
                    .ThenByDescending(p => p.TauxSucces)
                    .ToList();

                // Mise à jour des rangs
                for (int i = 0; i < classement.Count; i++)
                {
                    classement[i].RangEquipe = i + 1;
                }

                _logger.LogInformation("Classement des agents récupéré: {Count} agents", classement.Count);
                return Ok(classement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du classement des agents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du classement des agents", ex);
            }
        }

        /// <summary>
        /// Récupère les statistiques comparatives avec le mois précédent
        /// </summary>
        [HttpGet("comparaison-mois/{superviseurId}")]
        public async Task<ActionResult<object>> GetComparaisonMois(int superviseurId, CancellationToken ct, [FromQuery] int moisPrecedent = 1)
        {
            try
            {
                _logger.LogInformation("Récupération de la comparaison mensuelle pour le superviseur {SuperviseurId}", superviseurId);

                var statsActuel = await _superviseurService.GetStatsSuperviseurAsync(superviseurId, ct);
                
                // Calcul du mois précédent
                var moisPrecedentDate = DateTime.Now.AddMonths(-moisPrecedent);
                var debutPrecedent = new DateTime(moisPrecedentDate.Year, moisPrecedentDate.Month, 1);
                var finPrecedent = new DateTime(moisPrecedentDate.Year, moisPrecedentDate.Month, DateTime.DaysInMonth(moisPrecedentDate.Year, moisPrecedentDate.Month));
                
                var statsPrecedent = await _superviseurService.GetStatsSuperviseurAsync(superviseurId, ct);
                
                var comparaison = new
                {
                    moisActuel = DateTime.Now.ToString("yyyy-MM"),
                    moisPrecedent = moisPrecedentDate.ToString("yyyy-MM"),
                    
                    // Comparaison des montants
                    montantActuel = statsActuel.MontantTotalEquipe,
                    montantPrecedent = statsPrecedent.MontantTotalEquipe,
                    evolutionMontant = statsPrecedent.MontantTotalEquipe > 0 ? 
                        ((statsActuel.MontantTotalEquipe - statsPrecedent.MontantTotalEquipe) / statsPrecedent.MontantTotalEquipe) * 100 : 0,
                    
                    // Comparaison des transactions
                    transactionsActuel = statsActuel.NombreTransactionsSuperviseur,
                    transactionsPrecedent = statsPrecedent.NombreTransactionsSuperviseur,
                    evolutionTransactions = statsPrecedent.NombreTransactionsSuperviseur > 0 ? 
                        ((statsActuel.NombreTransactionsSuperviseur - statsPrecedent.NombreTransactionsSuperviseur) / (decimal)statsPrecedent.NombreTransactionsSuperviseur) * 100 : 0,
                    
                    // Comparaison des taux de succès
                    tauxSuccesActuel = statsActuel.TauxSuccesEquipe,
                    tauxSuccesPrecedent = statsPrecedent.TauxSuccesEquipe,
                    evolutionTauxSucces = statsPrecedent.TauxSuccesEquipe > 0 ? 
                        (statsActuel.TauxSuccesEquipe - statsPrecedent.TauxSuccesEquipe) : 0,
                    
                    // Comparaison des objectifs
                    objectifAtteintActuel = statsActuel.AtteinteObjectifEquipe >= 100,
                    objectifAtteintPrecedent = statsPrecedent.AtteinteObjectifEquipe >= 100
                };

                _logger.LogInformation("Comparaison mensuelle récupérée avec succès");
                return Ok(comparaison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la comparaison mensuelle");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la comparaison mensuelle", ex);
            }
        }

        /// <summary>
        /// Récupère les indicateurs de performance clés
        /// </summary>
        [HttpGet("indicateurs-performance/{superviseurId}")]
        public async Task<ActionResult<object>> GetIndicateursPerformance(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des indicateurs de performance pour le superviseur {SuperviseurId}", superviseurId);

                var stats = await _superviseurService.GetStatsSuperviseurAsync(superviseurId, ct);
                
                var indicateurs = new
                {
                    // Indicateurs de performance
                    performanceGlobale = stats.AtteinteObjectifEquipe >= 100 ? "Excellent" : 
                                     stats.AtteinteObjectifEquipe >= 80 ? "Bon" : 
                                     stats.AtteinteObjectifEquipe >= 60 ? "Moyen" : "À améliorer",
                    
                    // Indicateurs de tendance
                    tendancePerformance = stats.AtteinteObjectifEquipe >= 100 ? "En hausse" : "Stable",
                    
                    // Indicateurs d'efficacité
                    efficaciteEquipe = stats.TauxSuccesEquipe >= 90 ? "Très efficace" : 
                                    stats.TauxSuccesEquipe >= 75 ? "Efficace" : 
                                    stats.TauxSuccesEquipe >= 50 ? "Modérée" : "À améliorer",
                    
                    // Indicateurs d'activité
                    niveauActivite = stats.NombreAgentsDirects > 10 ? "Élevée" : 
                                   stats.NombreAgentsDirects > 5 ? "Normale" : "Faible",
                    
                    // Indicateurs de risque
                    niveauRisque = stats.AtteinteObjectifEquipe < 50 ? "Élevé" : 
                                  stats.AtteinteObjectifEquipe < 75 ? "Modéré" : "Faible",
                    
                    // Recommandations
                    recommandations = stats.AtteinteObjectifEquipe < 80 ? 
                        new List<string> { "Renforcer l'accompagnement", "Analyser les blocages", "Optimiser les processus" } :
                        new List<string> { "Maintenir le cap", "Partager les bonnes pratiques" }
                };

                _logger.LogInformation("Indicateurs de performance récupérés avec succès");
                return Ok(indicateurs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des indicateurs de performance");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des indicateurs de performance", ex);
            }
        }
    }
}
