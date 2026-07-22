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
    public class DependantController : BaseApiController
    {
        private readonly IDependantRepository _repo;
        private readonly ProsocDbContext _db;
        private readonly ITypeAdhesionDependantsValidationService _typeAdhesionDependantsValidation;

        public DependantController(
            IDependantRepository repo,
            ProsocDbContext db,
            ITypeAdhesionDependantsValidationService typeAdhesionDependantsValidation,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<DependantController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _db = db;
            _typeAdhesionDependantsValidation = typeAdhesionDependantsValidation;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<DependantReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des dépendants");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_DEPENDANT"))
                    return ForbiddenPermission("READ_DEPENDANT");

                var query = _db.Dependants
                    .Include(d => d.Affilie)
                    .Include(d => d.Antecedants)
                        .ThenInclude(a => a.Affilie)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(DependantDtoMapper.ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<DependantReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des dépendants paginés",
                    ex);
            }
        }

        /// <summary>Dépendants rattachés à l'affilié connecté.</summary>
        [HttpGet("mes-dependants")]
        public async Task<ActionResult<PaginatedResponse<DependantReadDto>>> GetMesDependants(
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var (affilieId, error) = await AffilieMemberScopeHelper.RequireOwnAffilieIdAsync(User, _db, ct);
                if (error != null)
                    return error;

                var query = _db.Dependants
                    .Include(d => d.Affilie)
                    .Include(d => d.Antecedants)
                        .ThenInclude(a => a.Affilie)
                    .Where(d => d.AffilieId == affilieId)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
                var dtos = result.Data.Select(DependantDtoMapper.ToReadDto).ToList();

                return Ok(new PaginatedResponse<DependantReadDto>
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
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération de vos dépendants",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DependantReadDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnDependantScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_DEPENDANT"))
                return ForbiddenPermission("READ_DEPENDANT");

            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(DependantDtoMapper.ToReadDto(item));
        }

        /// <summary>
        /// Récupère les antécédents d'un dépendant (paginé)
        /// </summary>
        [HttpGet("{id:int}/antecedants")]
        public async Task<ActionResult<PaginatedResponse<AntecedentReadDto>>> GetAntecedantsByDependant(
            int id,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnDependantScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_ANTECEDENT"))
                    return ForbiddenPermission("READ_ANTECEDENT");

                var dependantExists = await _db.Dependants
                    .AsNoTracking()
                    .AnyAsync(d => d.IdDependant == id, ct);
                if (!dependantExists)
                    return NotFound("Dépendant non trouvé");

                var query = _db.Antecedants
                    .AsNoTracking()
                    .Include(a => a.Affilie)
                    .Include(a => a.Dependant)
                    .Where(a => a.DependantId == id)
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
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des antécédents du dépendant",
                    ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<DependantReadDto>> Create([FromBody] DependantCreateDto input, CancellationToken ct)
        {
            if (AffilieMemberScopeHelper.IsMembreAffilie(User))
            {
                var (ownAffilieId, scopeError) = await AffilieMemberScopeHelper.RequireOwnAffilieIdAsync(User, _db, ct);
                if (scopeError != null)
                    return scopeError;

                if (input.AffilieId is null or <= 0 || input.AffilieId != ownAffilieId)
                    input.AffilieId = ownAffilieId;
            }
            else if (!HasPermission("CREATE_DEPENDANT"))
            {
                return ForbiddenPermission("CREATE_DEPENDANT");
            }

            if (input.AffilieId is null or <= 0)
                return BadRequest("affilieId est obligatoire.");

            var affilie = await _db.Affilies.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAffilie == input.AffilieId, ct);
            if (affilie == null)
                return BadRequest("Affilié introuvable.");

            var errors = PersonneEnChargeRegles.ValiderDependant(
                DependantValidationInput.FromCreate(input),
                affilie.DateNaissance);
            if (errors.Any())
                return BadRequest(new { message = string.Join(" ", errors) });

            var plafondError = await EnsureCanAddDependantAsync(input.AffilieId.Value, excludeDependantId: null, ct);
            if (plafondError != null)
                return plafondError;

            var entity = new Dependant
            {
                Nom = input.Nom.Trim(),
                Adresse = input.Adresse,
                LienParente = LienParenteRegles.Normaliser(input.LienParente),
                DateNaissance = input.DateNaissance,
                AffilieId = input.AffilieId.Value
            };
            DependantCertificatApplicator.Appliquer(
                entity, input.CertificatScolariteBase64, input.CertificatScolariteContentType);

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdDependant }, DependantDtoMapper.ToReadDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<DependantReadDto>> Update([FromRoute] int id, [FromBody] DependantUpdateDto input, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnDependantScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("UPDATE_DEPENDANT"))
                return ForbiddenPermission("UPDATE_DEPENDANT");

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing == null)
                return NotFound();

            if (AffilieMemberScopeHelper.IsMembreAffilie(User)
                && input.AffilieId.HasValue
                && input.AffilieId.Value != existing.AffilieId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Accès refusé : vous ne pouvez modifier que vos propres dépendants."
                });
            }

            var affilie = await _db.Affilies.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAffilie == (input.AffilieId ?? existing.AffilieId), ct);
            if (affilie == null)
                return BadRequest("Affilié introuvable.");

            var validation = DependantValidationInput.FromCreate(new DependantCreateDto
            {
                Nom = input.Nom,
                LienParente = input.LienParente,
                DateNaissance = input.DateNaissance ?? existing.DateNaissance,
                CertificatScolariteBase64 = input.CertificatScolariteBase64,
                CertificatScolariteContentType = input.CertificatScolariteContentType
            });
            var errors = PersonneEnChargeRegles.ValiderDependant(validation, affilie.DateNaissance);
            if (errors.Any())
                return BadRequest(new { message = string.Join(" ", errors) });

            var targetAffilieId = input.AffilieId ?? existing.AffilieId;
            if (targetAffilieId != existing.AffilieId)
            {
                var plafondError = await EnsureCanAddDependantAsync(targetAffilieId, excludeDependantId: null, ct);
                if (plafondError != null)
                    return plafondError;
            }

            var entity = new Dependant
            {
                Nom = input.Nom.Trim(),
                Adresse = input.Adresse,
                LienParente = LienParenteRegles.Normaliser(input.LienParente),
                DateNaissance = input.DateNaissance ?? existing.DateNaissance,
                AffilieId = targetAffilieId
            };
            DependantCertificatApplicator.Appliquer(
                entity, input.CertificatScolariteBase64, input.CertificatScolariteContentType);

            var updated = await _repo.UpdateAsync(id, entity, ct);
            return updated == null ? NotFound() : Ok(DependantDtoMapper.ToReadDto(updated));
        }

        /// <summary>
        /// Vérifie que l'ajout d'un dépendant ne dépasse pas TypeAdhesion.MaxDependants.
        /// </summary>
        private async Task<ActionResult?> EnsureCanAddDependantAsync(
            int affilieId,
            int? excludeDependantId,
            CancellationToken ct)
        {
            var typeAdhesionId = await _db.Adhesions
                .AsNoTracking()
                .Where(a => a.AffilieId == affilieId)
                .Select(a => (int?)a.TypeAdhesionId)
                .FirstOrDefaultAsync(ct);

            if (!typeAdhesionId.HasValue)
                return BadRequest(new { message = "Affilié sans adhésion — type d'adhésion introuvable." });

            var countQuery = _db.Dependants.AsNoTracking().Where(d => d.AffilieId == affilieId);
            if (excludeDependantId.HasValue)
                countQuery = countQuery.Where(d => d.IdDependant != excludeDependantId.Value);

            var countExistant = await countQuery.CountAsync(ct);

            try
            {
                await _typeAdhesionDependantsValidation.ValidateDependantsCountAsync(
                    typeAdhesionId.Value,
                    countExistant + 1,
                    ct);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return null;
        }

        [HttpGet("{id:int}/certificat-scolarite")]
        public async Task<IActionResult> GetCertificatScolarite(int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnDependantScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_DEPENDANT"))
                return ForbiddenPermission("READ_DEPENDANT");

            var dependant = await _db.Dependants.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDependant == id, ct);

            if (dependant == null || !AffilieFichierHelper.ADesDonnees(dependant.CertificatScolariteData))
                return NotFound();

            return File(
                dependant.CertificatScolariteData!,
                dependant.CertificatScolariteContentType ?? "application/octet-stream");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnDependantScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!HasPermission("DELETE_DEPENDANT"))
                return ForbiddenPermission("DELETE_DEPENDANT");

            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<ExtendedPaginatedResponse<DependantReadDto>>> GetDependantsAdvanced(
            [FromBody] AdvancedPaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des dépendants");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_DEPENDANT"))
                    return ForbiddenPermission("READ_DEPENDANT");

                var query = _db.Dependants
                    .Include(d => d.Affilie)
                    .Include(d => d.Antecedants)
                        .ThenInclude(a => a.Affilie)
                    .AsQueryable();

                if (request.FilterList != null && request.FilterList.Any())
                {
                    foreach (var filter in request.FilterList)
                    {
                        switch (filter.Field.ToLower())
                        {
                            case "affilieid":
                                if (filter.Operator == "eq")
                                    query = query.Where(d => d.AffilieId == int.Parse(filter.Value));
                                break;
                            case "nom":
                                if (filter.Operator == "contains")
                                    query = query.Where(d => d.Nom.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(d => d.Nom == filter.Value);
                                break;
                            case "lienparente":
                                if (filter.Operator == "contains")
                                    query = query.Where(d => d.LienParente.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(d => d.LienParente == filter.Value);
                                break;
                            case "affilienom":
                                if (filter.Operator == "contains")
                                    query = query.Where(d => d.Affilie != null &&
                                        (d.Affilie.Nom.Contains(filter.Value) || d.Affilie.Prenom.Contains(filter.Value)));
                                break;
                        }
                    }
                }

                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);
                var dependantDtos = response.Data.Select(DependantDtoMapper.ToReadDto).ToList();

                return Ok(new ExtendedPaginatedResponse<DependantReadDto>
                {
                    Data = dependantDtos,
                    CurrentPage = response.CurrentPage,
                    PageSize = response.PageSize,
                    TotalItems = response.TotalItems,
                    TotalPages = response.TotalPages,
                    HasNextPage = response.HasNextPage,
                    HasPreviousPage = response.HasPreviousPage,
                    AppliedFilters = request.FilterList?.Select(f => $"{f.Field} {f.Operator} {f.Value}").ToList() ?? new(),
                    AppliedSorting = $"{request.SortBy} {request.SortDirection}"
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des dépendants avancés",
                    ex);
            }
        }

        [HttpGet("by-affilie/{affilieId}")]
        public async Task<ActionResult<PaginatedResponse<DependantReadDto>>> GetByAffilie(
            int affilieId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_DEPENDANT"))
                    return ForbiddenPermission("READ_DEPENDANT");

                var query = DependantQueryHelper.GetByAffilieQuery(_db, affilieId);

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
                var dtos = result.Data.Select(DependantDtoMapper.ToReadDto).ToList();

                return Ok(new PaginatedResponse<DependantReadDto>
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
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des dépendants pour l'affilié ",
                    ex);
            }
        }
    }
}
