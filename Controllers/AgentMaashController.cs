using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    /// <summary>Retenue MAASH (5 USD/mois) et couverture agent + famille.</summary>
    [ApiController]
    [Route("api/agent-maash")]
    [Authorize]
    public class AgentMaashController : ControllerBase
    {
        private readonly IAgentMaashRetenueService _maashService;
        private readonly ILogger<AgentMaashController> _logger;

        public AgentMaashController(
            IAgentMaashRetenueService maashService,
            ILogger<AgentMaashController> logger)
        {
            _maashService = maashService;
            _logger = logger;
        }

        /// <summary>Déclenche manuellement la retenue automatique pour tous les agents éligibles (admin).</summary>
        [HttpPost("executer-retenue-automatique")]
        [Authorize(Roles = "Admin,SuperAdmin,IT,Financier")]
        public async Task<ActionResult<AgentMaashBatchResultDto>> ExecuterRetenueAutomatique(
            CancellationToken ct = default)
        {
            try
            {
                var result = await _maashService.ExecuterRetenueAutomatiqueAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur exécution batch retenue MAASH");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        /// <summary>Statut de couverture MAASH pour la période courante.</summary>
        [HttpGet("{agentId}/couverture")]
        public async Task<ActionResult<AgentMaashCouvertureReadDto>> GetCouverture(
            int agentId,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _maashService.GetCouvertureAsync(agentId, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur couverture MAASH agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        /// <summary>Appliquer la retenue mensuelle (débit wallet) et enregistrer les bénéficiaires famille.</summary>
        [HttpPost("{agentId}/retenue")]
        public async Task<ActionResult<AgentMaashRetenueReadDto>> AppliquerRetenue(
            int agentId,
            [FromBody] AgentMaashRetenueRequestDto? request,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _maashService.AppliquerRetenueMensuelleAsync(agentId, request, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur retenue MAASH agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }
    }
}
