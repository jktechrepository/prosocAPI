using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.DTOs.Statistiques;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatistiquesController : BaseApiController
    {
        private readonly IStatistiquesService _service;
        private readonly ILogger<StatistiquesController> _logger;

        public StatistiquesController(
            IStatistiquesService service,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<StatistiquesController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("generales")]
        public async Task<ActionResult<StatistiquesGeneralesDto>> GetGenerales([FromQuery] StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            if (!HasPermission("READ_STATISTIQUES"))
                return ForbiddenPermission("READ_STATISTIQUES");

            try
            {
                var result = await _service.GetGeneralesAsync(filtres, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur statistiques generales");
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques générales" });
            }
        }

        [HttpGet("financieres")]
        public async Task<ActionResult<StatistiquesFinancieresDto>> GetFinancieres([FromQuery] StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            if (!HasPermission("READ_STATISTIQUES"))
                return ForbiddenPermission("READ_STATISTIQUES");

            try
            {
                var result = await _service.GetFinancieresAsync(filtres, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur statistiques financieres");
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques financières" });
            }
        }

        [HttpGet("operationnelles")]
        public async Task<ActionResult<StatistiquesOperationnellesDto>> GetOperationnelles([FromQuery] StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            if (!HasPermission("READ_STATISTIQUES"))
                return ForbiddenPermission("READ_STATISTIQUES");

            try
            {
                var result = await _service.GetOperationnellesAsync(filtres, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur statistiques operationnelles");
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques opérationnelles" });
            }
        }

        [HttpGet("performance")]
        public async Task<ActionResult<StatistiquesPerformanceDto>> GetPerformance([FromQuery] StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            if (!HasPermission("READ_STATISTIQUES"))
                return ForbiddenPermission("READ_STATISTIQUES");

            try
            {
                var result = await _service.GetPerformanceAsync(filtres, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur statistiques performance");
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques de performance" });
            }
        }

        [HttpGet("consolidees")]
        public async Task<ActionResult<StatistiquesConsolideesDto>> GetConsolidees([FromQuery] StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            if (!HasPermission("READ_STATISTIQUES"))
                return ForbiddenPermission("READ_STATISTIQUES");

            try
            {
                var result = await _service.GetConsolideesAsync(filtres, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur statistiques consolidees");
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques consolidées" });
            }
        }
    }
}
