using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Agent (AT),Chef d'équipe")]
    public class DashboardAgentController : ControllerBase
    {
        private readonly IDashboardAgentRepository _dashboardService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardAgentController> _logger;

        public DashboardAgentController(
            IDashboardAgentRepository dashboardService,
            ProsocDbContext db,
            ILogger<DashboardAgentController> logger)
        {
            _dashboardService = dashboardService;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard consolidé AT — primes, commissions, suivi adhérents (Remarque 5/6).
        /// </summary>
        [HttpGet("terrain")]
        public async Task<ActionResult<AgentTerrainDashboardDto>> GetDashboardTerrain(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                return Ok(await _dashboardService.GetDashboardTerrainAsync(agentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dashboard terrain agent");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard terrain", ex);
            }
        }

        /// <summary>Primes générées (souscriptions) par l'agent.</summary>
        [HttpGet("primes-generees")]
        public async Task<ActionResult<AgentPrimesResumeDto>> GetPrimesGenerees(
            [FromQuery] int mois,
            [FromQuery] int annee,
            [FromQuery] int limit,
            CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                if (limit == 0) limit = 50;
                return Ok(await _dashboardService.GetPrimesGenereesAsync(agentId, mois, annee, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur primes générées");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des primes", ex);
            }
        }

        /// <summary>Résumé commissions wallet + mouvements récents.</summary>
        [HttpGet("commissions-resume")]
        public async Task<ActionResult<AgentCommissionsResumeDto>> GetCommissionsResume(
            [FromQuery] int limit,
            CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                if (limit == 0) limit = 20;
                return Ok(await _dashboardService.GetCommissionsResumeAsync(agentId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur commissions résumé");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des commissions", ex);
            }
        }

        /// <summary>Suivi des adhérents (statut dossier, cotisation, alertes).</summary>
        [HttpGet("suivi-adherents")]
        public async Task<ActionResult<List<AgentSuiviAdherentDto>>> GetSuiviAdherents(
            [FromQuery] int limit,
            [FromQuery] string? statutGlobal,
            CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                if (limit == 0) limit = 50;
                return Ok(await _dashboardService.GetSuiviAdherentsAsync(agentId, limit, statutGlobal, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur suivi adhérents");
                return this.TechnicalErrorResponse("Erreur lors du suivi des adhérents", ex);
            }
        }

        /// <summary>
        /// Récupère les KPIs personnels de l'agent connecté
        /// </summary>
        [HttpGet("kpis")]
        public async Task<ActionResult<AgentKpisDto>> GetKpis(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                _logger.LogInformation("Récupération des KPIs pour l'agent {AgentId}", agentId);

                var kpis = await _dashboardService.GetAgentKpisAsync(agentId, ct);
                
                _logger.LogInformation("KPIs récupérés avec succès pour l'agent {AgentId}", agentId);
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs", ex);
            }
        }

        /// <summary>
        /// Récupère les données pour les graphiques personnels de l'agent
        /// </summary>
        [HttpGet("graphs")]
        public async Task<ActionResult<AgentGraphsDto>> GetGraphs(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                var graphs = await _dashboardService.GetAgentGraphsAsync(agentId, ct);
                return Ok(graphs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des graphiques", ex);
            }
        }
        /// <summary>
        /// Récupère les performances personnelles de l'agent
        /// </summary>
        [HttpGet("performance")]
        public async Task<ActionResult<AgentPerformanceDto>> GetPerformance(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                _logger.LogInformation("Récupération de la performance pour l'agent {AgentId}", agentId);

                var performance = await _dashboardService.GetAgentPerformanceAsync(agentId, ct);
                
                _logger.LogInformation("Performance récupérée avec succès pour l'agent {AgentId}", agentId);
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la performance", ex);
            }
        }

        /// <summary>
        /// Récupère les affiliés récents de l'agent
        /// </summary>
        [HttpGet("affilies-recents")]
        public async Task<ActionResult<List<AgentAffilieRecentDto>>> GetAffiliesRecents([FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                
                // Par défaut, limit = 10
                if (limit == 0) limit = 10;
                
                _logger.LogInformation("Récupération des {Limit} affiliés récents pour l'agent {AgentId}", limit, agentId);

                var affilies = await _dashboardService.GetAffiliesRecentsAsync(agentId, limit, ct);
                
                _logger.LogInformation("Affiliés récents récupérés: {Count}", affilies.Count);
                return Ok(affilies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des affiliés récents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des affiliés récents", ex);
            }
        }

        /// <summary>
        /// Récupère les collectes en attente de l'agent
        /// </summary>
        [HttpGet("collectes-en-attente")]
        public async Task<ActionResult<List<AgentCollecteEnAttenteDto>>> GetCollectesEnAttente(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                _logger.LogInformation("Récupération des collectes en attente pour l'agent {AgentId}", agentId);

                var collectes = await _dashboardService.GetCollectesEnAttenteAsync(agentId, ct);
                
                _logger.LogInformation("Collectes en attente récupérées: {Count}", collectes.Count);
                return Ok(collectes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des collectes en attente");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des collectes en attente", ex);
            }
        }

        /// <summary>
        /// Récupère les commissions détaillées de l'agent
        /// </summary>
        [HttpGet("commissions")]
        public async Task<ActionResult<List<AgentCommissionDto>>> GetCommissions([FromQuery] int mois, [FromQuery] int annee, CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                
                // Par défaut, mois et année courants
                if (mois == 0) mois = DateTime.Now.Month;
                if (annee == 0) annee = DateTime.Now.Year;

                _logger.LogInformation("Récupération des commissions pour l'agent {AgentId}, mois {Mois}, année {Annee}", agentId, mois, annee);

                var commissions = await _dashboardService.GetCommissionsAsync(agentId, mois, annee, ct);
                
                _logger.LogInformation("Commissions récupérées: {Count}", commissions.Count);
                return Ok(commissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commissions");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des commissions", ex);
            }
        }

        /// <summary>
        /// Récupère les objectifs de l'agent
        /// </summary>
        [HttpGet("objectifs")]
        public async Task<ActionResult<AgentObjectifDto>> GetObjectifs([FromQuery] int mois, [FromQuery] int annee, CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                
                // Par défaut, mois et année courants
                if (mois == 0) mois = DateTime.Now.Month;
                if (annee == 0) annee = DateTime.Now.Year;

                _logger.LogInformation("Récupération des objectifs pour l'agent {AgentId}, mois {Mois}, année {Annee}", agentId, mois, annee);

                var objectifs = await _dashboardService.GetObjectifsAsync(agentId, mois, annee, ct);
                
                _logger.LogInformation("Objectifs récupérés pour l'agent {AgentId}", agentId);
                return Ok(objectifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des objectifs", ex);
            }
        }

        /// <summary>
        /// Récupère les statistiques des prestations de l'agent
        /// </summary>
        [HttpGet("prestations-stats")]
        public async Task<ActionResult<List<PrestationStatsDto>>> GetPrestationsStats(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                _logger.LogInformation("Récupération des statistiques des prestations pour l'agent {AgentId}", agentId);

                var stats = await _dashboardService.GetPrestationsStatsAsync(agentId, ct);
                
                _logger.LogInformation("Statistiques des prestations récupérées: {Count}", stats.Count);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des prestations");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des statistiques des prestations", ex);
            }
        }

        /// <summary>
        /// Récupère l'activité quotidienne de l'agent
        /// </summary>
        [HttpGet("activite-quotidienne")]
        public async Task<ActionResult<List<DailyActivityDto>>> GetActiviteQuotidienne([FromQuery] int jours, CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                
                // Par défaut, jours = 30
                if (jours == 0) jours = 30;
                
                _logger.LogInformation("Récupération de l'activité quotidienne sur {Jours} jours pour l'agent {AgentId}", jours, agentId);

                var activite = await _dashboardService.GetActiviteQuotidienneAsync(agentId, jours, ct);
                
                _logger.LogInformation("Activité quotidienne récupérée: {Count} jours", activite.Count);
                return Ok(activite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'activité quotidienne");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'activité quotidienne", ex);
            }
        }

        /// <summary>
        /// Récupère les performances mensuelles de l'agent
        /// </summary>
        [HttpGet("performances-mensuelles")]
        public async Task<ActionResult<List<MonthlyCollectionDto>>> GetPerformancesMensuelles([FromQuery] int mois, CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                
                // Par défaut, mois = 12
                if (mois == 0) mois = 12;
                
                _logger.LogInformation("Récupération des performances sur {Mois} mois pour l'agent {AgentId}", mois, agentId);

                var performances = await _dashboardService.GetPerformancesMensuellesAsync(agentId, mois, ct);
                
                _logger.LogInformation("Performances mensuelles récupérées: {Count}", performances.Count);
                return Ok(performances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances mensuelles");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des performances mensuelles", ex);
            }
        }

        /// <summary>
        /// Récupère le top des affiliés de l'agent
        /// </summary>
        [HttpGet("top-affilies")]
        public async Task<ActionResult<List<AgentAffilieRecentDto>>> GetTopAffilies([FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                
                // Par défaut, limit = 5
                if (limit == 0) limit = 5;
                
                _logger.LogInformation("Récupération du top {Limit} des affiliés pour l'agent {AgentId}", limit, agentId);

                var topAffilies = await _dashboardService.GetTopAffiliesAsync(agentId, limit, ct);
                
                _logger.LogInformation("Top affiliés récupérés: {Count}", topAffilies.Count);
                return Ok(topAffilies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du top des affiliés");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du top des affiliés", ex);
            }
        }

        /// <summary>
        /// Récupère un résumé complet du dashboard de l'agent
        /// </summary>
        [HttpGet("resume")]
        public async Task<ActionResult<AgentTerrainDashboardDto>> GetResume(CancellationToken ct)
        {
            try
            {
                var agentId = await GetCurrentAgentIdAsync(ct);
                return Ok(await _dashboardService.GetDashboardTerrainAsync(agentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé du dashboard");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du résumé du dashboard", ex);
            }
        }

        private Task<int> GetCurrentAgentIdAsync(CancellationToken ct) =>
            CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
    }
}
