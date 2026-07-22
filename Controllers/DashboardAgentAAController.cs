using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.DashboardAgentAa;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = CurrentUserAgentResolver.AgentAaRoleName)]
    public class DashboardAgentAAController : ControllerBase
    {
        private readonly IDashboardAgentAARepository _dashboardService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardAgentAAController> _logger;

        public DashboardAgentAAController(
            IDashboardAgentAARepository dashboardService,
            ProsocDbContext db,
            ILogger<DashboardAgentAAController> logger)
        {
            _dashboardService = dashboardService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardAgentAaDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDashboardSummaryAsync(agentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dashboard agent AA (summary)");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard encodeur", ex);
            }
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<AgentAaKpisDto>> GetKpis(CancellationToken ct)
        {
            try
            {
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetKpisAsync(agentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs agent AA");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs encodeur", ex);
            }
        }

        [HttpGet("dossiers-a-traiter")]
        public async Task<ActionResult<List<AgentAaDossierDto>>> GetDossiersATraiter(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDossiersATraiterAsync(agentId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dossiers à traiter agent AA");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des dossiers à traiter", ex);
            }
        }

        [HttpGet("dependants-recents")]
        public async Task<ActionResult<List<AgentAaDependantRecentDto>>> GetDependantsRecents(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDependantsRecentsAsync(agentId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dépendants récents agent AA");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des dépendants récents", ex);
            }
        }

        [HttpGet("antecedents-recents")]
        public async Task<ActionResult<List<AgentAaAntecedentRecentDto>>> GetAntecedentsRecents(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetAntecedentsRecentsAsync(agentId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur antécédents récents agent AA");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des antécédents récents", ex);
            }
        }

        [HttpGet("repartition-statuts")]
        public async Task<ActionResult<List<AgentAaRepartitionStatutDto>>> GetRepartitionStatuts(CancellationToken ct)
        {
            try
            {
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetRepartitionStatutsAsync(agentId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur répartition statuts agent AA");
                return this.TechnicalErrorResponse("Erreur lors de la répartition par statut de dossier", ex);
            }
        }
    }
}
