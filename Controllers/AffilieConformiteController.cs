using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AffilieConformiteController : BaseApiController
    {
        private readonly IAffilieConformiteService _conformiteService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<AffilieConformiteController> _logger;

        public AffilieConformiteController(
            IAffilieConformiteService conformiteService,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<AffilieConformiteController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _conformiteService = conformiteService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("{affilieId:int}")]
        public async Task<ActionResult<AffilieConformiteDto>> GetByAffilie(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureAccessAffilieAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var conformite = await _conformiteService.GetConformiteAffilieAsync(affilieId, ct);
                if (conformite == null)
                    return NotFound(new { error = "Affilié introuvable" });

                return Ok(conformite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur conformité affilié {AffilieId}", affilieId);
                return StatusCode(500, new { error = "Erreur lors du calcul de la conformité" });
            }
        }

        [HttpGet("mes-conformite")]
        [Authorize(Roles = "Affilié")]
        public async Task<ActionResult<AffilieConformiteDto>> GetMesConformite(CancellationToken ct = default)
        {
            try
            {
                var affilieId = await CurrentUserAffilieResolver.ResolveAffilieIdAsync(User, _db, ct);
                if (affilieId <= 0)
                    return Unauthorized(new { error = "Utilisateur non rattaché à un affilié" });

                var conformite = await _conformiteService.GetConformiteAffilieAsync(affilieId, ct);
                if (conformite == null)
                    return NotFound(new { error = "Affilié introuvable" });

                return Ok(conformite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mes-conformite");
                return StatusCode(500, new { error = "Erreur lors du calcul de la conformité" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Financier,SuperAdmin,Agent (AT),Chef d'équipe")]
        public async Task<ActionResult<PaginatedResponse<AffilieConformiteDto>>> GetListe(
            [FromQuery] AffilieConformiteFiltreDto filtres,
            [FromQuery] PaginationRequest? pagination,
            CancellationToken ct = default)
        {
            try
            {
                filtres ??= new AffilieConformiteFiltreDto();

                if (IsAgentRole() && !IsStaffGlobal())
                {
                    var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                    filtres.AgentId = agentId;
                }

                pagination = ValidatePaginationRequest(pagination ?? new PaginationRequest());
                var result = await _conformiteService.GetConformiteListeAsync(filtres, pagination, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste conformité affiliés");
                return StatusCode(500, new { error = "Erreur lors de la récupération de la liste de conformité" });
            }
        }

        private async Task<ActionResult?> EnsureAccessAffilieAsync(int affilieId, CancellationToken ct)
        {
            var membreScope = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, affilieId, ct);
            if (membreScope != null)
                return membreScope;

            if (IsStaffGlobal())
                return null;

            if (IsAgentRole())
            {
                var agentId = await CurrentUserAgentResolver.RequireAgentIdAsync(User, _db, ct);
                var hasAccess = await _db.Adhesions.AnyAsync(
                    a => a.AffilieId == affilieId && a.AgentId == agentId && a.Statut, ct);
                if (!hasAccess)
                    return Forbid();
                return null;
            }

            if (User.IsInRole("Financier") || User.IsInRole("Percepteur"))
                return null;

            return Forbid();
        }

        private bool IsStaffGlobal() =>
            User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("IT");

        private bool IsAgentRole() =>
            User.IsInRole("Agent (AT)") || User.IsInRole("Chef d'équipe");
    }
}
