using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.DashboardAssureur;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Assureur")]
    public class DashboardAssureurController : ControllerBase
    {
        private readonly IDashboardAssureurRepository _dashboardService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardAssureurController> _logger;

        public DashboardAssureurController(
            IDashboardAssureurRepository dashboardService,
            ProsocDbContext db,
            ILogger<DashboardAssureurController> logger)
        {
            _dashboardService = dashboardService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardAssureurDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                var assureurId = await CurrentUserAssureurResolver.RequireAssureurIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDashboardSummaryAsync(assureurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dashboard assureur (summary)");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard assureur", ex);
            }
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<AssureurKpisDto>> GetKpis(CancellationToken ct)
        {
            try
            {
                var assureurId = await CurrentUserAssureurResolver.RequireAssureurIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetKpisAsync(assureurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs assureur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs assureur", ex);
            }
        }

        [HttpGet("affilies")]
        public async Task<ActionResult<List<AssureurAffilieDto>>> GetAffilies(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var assureurId = await CurrentUserAssureurResolver.RequireAssureurIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetAffiliesAsync(assureurId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste affiliés assureur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des affiliés", ex);
            }
        }

        [HttpGet("dependants")]
        public async Task<ActionResult<List<AssureurDependantDto>>> GetDependants(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 100;
                var assureurId = await CurrentUserAssureurResolver.RequireAssureurIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetDependantsAsync(assureurId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste dépendants assureur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des dépendants", ex);
            }
        }

        [HttpGet("antecedents")]
        public async Task<ActionResult<List<AssureurAntecedentDto>>> GetAntecedents(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 100;
                var assureurId = await CurrentUserAssureurResolver.RequireAssureurIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetAntecedentsAsync(assureurId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste antécédents assureur");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des antécédents", ex);
            }
        }

        [HttpGet("repartition-produits")]
        public async Task<ActionResult<List<AssureurRepartitionProduitDto>>> GetRepartitionProduits(CancellationToken ct)
        {
            try
            {
                var assureurId = await CurrentUserAssureurResolver.RequireAssureurIdAsync(User, _db, ct);
                return Ok(await _dashboardService.GetRepartitionProduitsAsync(assureurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur répartition produits assureur");
                return this.TechnicalErrorResponse("Erreur lors de la répartition par produit", ex);
            }
        }
    }
}
