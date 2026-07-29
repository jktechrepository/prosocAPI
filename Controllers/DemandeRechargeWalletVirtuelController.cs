using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DemandeRechargeWalletVirtuelController : BaseApiController
    {
        private const string CreatePermission = "CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL";
        private const string ReadPermission = "READ_DEMANDE_RECHARGE_WALLET_VIRTUEL";
        private const string ConfirmPermission = "CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL";

        private readonly IDemandeRechargeWalletVirtuelService _service;
        private readonly ILogger<DemandeRechargeWalletVirtuelController> _logger;

        public DemandeRechargeWalletVirtuelController(
            IDemandeRechargeWalletVirtuelService service,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<DemandeRechargeWalletVirtuelController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<DemandeRechargeWalletVirtuelReadDto>>> GetAll(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? statutDemande = null,
            CancellationToken ct = default)
        {
            if (!HasPermission(ReadPermission))
                return ForbiddenPermission(ReadPermission);

            try
            {
                return Ok(await _service.GetAllAsync(request, statutDemande, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste demandes recharge wallet virtuel");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("en-attente")]
        public async Task<ActionResult<List<DemandeRechargeWalletVirtuelReadDto>>> GetEnAttente(
            CancellationToken ct = default)
        {
            if (!HasPermission(ReadPermission))
                return ForbiddenPermission(ReadPermission);

            try
            {
                return Ok(await _service.GetEnAttenteAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur file d'attente recharge wallet virtuel");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DemandeRechargeWalletVirtuelReadDto>> GetById(
            int id,
            CancellationToken ct = default)
        {
            if (!HasPermission(ReadPermission))
                return ForbiddenPermission(ReadPermission);

            try
            {
                var demande = await _service.GetByIdAsync(id, ct);
                if (demande == null)
                    return NotFound(new { message = "Demande introuvable" });

                return Ok(demande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture demande recharge {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpGet("by-agent/{agentId:int}")]
        public async Task<ActionResult<List<DemandeRechargeWalletVirtuelReadDto>>> GetByAgent(
            int agentId,
            CancellationToken ct = default)
        {
            if (!HasPermission(ReadPermission))
                return ForbiddenPermission(ReadPermission);

            try
            {
                return Ok(await _service.GetByAgentAsync(agentId, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur demandes recharge agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<DemandeRechargeWalletVirtuelReadDto>> Creer(
            [FromBody] DemandeRechargeWalletVirtuelCreateDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission(CreatePermission))
                return ForbiddenPermission(CreatePermission);

            try
            {
                var utilisateurId = ResolveCurrentUserId();
                if (utilisateurId <= 0)
                    return Unauthorized(new { message = "Utilisateur non identifié." });

                var result = await _service.CreerAsync(User, utilisateurId, dto, ct);
                return MapOperationResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création demande recharge wallet virtuel");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPost("{id:int}/confirmer")]
        public async Task<ActionResult<DemandeRechargeWalletVirtuelReadDto>> Confirmer(
            int id,
            CancellationToken ct = default)
        {
            if (!HasPermission(ConfirmPermission))
                return ForbiddenPermission(ConfirmPermission);

            try
            {
                var utilisateurId = ResolveCurrentUserId();
                if (utilisateurId <= 0)
                    return Unauthorized(new { message = "Utilisateur non identifié." });

                var result = await _service.ConfirmerAsync(User, utilisateurId, id, ct);
                return MapOperationResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur confirmation demande recharge {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        [HttpPost("{id:int}/rejeter")]
        public async Task<ActionResult<DemandeRechargeWalletVirtuelReadDto>> Rejeter(
            int id,
            [FromBody] DemandeRechargeWalletVirtuelRejeterDto dto,
            CancellationToken ct = default)
        {
            if (!HasPermission(ConfirmPermission))
                return ForbiddenPermission(ConfirmPermission);

            try
            {
                var utilisateurId = ResolveCurrentUserId();
                if (utilisateurId <= 0)
                    return Unauthorized(new { message = "Utilisateur non identifié." });

                var result = await _service.RejeterAsync(utilisateurId, id, dto, ct);
                return MapOperationResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur rejet demande recharge {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        private ActionResult MapOperationResult(DemandeRechargeWalletVirtuelOperationResultDto result)
        {
            if (result.Success && result.Demande != null)
                return Ok(result.Demande);

            var body = new { codeErreur = result.CodeErreur, message = result.Message };
            if (result.Forbidden)
                return StatusCode(StatusCodes.Status403Forbidden, body);
            if (result.Conflict)
                return Conflict(body);

            return BadRequest(body);
        }

        private int ResolveCurrentUserId()
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
                return userId;

            var resolved = CurrentUserResolver.TryGetCurrentUtilisateurId(User);
            return resolved ?? 0;
        }
    }
}
