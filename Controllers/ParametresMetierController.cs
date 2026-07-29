using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [Route("api/parametres-metier")]
    [ApiController]
    [Authorize]
    public class ParametresMetierController : BaseApiController
    {
        private readonly IParametresMetierProvider _parametresMetierProvider;
        private readonly ILogger<ParametresMetierController> _logger;

        public ParametresMetierController(
            IParametresMetierProvider parametresMetierProvider,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<ParametresMetierController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _parametresMetierProvider = parametresMetierProvider;
            _logger = logger;
        }

        [HttpGet("retrait-agent")]
        public async Task<ActionResult<RetraitAgentParametresReadDto>> GetRetraitAgent(CancellationToken ct = default)
        {
            if (!HasPermission("READ_PARAMETRES_METIER"))
                return ForbiddenPermission("READ_PARAMETRES_METIER");

            try
            {
                return Ok(await _parametresMetierProvider.GetRetraitAgentReadAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture paramètres RetraitAgent");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPut("retrait-agent")]
        public async Task<ActionResult<RetraitAgentParametresReadDto>> UpdateRetraitAgent(
            [FromBody] RetraitAgentParametresUpdateDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission("UPDATE_PARAMETRES_METIER"))
                return ForbiddenPermission("UPDATE_PARAMETRES_METIER");

            try
            {
                var result = await _parametresMetierProvider.UpdateRetraitAgentAsync(
                    dto,
                    ResolveCurrentUserId(),
                    ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour paramètres RetraitAgent");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("agent-maash")]
        public async Task<ActionResult<AgentMaashParametresReadDto>> GetAgentMaash(CancellationToken ct = default)
        {
            if (!HasPermission("READ_PARAMETRES_METIER"))
                return ForbiddenPermission("READ_PARAMETRES_METIER");

            try
            {
                return Ok(await _parametresMetierProvider.GetAgentMaashReadAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture paramètres AgentMaash");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPut("agent-maash")]
        public async Task<ActionResult<AgentMaashParametresReadDto>> UpdateAgentMaash(
            [FromBody] AgentMaashParametresUpdateDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission("UPDATE_PARAMETRES_METIER"))
                return ForbiddenPermission("UPDATE_PARAMETRES_METIER");

            try
            {
                var result = await _parametresMetierProvider.UpdateAgentMaashAsync(
                    dto,
                    ResolveCurrentUserId(),
                    ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour paramètres AgentMaash");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("arrieres")]
        public async Task<ActionResult<ArrieresParametresReadDto>> GetArrieres(CancellationToken ct = default)
        {
            if (!HasPermission("READ_PARAMETRES_METIER"))
                return ForbiddenPermission("READ_PARAMETRES_METIER");

            try
            {
                return Ok(await _parametresMetierProvider.GetArrieresReadAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture paramètres Arrieres");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPut("arrieres")]
        public async Task<ActionResult<ArrieresParametresReadDto>> UpdateArrieres(
            [FromBody] ArrieresParametresUpdateDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission("UPDATE_PARAMETRES_METIER"))
                return ForbiddenPermission("UPDATE_PARAMETRES_METIER");

            try
            {
                var result = await _parametresMetierProvider.UpdateArrieresAsync(
                    dto,
                    ResolveCurrentUserId(),
                    ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour paramètres Arrieres");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("penalite")]
        public async Task<ActionResult<PenaliteParametresReadDto>> GetPenalite(CancellationToken ct = default)
        {
            if (!HasPermission("READ_PARAMETRES_METIER"))
                return ForbiddenPermission("READ_PARAMETRES_METIER");

            try
            {
                return Ok(await _parametresMetierProvider.GetPenaliteReadAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture paramètres Penalite");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPut("penalite")]
        public async Task<ActionResult<PenaliteParametresReadDto>> UpdatePenalite(
            [FromBody] PenaliteParametresUpdateDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission("UPDATE_PARAMETRES_METIER"))
                return ForbiddenPermission("UPDATE_PARAMETRES_METIER");

            try
            {
                var result = await _parametresMetierProvider.UpdatePenaliteAsync(
                    dto,
                    ResolveCurrentUserId(),
                    ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour paramètres Penalite");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("plafond-wallet-virtuel")]
        public async Task<ActionResult<WalletVirtuelParametresReadDto>> GetPlafondWalletVirtuel(
            CancellationToken ct = default)
        {
            if (!HasPermission("READ_PARAMETRES_METIER"))
                return ForbiddenPermission("READ_PARAMETRES_METIER");

            try
            {
                return Ok(await _parametresMetierProvider.GetWalletVirtuelReadAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture paramètres WalletVirtuel");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPut("plafond-wallet-virtuel")]
        public async Task<ActionResult<WalletVirtuelParametresReadDto>> UpdatePlafondWalletVirtuel(
            [FromBody] WalletVirtuelParametresUpdateDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission("UPDATE_PARAMETRES_METIER"))
                return ForbiddenPermission("UPDATE_PARAMETRES_METIER");

            try
            {
                var result = await _parametresMetierProvider.UpdateWalletVirtuelAsync(
                    dto,
                    ResolveCurrentUserId(),
                    ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour paramètres WalletVirtuel");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        private int ResolveCurrentUserId()
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
                return userId;

            if (int.TryParse(User.FindFirst("uid")?.Value, out var uid))
                return uid;

            return 0;
        }
    }
}
