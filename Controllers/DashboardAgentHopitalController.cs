using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.DashboardAgentHopital;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = CurrentUserHopitalResolver.AgentHopitalRoleName)]
    public class DashboardAgentHopitalController : ControllerBase
    {
        private readonly IDashboardAgentHopitalRepository _dashboardService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardAgentHopitalController> _logger;

        public DashboardAgentHopitalController(
            IDashboardAgentHopitalRepository dashboardService,
            ProsocDbContext db,
            ILogger<DashboardAgentHopitalController> logger)
        {
            _dashboardService = dashboardService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardAgentHopitalDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDashboardSummaryAsync(hopitalId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dashboard agent hôpital (summary)");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard hôpital", ex);
            }
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<HopitalKpisDto>> GetKpis(CancellationToken ct)
        {
            try
            {
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetKpisAsync(hopitalId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs agent hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs hôpital", ex);
            }
        }

        [HttpGet("jetons-en-attente")]
        public async Task<ActionResult<List<HopitalJetonEnAttenteDto>>> GetJetonsEnAttente(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetJetonsEnAttenteAsync(hopitalId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur jetons en attente hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des jetons en attente", ex);
            }
        }

        [HttpGet("bons-recents")]
        public async Task<ActionResult<List<HopitalBonRecentDto>>> GetBonsRecents(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetBonsRecentsAsync(hopitalId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur bons récents hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des bons récents", ex);
            }
        }

        [HttpGet("patients")]
        public async Task<ActionResult<List<HopitalPatientDto>>> GetPatients(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetPatientsAsync(hopitalId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste patients hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des patients", ex);
            }
        }

        [HttpGet("dependants")]
        public async Task<ActionResult<List<HopitalDependantDto>>> GetDependants(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 100;
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDependantsAsync(hopitalId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste dépendants hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des dépendants", ex);
            }
        }

        [HttpGet("antecedents")]
        public async Task<ActionResult<List<HopitalAntecedentDto>>> GetAntecedents(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 100;
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetAntecedentsAsync(hopitalId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste antécédents hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des antécédents", ex);
            }
        }

        [HttpGet("repartition-prestations")]
        public async Task<ActionResult<List<HopitalRepartitionPrestationDto>>> GetRepartitionPrestations(
            CancellationToken ct)
        {
            try
            {
                var hopitalId = await CurrentUserHopitalResolver.RequireHopitalPartenaireIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetRepartitionPrestationsAsync(hopitalId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur répartition prestations hôpital");
                return this.TechnicalErrorResponse("Erreur lors de la répartition par prestation", ex);
            }
        }
    }
}
