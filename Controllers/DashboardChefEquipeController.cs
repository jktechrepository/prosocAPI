using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = ChefEquipeZoneScopeHelper.RoleName)]
    public class DashboardChefEquipeController : ControllerBase
    {
        private readonly IDashboardChefEquipeRepository _dashboardService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardChefEquipeController> _logger;

        public DashboardChefEquipeController(
            IDashboardChefEquipeRepository dashboardService,
            ProsocDbContext db,
            ILogger<DashboardChefEquipeController> logger)
        {
            _dashboardService = dashboardService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<ChefEquipeKpisDto>> GetKpis(CancellationToken ct)
        {
            try
            {
                var chefAgentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetKpisAsync(chefAgentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs dashboard chef d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs équipe", ex);
            }
        }

        [HttpGet("agents")]
        public async Task<ActionResult<List<ChefEquipeAgentResumeDto>>> GetAgents(CancellationToken ct)
        {
            try
            {
                var chefAgentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetAgentsZoneAsync(chefAgentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste agents zone chef d'équipe");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des agents de la zone", ex);
            }
        }

        [HttpGet("agents/{agentId:int}/mouvements-wallet")]
        public async Task<ActionResult<AgentCommissionsResumeDto>> GetMouvementsWallet(
            int agentId,
            [FromQuery] int limit,
            CancellationToken ct)
        {
            try
            {
                var chefAgentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                if (limit == 0) limit = 20;
                return Ok(await _dashboardService.GetMouvementsWalletAgentAsync(chefAgentId, agentId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mouvements wallet équipe agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des mouvements wallet", ex);
            }
        }

        [HttpGet("agents/{agentId:int}/collectes")]
        public async Task<ActionResult<List<ChefEquipeCollecteResumeDto>>> GetCollectes(
            int agentId,
            [FromQuery] int limit,
            CancellationToken ct)
        {
            try
            {
                var chefAgentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                if (limit == 0) limit = 50;
                return Ok(await _dashboardService.GetCollectesAgentAsync(chefAgentId, agentId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur collectes équipe agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des collectes", ex);
            }
        }
    }
}
