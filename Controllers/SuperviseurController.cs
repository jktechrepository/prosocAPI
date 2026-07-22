using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Superviseur")]
    public class SuperviseurController : BaseApiController
    {
        private readonly ISuperviseurRepository _superviseurService;
        private readonly ProsocDbContext _db;

        public SuperviseurController(
            ISuperviseurRepository superviseurService,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<SuperviseurController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _superviseurService = superviseurService;
            _db = db;
        }

        /// <summary>
        /// Récupère les statistiques générales du superviseur
        /// </summary>
        [HttpGet("stats/{superviseurId}")]
        public async Task<ActionResult<SuperviseurStatsDto>> GetStatsSuperviseur(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des statistiques du superviseur {SuperviseurId}", superviseurId);

                var stats = await _superviseurService.GetStatsSuperviseurAsync(superviseurId, ct);
                
                _logger.LogInformation("Statistiques du superviseur récupérées avec succès");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques du superviseur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des statistiques du superviseur", ex);
            }
        }

        /// <summary>
        /// Récupère les performances des agents supervisés
        /// </summary>
        [HttpGet("performances-agents/{superviseurId}")]
        public async Task<ActionResult<List<AgentPerformanceDto>>> GetPerformancesAgents(int superviseurId, CancellationToken ct, [FromQuery] int limit = 50)
        {
            try
            {
                _logger.LogInformation("Récupération des performances des agents pour le superviseur {SuperviseurId}", superviseurId);

                var performances = await _superviseurService.GetPerformancesAgentsAsync(superviseurId, ct);
                
                _logger.LogInformation("Performances des agents récupérées: {Count} agents", performances.Count);
                return Ok(performances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances des agents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des performances des agents", ex);
            }
        }

        /// <summary>
        /// Récupère la performance d'un agent spécifique
        /// </summary>
        [HttpGet("performance-agent/{superviseurId}/{agentId}")]
        public async Task<ActionResult<AgentPerformanceDto>> GetPerformanceAgent(int superviseurId, int agentId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de la performance de l'agent {AgentId} pour le superviseur {SuperviseurId}", agentId, superviseurId);

                var performance = await _superviseurService.GetPerformanceAgentAsync(superviseurId, agentId, ct);
                
                _logger.LogInformation("Performance de l'agent récupérée avec succès");
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance de l'agent");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la performance de l'agent", ex);
            }
        }

        /// <summary>
        /// Récupère le top des agents par performance
        /// </summary>
        [HttpGet("top-agents/{superviseurId}")]
        public async Task<ActionResult<List<AgentPerformanceDto>>> GetTopAgents(int superviseurId, CancellationToken ct, [FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Récupération du top {Limit} agents pour le superviseur {SuperviseurId}", limit, superviseurId);

                var topAgents = await _superviseurService.GetTopAgentsAsync(superviseurId, limit, ct);
                
                _logger.LogInformation("Top agents récupérés: {Count} agents", topAgents.Count);
                return Ok(topAgents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du top agents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du top agents", ex);
            }
        }

        /// <summary>
        /// Récupère la hiérarchie complète du superviseur
        /// </summary>
        [HttpGet("hierarchie/{superviseurId}")]
        public async Task<ActionResult<HierarchieSuperviseurDto>> GetHierarchieComplete(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de la hiérarchie complète pour le superviseur {SuperviseurId}", superviseurId);

                var hierarchie = await _superviseurService.GetHierarchieCompleteAsync(superviseurId, ct);
                
                _logger.LogInformation("Hiérarchie complète récupérée avec succès");
                return Ok(hierarchie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la hiérarchie complète");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la hiérarchie complète", ex);
            }
        }

        /// <summary>
        /// Récupère les agents supervisés directs
        /// </summary>
        [HttpGet("agents-directs/{superviseurId}")]
        public async Task<ActionResult<List<AgentHierarchieDto>>> GetAgentsSupervisesDirects(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des agents supervisés directs pour le superviseur {SuperviseurId}", superviseurId);

                var agents = await _superviseurService.GetAgentsSupervisesDirectsAsync(superviseurId, ct);
                
                _logger.LogInformation("Agents supervisés directs récupérés: {Count} agents", agents.Count);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des agents supervisés directs");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des agents supervisés directs", ex);
            }
        }

        /// <summary>
        /// Récupère tous les agents de la hiérarchie
        /// </summary>
        [HttpGet("tous-agents-hierarchie/{superviseurId}")]
        public async Task<ActionResult<List<AgentHierarchieDto>>> GetTousAgentsHierarchie(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de tous les agents de la hiérarchie pour le superviseur {SuperviseurId}", superviseurId);

                var agents = await _superviseurService.GetTousAgentsHierarchieAsync(superviseurId, ct);
                
                _logger.LogInformation("Tous les agents de la hiérarchie récupérés: {Count} agents", agents.Count);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les agents de la hiérarchie");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de tous les agents de la hiérarchie", ex);
            }
        }

        /// <summary>
        /// Vérifie si un agent est dans la hiérarchie du superviseur
        /// </summary>
        [HttpGet("est-dans-hierarchie/{superviseurId}/{agentId}")]
        public async Task<ActionResult<bool>> EstDansHierarchie(int superviseurId, int agentId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Vérification de l'appartenance à la hiérarchie: superviseur {SuperviseurId}, agent {AgentId}", superviseurId, agentId);

                var estDansHierarchie = await _superviseurService.EstDansHierarchieAsync(superviseurId, agentId, ct);
                
                _logger.LogInformation("Vérification de hiérarchie: {Result}", estDansHierarchie);
                return Ok(estDansHierarchie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'appartenance à la hiérarchie");
                return this.TechnicalErrorResponse("Erreur lors de la vérification de l'appartenance à la hiérarchie", ex);
            }
        }

        /// <summary>
        /// Récupère les affectations récentes
        /// </summary>
        [HttpGet("affectations-recentes/{superviseurId}")]
        public async Task<ActionResult<List<AffectationSuperviseurDto>>> GetAffectationsRecentes(int superviseurId, CancellationToken ct, [FromQuery] int limit = 20)
        {
            try
            {
                _logger.LogInformation("Récupération des affectations récentes pour le superviseur {SuperviseurId}", superviseurId);

                var affectations = await _superviseurService.GetAffectationsRecentesAsync(superviseurId, limit, ct);
                
                _logger.LogInformation("Affectations récentes récupérées: {Count} affectations", affectations.Count);
                return Ok(affectations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des affectations récentes");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des affectations récentes", ex);
            }
        }

        /// <summary>
        /// Récupère l'historique des affectations d'un agent
        /// </summary>
        [HttpGet("historique-affectations/{agentId}")]
        public async Task<ActionResult<List<AffectationSuperviseurDto>>> GetHistoriqueAffectations(int agentId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de l'historique des affectations pour l'agent {AgentId}", agentId);

                var historique = await _superviseurService.GetHistoriqueAffectationsAsync(agentId, ct);
                
                _logger.LogInformation("Historique des affectations récupéré: {Count} affectations", historique.Count);
                return Ok(historique);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'historique des affectations");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'historique des affectations", ex);
            }
        }

        /// <summary>
        /// Récupère les objectifs d'équipe
        /// </summary>
        [HttpGet("objectifs-equipe/{superviseurId}")]
        public async Task<ActionResult<List<ObjectifEquipeDto>>> GetObjectifsEquipe(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des objectifs d'équipe pour le superviseur {SuperviseurId}", superviseurId);

                var objectifs = await _superviseurService.GetObjectifsEquipeAsync(superviseurId, ct);
                
                _logger.LogInformation("Objectifs d'équipe récupérés: {Count} objectifs", objectifs.Count);
                return Ok(objectifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des objectifs d'équipe", ex);
            }
        }

        /// <summary>
        /// Crée un nouvel objectif d'équipe
        /// </summary>
        [HttpPost("creer-objectif")]
        public async Task<ActionResult<ObjectifEquipeDto>> CreerObjectifEquipe([FromBody] ObjectifEquipeDto objectif, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Création d'un nouvel objectif d'équipe pour le superviseur {SuperviseurId}", objectif.SuperviseurId);

                var resultat = await _superviseurService.CreerObjectifEquipeAsync(objectif, ct);
                
                _logger.LogInformation("Objectif d'équipe créé avec succès");
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'objectif d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la création de l'objectif d'équipe", ex);
            }
        }

        /// <summary>
        /// Modifie un objectif d'équipe existant
        /// </summary>
        [HttpPut("modifier-objectif/{objectifId}")]
        public async Task<ActionResult<bool>> ModifierObjectifEquipe(int objectifId, [FromBody] ObjectifEquipeDto objectif, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Modification de l'objectif d'équipe {ObjectifId}", objectifId);

                var resultat = await _superviseurService.ModifierObjectifEquipeAsync(objectifId, objectif, ct);
                
                _logger.LogInformation("Objectif d'équipe modifié avec succès: {Result}", resultat);
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de l'objectif d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la modification de l'objectif d'équipe", ex);
            }
        }

        /// <summary>
        /// Supprime un objectif d'équipe
        /// </summary>
        [HttpDelete("supprimer-objectif/{objectifId}")]
        public async Task<ActionResult<bool>> SupprimerObjectifEquipe(int objectifId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Suppression de l'objectif d'équipe {ObjectifId}", objectifId);

                var resultat = await _superviseurService.SupprimerObjectifEquipeAsync(objectifId, ct);
                
                _logger.LogInformation("Objectif d'équipe supprimé avec succès: {Result}", resultat);
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'objectif d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la suppression de l'objectif d'équipe", ex);
            }
        }

        /// <summary>
        /// Génère un rapport de performance
        /// </summary>
        [HttpPost("rapport-performance")]
        public async Task<ActionResult<RapportPerformanceEquipeDto>> GetRapportPerformance([FromBody] RapportRequestDto request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Génération du rapport de performance pour le superviseur {SuperviseurId}", request.SuperviseurId);

                var rapport = await _superviseurService.GetRapportPerformanceAsync(request.SuperviseurId, request.Debut, request.Fin, ct);
                
                _logger.LogInformation("Rapport de performance généré avec succès");
                return Ok(rapport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du rapport de performance");
                return this.TechnicalErrorResponse("Erreur lors de la génération du rapport de performance", ex);
            }
        }

        /// <summary>
        /// Récupère les rapports périodiques
        /// </summary>
        [HttpGet("rapports-periodiques/{superviseurId}")]
        public async Task<ActionResult<List<RapportPerformanceEquipeDto>>> GetRapportsPeriodiques(int superviseurId, CancellationToken ct, [FromQuery] int mois = 6)
        {
            try
            {
                _logger.LogInformation("Récupération des rapports périodiques pour le superviseur {SuperviseurId}", superviseurId);

                var rapports = await _superviseurService.GetRapportsPeriodiquesAsync(superviseurId, mois, ct);
                
                _logger.LogInformation("Rapports périodiques récupérés: {Count} rapports", rapports.Count);
                return Ok(rapports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des rapports périodiques");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des rapports périodiques", ex);
            }
        }

        /// <summary>
        /// Exporte un rapport de performance
        /// </summary>
        [HttpPost("exporter-rapport")]
        public async Task<ActionResult<byte[]>> ExporterRapportPerformance([FromBody] RapportRequestDto request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Export du rapport de performance pour le superviseur {SuperviseurId}", request.SuperviseurId);

                var data = await _superviseurService.ExporterRapportPerformanceAsync(request.SuperviseurId, request.Debut, request.Fin, ct);
                
                _logger.LogInformation("Rapport de performance exporté avec succès");
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"rapport_performance_{request.SuperviseurId}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export du rapport de performance");
                return this.TechnicalErrorResponse("Erreur lors de l'export du rapport de performance", ex);
            }
        }

        /// <summary>
        /// Compare les performances entre équipes
        /// </summary>
        [HttpPost("comparer-equipes")]
        public async Task<ActionResult<ComparaisonEquipesDto>> GetComparaisonEquipes([FromBody] ComparaisonRequestDto request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Comparaison des équipes pour {Count} superviseurs", request.SuperviseurIds.Count);

                var comparaison = await _superviseurService.GetComparaisonEquipesAsync(request.SuperviseurIds, request.Debut, request.Fin, ct);
                
                _logger.LogInformation("Comparaison des équipes réalisée avec succès");
                return Ok(comparaison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la comparaison des équipes");
                return this.TechnicalErrorResponse("Erreur lors de la comparaison des équipes", ex);
            }
        }

        /// <summary>
        /// Récupère le classement des superviseurs
        /// </summary>
        [HttpGet("classement-superviseurs")]
        public async Task<ActionResult<List<SuperviseurStatsDto>>> GetClassementSuperviseurs([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération du classement des superviseurs du {Debut} au {Fin}", debut, fin);

                var classement = await _superviseurService.GetClassementSuperviseursAsync(debut, fin, ct);
                
                _logger.LogInformation("Classement des superviseurs récupéré: {Count} superviseurs", classement.Count);
                return Ok(classement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du classement des superviseurs");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du classement des superviseurs", ex);
            }
        }

        /// <summary>
        /// Récupère les tendances de l'équipe
        /// </summary>
        [HttpGet("tendances-equipe/{superviseurId}")]
        public async Task<ActionResult<List<TendanceEquipeDto>>> GetTendancesEquipe(int superviseurId, CancellationToken ct, [FromQuery] int mois = 12)
        {
            try
            {
                _logger.LogInformation("Récupération des tendances de l'équipe pour le superviseur {SuperviseurId}", superviseurId);

                var tendances = await _superviseurService.GetTendancesEquipeAsync(superviseurId, mois, ct);
                
                _logger.LogInformation("Tendances de l'équipe récupérées: {Count} mois", tendances.Count);
                return Ok(tendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances de l'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des tendances de l'équipe", ex);
            }
        }

        /// <summary>
        /// Récupère les tendances générales
        /// </summary>
        [HttpGet("tendances-generales")]
        public async Task<ActionResult<List<TendanceEquipeDto>>> GetTendancesGenerales(CancellationToken ct, [FromQuery] int mois = 12)
        {
            try
            {
                _logger.LogInformation("Récupération des tendances générales sur {Mois} mois", mois);

                var tendances = await _superviseurService.GetTendancesGeneralesAsync(mois, ct);
                
                _logger.LogInformation("Tendances générales récupérées: {Count} mois", tendances.Count);
                return Ok(tendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances générales");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des tendances générales", ex);
            }
        }

        /// <summary>
        /// Récupère l'activité journalière du superviseur
        /// </summary>
        [HttpGet("activite-journaliere/{superviseurId}")]
        public async Task<ActionResult<ActiviteSuperviseurDto>> GetActiviteJournaliere(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de l'activité journalière pour le superviseur {SuperviseurId}", superviseurId);

                var activite = await _superviseurService.GetActiviteJournaliereAsync(superviseurId, ct);
                
                _logger.LogInformation("Activité journalière récupérée avec succès");
                return Ok(activite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'activité journalière");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'activité Journalière", ex);
            }
        }

        /// <summary>
        /// Récupère l'activité périodique
        /// </summary>
        [HttpPost("activite-periodique")]
        public async Task<ActionResult<List<ActiviteSuperviseurDto>>> GetActivitePeriodique([FromBody] ActiviteRequestDto request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération de l'activité périodique pour le superviseur {SuperviseurId}", request.SuperviseurId);

                var activites = await _superviseurService.GetActivitePeriodiqueAsync(request.SuperviseurId, request.Debut, request.Fin, ct);
                
                _logger.LogInformation("Activité périodique récupérée: {Count} jours", activites.Count);
                return Ok(activites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'activité périodique");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'activité périodique", ex);
            }
        }

        /// <summary>
        /// Récupère les permissions du superviseur
        /// </summary>
        [HttpGet("permissions/{superviseurId}")]
        public async Task<ActionResult<PermissionSuperviseurDto>> GetPermissionsSuperviseur(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des permissions pour le superviseur {SuperviseurId}", superviseurId);

                var permissions = await _superviseurService.GetPermissionsSuperviseurAsync(superviseurId, ct);
                
                _logger.LogInformation("Permissions du superviseur récupérées avec succès");
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des permissions du superviseur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des permissions du superviseur", ex);
            }
        }

        /// <summary>
        /// Modifie les permissions du superviseur
        /// </summary>
        [HttpPut("modifier-permissions/{superviseurId}")]
        public async Task<ActionResult<bool>> ModifierPermissionsSuperviseur(int superviseurId, [FromBody] PermissionSuperviseurDto permissions, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Modification des permissions pour le superviseur {SuperviseurId}", superviseurId);

                var resultat = await _superviseurService.ModifierPermissionsSuperviseurAsync(superviseurId, permissions, ct);
                
                _logger.LogInformation("Permissions du superviseur modifiées avec succès: {Result}", resultat);
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification des permissions du superviseur");
                return this.TechnicalErrorResponse("Erreur lors de la modification des permissions du superviseur", ex);
            }
        }

        /// <summary>
        /// Récupère les alertes de l'équipe
        /// </summary>
        [HttpGet("alertes-equipe/{superviseurId}")]
        public async Task<ActionResult<List<string>>> GetAlertesEquipe(int superviseurId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Récupération des alertes pour l'équipe du superviseur {SuperviseurId}", superviseurId);

                var alertes = await _superviseurService.GetAlertesEquipeAsync(superviseurId, ct);
                
                _logger.LogInformation("Alertes de l'équipe récupérées: {Count} alertes", alertes.Count);
                return Ok(alertes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des alertes de l'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des alertes de l'équipe", ex);
            }
        }

        /// <summary>
        /// Crée une alerte d'équipe
        /// </summary>
        [HttpPost("creer-alerte")]
        public async Task<ActionResult<bool>> CreerAlerteEquipe([FromBody] AlerteRequestDto request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Création d'alerte pour l'équipe du superviseur {SuperviseurId}", request.SuperviseurId);

                var resultat = await _superviseurService.CreerAlerteEquipeAsync(request.SuperviseurId, request.Message, ct);
                
                _logger.LogInformation("Alerte d'équipe créée avec succès: {Result}", resultat);
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'alerte d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la création de l'alerte d'équipe", ex);
            }
        }

        /// <summary>
        /// Exporte les données de l'équipe
        /// </summary>
        [HttpPost("exporter-donnees-equipe")]
        public async Task<ActionResult<byte[]>> ExporterDonneesEquipe([FromBody] ExportRequestDto request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Export des données de l'équipe pour le superviseur {SuperviseurId}", request.SuperviseurId);

                var data = await _superviseurService.ExporterDonneesEquipeAsync(request.SuperviseurId, request.Debut, request.Fin, request.Format, ct);
                
                _logger.LogInformation("Données de l'équipe exportées avec succès");
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"donnees_equipe_{request.SuperviseurId}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des données de l'équipe");
                return this.TechnicalErrorResponse("Erreur lors de l'export des données de l'équipe", ex);
            }
        }
    }

    // DTOs pour les requêtes complexes
    public class RapportRequestDto
    {
        public int SuperviseurId { get; set; }
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
    }

    public class ComparaisonRequestDto
    {
        public List<int> SuperviseurIds { get; set; } = new List<int>();
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
    }

    public class ActiviteRequestDto
    {
        public int SuperviseurId { get; set; }
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
    }

    public class AlerteRequestDto
    {
        public int SuperviseurId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ExportRequestDto
    {
        public int SuperviseurId { get; set; }
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
        public string Format { get; set; } = "Excel";
    }
}
