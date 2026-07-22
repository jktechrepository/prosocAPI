using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Percepteur,Financier")]
    public class PerceptionVirtuelleController : ControllerBase
    {
        private readonly IPerceptionVirtuelleService _perceptionService;
        private readonly IPerceptionVirtuelleExportService _exportService;
        private readonly ILogger<PerceptionVirtuelleController> _logger;

        public PerceptionVirtuelleController(
            IPerceptionVirtuelleService perceptionService,
            IPerceptionVirtuelleExportService exportService,
            ILogger<PerceptionVirtuelleController> logger)
        {
            _perceptionService = perceptionService;
            _exportService = exportService;
            _logger = logger;
        }

        [HttpGet("collectes-en-attente")]
        public async Task<ActionResult<PaginatedResponse<CollecteVirtuelleEnAttenteDto>>> GetCollectesEnAttente(
            [FromQuery] int? agentId,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] PaginationRequest pagination,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _perceptionService.GetCollectesEnAttenteAsync(
                    agentId, dateDebut, dateFin, pagination, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste collectes virtuelles en attente");
                return StatusCode(500, new { error = "Erreur lors de la récupération des collectes en attente" });
            }
        }

        [HttpGet("synthese-agents")]
        public async Task<ActionResult<List<PerceptionVirtuelleSyntheseAgentDto>>> GetSyntheseAgents(
            CancellationToken ct = default)
        {
            try
            {
                return Ok(await _perceptionService.GetSyntheseAgentsAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur synthèse agents perception virtuelle");
                return StatusCode(500, new { error = "Erreur lors de la synthèse par agent" });
            }
        }

        [HttpGet("historique")]
        public async Task<ActionResult<PaginatedResponse<PerceptionVirtuelleReadDto>>> GetHistorique(
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] PaginationRequest pagination,
            CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var filtres = new PerceptionVirtuelleHistoriqueFiltreDto
                {
                    PercepteurUtilisateurId = utilisateurId,
                    DateDebut = dateDebut,
                    DateFin = dateFin
                };
                var result = await _perceptionService.GetHistoriqueGlobalAsync(filtres, pagination, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur historique perceptions");
                return StatusCode(500, new { error = "Erreur lors de la récupération de l'historique" });
            }
        }

        [HttpGet("historique-global")]
        [Authorize(Roles = "Admin,Financier")]
        public async Task<ActionResult<PaginatedResponse<PerceptionVirtuelleReadDto>>> GetHistoriqueGlobal(
            [FromQuery] int? percepteurUtilisateurId,
            [FromQuery] int? agentId,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] PaginationRequest pagination,
            CancellationToken ct = default)
        {
            try
            {
                var filtres = new PerceptionVirtuelleHistoriqueFiltreDto
                {
                    PercepteurUtilisateurId = percepteurUtilisateurId,
                    AgentId = agentId,
                    DateDebut = dateDebut,
                    DateFin = dateFin
                };
                var result = await _perceptionService.GetHistoriqueGlobalAsync(filtres, pagination, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur historique global perceptions");
                return StatusCode(500, new { error = "Erreur lors de la récupération de l'historique global" });
            }
        }

        [HttpGet("reconciliation")]
        [Authorize(Roles = "Admin,Financier")]
        public async Task<ActionResult<PerceptionReconciliationDto>> GetReconciliation(
            [FromQuery] int? agentId,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _perceptionService.GetReconciliationAsync(agentId, dateDebut, dateFin, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur réconciliation perception virtuelle");
                return StatusCode(500, new { error = "Erreur lors de la réconciliation" });
            }
        }

        [HttpGet("export")]
        [Authorize(Roles = "Admin,Financier")]
        public async Task<IActionResult> ExportRapport(
            [FromQuery] string? origine,
            [FromQuery] string? statut,
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] int? agentId,
            [FromQuery] int? affilieId,
            [FromQuery] string format = "excel",
            CancellationToken ct = default)
        {
            try
            {
                if (!string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { error = "Seul le format excel est supporté." });

                var data = await _exportService.ExportRapportAsync(
                    dateDebut, dateFin, origine, statut, agentId, affilieId, ct);

                var fileName = $"perception_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(
                    data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur export perception virtuelle");
                return StatusCode(500, new { error = "Erreur lors de l'export" });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PerceptionVirtuelleReadDto>> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                var perception = await _perceptionService.GetByIdAsync(id, ct);
                if (perception == null)
                    return NotFound(new { error = "Perception introuvable" });
                return Ok(perception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur détail perception {Id}", id);
                return StatusCode(500, new { error = "Erreur lors de la récupération de la perception" });
            }
        }

        [HttpPost("confirmer")]
        public async Task<ActionResult<PerceptionVirtuelleConfirmerResultDto>> Confirmer(
            [FromBody] PerceptionVirtuelleConfirmerDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var result = await _perceptionService.ConfirmerPerceptionAsync(utilisateurId, dto, ct);

                if (result.Succes)
                    return Ok(result);

                if (result.CodeErreur == "COLLECTE_DEJA_PERCUE")
                    return Conflict(result);

                return BadRequest(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur confirmation perception virtuelle");
                return StatusCode(500, new { error = "Erreur lors de la confirmation de la perception" });
            }
        }
    }
}
