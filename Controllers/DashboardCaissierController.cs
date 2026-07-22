using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Caissier")]
    public class DashboardCaissierController : ControllerBase
    {
        private readonly IDashboardCaissierRepository _dashboardService;
        private readonly ILogger<DashboardCaissierController> _logger;

        public DashboardCaissierController(
            IDashboardCaissierRepository dashboardService,
            ILogger<DashboardCaissierController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardCaissierDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                return Ok(await _dashboardService.GetDashboardSummaryAsync(utilisateurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dashboard caissier (summary)");
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard caissier", ex);
            }
        }

        [HttpGet("kpis")]
        public async Task<ActionResult<CaissierKpisDto>> GetKpis(CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                return Ok(await _dashboardService.GetKpisAsync(utilisateurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur KPIs caissier");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs caissier", ex);
            }
        }

        [HttpGet("collectes-recentes")]
        public async Task<ActionResult<List<CaissierCollecteDto>>> GetCollectesRecentes(
            [FromQuery] int limit, CancellationToken ct)
        {
            try
            {
                if (limit == 0) limit = 50;
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                return Ok(await _dashboardService.GetCollectesRecentesAsync(utilisateurId, limit, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur collectes récentes caissier");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des collectes récentes", ex);
            }
        }

        /// <summary>
        /// Historique paginé des collectes guichet saisies par le caissier connecté.
        /// </summary>
        [HttpGet("collectes")]
        public async Task<ActionResult<PaginatedResponse<CaissierCollecteDto>>> GetCollectesHistorique(
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? modePaiement,
            [FromQuery] PaginationRequest pagination,
            CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var filtres = new GuichetCollecteHistoriqueFiltreDto
                {
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    ModePaiement = modePaiement
                };
                return Ok(await _dashboardService.GetCollectesHistoriqueAsync(utilisateurId, filtres, pagination, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur historique collectes caissier");
                return this.TechnicalErrorResponse("Erreur lors de la récupération de l'historique des collectes", ex);
            }
        }

        [HttpGet("repartition-type")]
        public async Task<ActionResult<List<CaissierRepartitionDto>>> GetRepartitionParType(CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                return Ok(await _dashboardService.GetRepartitionParTypeAsync(utilisateurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur répartition par type caissier");
                return this.TechnicalErrorResponse("Erreur lors de la répartition par type", ex);
            }
        }

        [HttpGet("repartition-mode")]
        public async Task<ActionResult<List<CaissierRepartitionDto>>> GetRepartitionParMode(CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                return Ok(await _dashboardService.GetRepartitionParModeAsync(utilisateurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur répartition par mode caissier");
                return this.TechnicalErrorResponse("Erreur lors de la répartition par mode", ex);
            }
        }

        [HttpGet("adhesions-du-jour")]
        public async Task<ActionResult<List<CaissierAdhesionDuJourDto>>> GetAdhesionsDuJour(CancellationToken ct)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                return Ok(await _dashboardService.GetAdhesionsDuJourAsync(utilisateurId, ct));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur adhésions du jour caissier");
                return this.TechnicalErrorResponse("Erreur lors de la récupération des adhésions du jour", ex);
            }
        }
    }
}
