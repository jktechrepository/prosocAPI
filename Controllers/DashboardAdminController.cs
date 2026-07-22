using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.DTOs.DashboardAdmin;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardAdminController : ControllerBase
    {
        private readonly IDashboardAdminRepository _dashboardService;
        private readonly ILogger<DashboardAdminController> _logger;

        public DashboardAdminController(IDashboardAdminRepository dashboardService, ILogger<DashboardAdminController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère les KPIs principaux pour le dashboard admin
        /// </summary>
        [HttpGet("kpis")]
        public async Task<ActionResult<DashboardAdminKpisDto>> GetKpis(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs du dashboard admin");
                var kpis = await _dashboardService.GetKpisAsync(ct);
                
                _logger.LogInformation("KPIs récupérés avec succès: {TotalAffilies} affilies, {TotalAgents} agents, {TotalCollectesMois} collectes ce mois", 
                    kpis.TotalAffilies, kpis.TotalAgents, kpis.TotalCollectesMois);
                
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs du dashboard admin");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs", ex);
            }
        }

        /// <summary>
        /// Récupère les données pour les graphiques du dashboard
        /// </summary>
       /*
        [HttpGet("graphs")]
        public async Task<ActionResult<DashboardAdminGraphsDto>> GetGraphs([FromQuery] int mois = 12, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques du dashboard admin pour {Mois} mois", mois);
                var graphs = await _dashboardService.GetGraphsAsync(mois, ct);
                
                _logger.LogInformation("Graphiques récupérés avec succès: {NbCollectesMensuelles} collectes mensuelles, {NbTopAgents} agents top", 
                    graphs.CollectesMensuelles?.Count ?? 0, graphs.TopAgents?.Count ?? 0);
                
                return Ok(graphs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques du dashboard admin");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des graphiques", ex);
            }
        }
        */
        /// <summary>
        /// Récupère la performance des agents
        /// </summary>
        [HttpGet("agents-performance")]
        public async Task<ActionResult<List<PerformanceAgentsDto>>> GetAgentsPerformance([FromQuery] int limit = 10, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération de la performance des {Limit} meilleurs agents", limit);
                var performance = await _dashboardService.GetTopAgentsAsync(limit, ct);
                
                _logger.LogInformation("Performance des agents récupérée avec succès: {NbAgents} agents", performance?.Count ?? 0);
                
                return Ok(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance des agents");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de la performance des agents", ex);
            }
        }

        /// <summary>
        /// Valide une collecte en attente
        /// </summary>
        [HttpPost("validate-collecte/{id}")]
        public async Task<IActionResult> ValidateCollecte(int id, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Tentative de validation de la collecte {CollecteId}", id);
                var result = await _dashboardService.ValidateCollecteAsync(id, ct);
                
                if (!result)
                {
                    _logger.LogWarning("Échec de la validation de la collecte {CollecteId}", id);
                    return NotFound(new { error = "Collecte non trouvée" });
                }
                
                _logger.LogInformation("Collecte {CollecteId} validée avec succès", id);
                return Ok(new { message = "Collecte validée avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la collecte {CollecteId}", id);
                return this.TechnicalErrorResponse("Erreur lors de la validation de la collecte", ex);
            }
        }

        /// <summary>
        /// Active ou désactive un agent
        /// </summary>
        [HttpPost("toggle-agent/{id}")]
        public async Task<IActionResult> ToggleAgentStatus(int id, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Tentative de changement de statut pour l'agent {AgentId}", id);
                var result = await _dashboardService.ToggleAgentStatusAsync(id, ct);
                
                if (!result)
                {
                    _logger.LogWarning("Échec du changement de statut pour l'agent {AgentId}", id);
                    return NotFound(new { error = "Agent non trouvé" });
                }
                
                _logger.LogInformation("Statut de l'agent {AgentId} changé avec succès", id);
                return Ok(new { message = "Statut de l'agent changé avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de statut pour l'agent {AgentId}", id);
                return this.TechnicalErrorResponse("Erreur lors du changement de statut de l'agent", ex);
            }
        }

        /// <summary>
        /// Récupère les collectes en attente de validation
        /// </summary>
        [HttpGet("collectes-en-attente")]
        public async Task<ActionResult<List<CollecteEnAttenteDto>>> GetCollectesEnAttente(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des collectes en attente de validation");
                var collectes = await _dashboardService.GetCollectesEnAttenteAsync(ct);
                
                _logger.LogInformation("Collectes en attente récupérées: {NbCollectes} collectes", collectes?.Count ?? 0);
                
                return Ok(collectes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des collectes en attente");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des collectes en attente", ex);
            }
        }
    }
}
