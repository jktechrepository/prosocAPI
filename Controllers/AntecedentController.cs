using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AntecedentController : BaseApiController
    {
        private readonly IAntecedentRepository _repo;
        private readonly ProsocDbContext _db;

        public AntecedentController(
            IAntecedentRepository repo,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<AntecedentController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<AntecedentReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des antécédents");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_ANTECEDENT"))
                    return ForbiddenPermission("READ_ANTECEDENT");

                var query = _db.Antecedants.AsQueryable();
                var result = await CreatePaginatedResponseAsync<Antecedant>(query, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des antécédents");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        /// <summary>Antécédents rattachés à l'affilié connecté.</summary>
        [HttpGet("mes-antecedents")]
        public async Task<ActionResult<PaginatedResponse<AntecedentReadDto>>> GetMesAntecedents(
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var (affilieId, error) = await AffilieMemberScopeHelper.RequireOwnAffilieIdAsync(User, _db, ct);
                if (error != null)
                    return error;

                var query = _db.Antecedants
                    .AsNoTracking()
                    .Include(a => a.Affilie)
                    .Include(a => a.Dependant)
                    .Where(a => a.AffilieId == affilieId)
                    .OrderByDescending(a => a.DateCreation);

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
                var dtos = result.Data.Select(a => a.ToReadDto()).ToList();

                return Ok(new PaginatedResponse<AntecedentReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des antécédents de l'affilié connecté");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AntecedentReadDto>> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAntecedentScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_ANTECEDENT"))
                    return ForbiddenPermission("READ_ANTECEDENT");

                var antecedent = await _repo.GetByIdAsync(id);
                if (antecedent == null)
                    return NotFound();

                return Ok(antecedent.ToReadDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'antécédent {Id}", id);
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<AntecedentReadDto>> Create([FromBody] AntecedentCreateDto dto, CancellationToken ct = default)
        {
            try
            {
                if (AffilieMemberScopeHelper.IsMembreAffilie(User))
                {
                    var (ownAffilieId, scopeError) = await AffilieMemberScopeHelper.RequireOwnAffilieIdAsync(User, _db, ct);
                    if (scopeError != null)
                        return scopeError;

                    if (dto.AffilieId <= 0 || dto.AffilieId != ownAffilieId)
                        dto.AffilieId = ownAffilieId;
                }
                else if (!HasPermission("CREATE_ANTECEDENT"))
                {
                    return ForbiddenPermission("CREATE_ANTECEDENT");
                }

                var dependantError = await AntecedentDependantValidationHelper.ValidateDependantForAffilieAsync(
                    _db, dto.AffilieId, dto.DependantId, ct);
                if (dependantError != null)
                    return dependantError;

                var antecedent = dto.ToEntity();
                var result = await _repo.CreateAsync(antecedent);
                return CreatedAtAction(nameof(GetById), new { id = result.IdAntecedant }, result.ToReadDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'antécédent");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AntecedentReadDto>> Update(int id, [FromBody] AntecedentUpdateDto dto, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAntecedentScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("UPDATE_ANTECEDENT"))
                    return ForbiddenPermission("UPDATE_ANTECEDENT");

                var existingAntecedent = await _repo.GetByIdAsync(id);
                if (existingAntecedent == null)
                    return NotFound();

                if (AffilieMemberScopeHelper.IsMembreAffilie(User) && dto.AffilieId != existingAntecedent.AffilieId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        message = "Accès refusé : vous ne pouvez modifier que vos propres antécédents."
                    });
                }

                var dependantError = await AntecedentDependantValidationHelper.ValidateDependantForAffilieAsync(
                    _db, dto.AffilieId, dto.DependantId, ct);
                if (dependantError != null)
                    return dependantError;

                var updatedAntecedent = dto.ToEntity();
                var result = await _repo.UpdateAsync(id, updatedAntecedent);
                return Ok(result.ToReadDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'antécédent {Id}", id);
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAntecedentScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!HasPermission("DELETE_ANTECEDENT"))
                    return ForbiddenPermission("DELETE_ANTECEDENT");

                var antecedent = await _repo.GetByIdAsync(id);
                if (antecedent == null)
                    return NotFound();

                await _repo.DeleteAsync(id);
                return Ok(new { Message = "Antécédent supprimé avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'antécédent {Id}", id);
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginatedResponse<AntecedentReadDto>>> GetAdvanced(
            [FromQuery] PaginationRequest request,
            [FromBody] AntecedentSearchDto searchDto)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des antécédents");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_ANTECEDENT"))
                    return ForbiddenPermission("READ_ANTECEDENT");

                var query = _db.Antecedants.AsQueryable();

                if (!string.IsNullOrEmpty(searchDto.Description))
                    query = query.Where(a => a.Description.Contains(searchDto.Description));

                if (searchDto.DateNaissanceDebut.HasValue)
                    query = query.Where(a => a.DateCreation >= searchDto.DateNaissanceDebut.Value);

                if (searchDto.DateNaissanceFin.HasValue)
                    query = query.Where(a => a.DateCreation <= searchDto.DateNaissanceFin.Value);

                var result = await CreatePaginatedResponseAsync<Antecedant>(query, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche avancée des antécédents");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }
    }
}
