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
    public class BonEnvoiController : BaseApiController
    {
        private readonly BonEnvoiService _bonEnvoiService;
        private readonly ProsocDbContext _db;

        public BonEnvoiController(
            BonEnvoiService bonEnvoiService,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<BonEnvoiController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _bonEnvoiService = bonEnvoiService;
            _db = db;
        }

        /// <summary>
        /// Lit un QR code scanné et retourne les informations du bon d'envoi.
        /// </summary>
        [HttpPost("scanner")]
        [Authorize(Roles = "Admin,SuperAdmin,IT,Agent Hôpital,Agent (AT),Agent (AA),Superviseur,Caissier")]
        public async Task<ActionResult<BonEnvoiScanResultDto>> Scanner(
            [FromBody] BonEnvoiScanRequestDto request,
            CancellationToken ct = default)
        {
            try
            {
                if (!HasPermission("SCAN_BON_ENVOI") && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                    return ForbiddenPermission("SCAN_BON_ENVOI");

                var result = await _bonEnvoiService.ScannerAsync(request, ct);

                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                    if (hopitalId <= 0)
                        return Forbid();

                    if (result.Valide && result.Bon?.IdBonEnvoi is int bonEnvoiId)
                    {
                        var autorise = await HopitalScopeHelper.IsBonLinkedToHopitalAsync(_db, bonEnvoiId, hopitalId, ct);
                        if (!autorise)
                            return Forbid();
                    }
                    else if (!result.Valide)
                    {
                        return BadRequest(result);
                    }
                }

                return result.Valide ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur lors du scan d'un bon d'envoi",
                    ex);
            }
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.BonsEnvoi
                    .Include(b => b.Affilie)
                    .Include(b => b.Prestation)
                    .Include(b => b.JetonMedical)
                    .AsQueryable();

                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct: default);
                    if (hopitalId <= 0)
                        return Forbid();

                    query = query.Where(b =>
                        _db.DemandesBonEnvoi.Any(d =>
                            d.BonEnvoiId == b.IdBonEnvoi
                            && d.JetonMedicalId != null
                            && _db.JetonsMedicaux.Any(j =>
                                j.IdJeton == d.JetonMedicalId
                                && j.HopitalPartenaireId == hopitalId)));
                }

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(BonEnvoiDtoMapper.ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BonEnvoiReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var bon = await _bonEnvoiService.GetByIdAsync(id, ct);
            if (bon == null)
                return NotFound();

            if (CurrentUserHopitalResolver.IsAgentHopital(User))
            {
                var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                if (hopitalId <= 0
                    || !await HopitalScopeHelper.IsBonLinkedToHopitalAsync(_db, id, hopitalId, ct))
                    return Forbid();
            }

            return Ok(BonEnvoiDtoMapper.ToReadDto(bon));
        }

        [HttpGet("by-numero/{numeroBon}")]
        public async Task<ActionResult<BonEnvoiReadDto>> GetByNumeroBon(string numeroBon, CancellationToken ct = default)
        {
            var bon = await _bonEnvoiService.GetByNumeroBonAsync(numeroBon, ct);
            if (bon == null)
                return NotFound();

            var dto = new BonEnvoiReadDto
            {
                IdBonEnvoi = bon.IdBonEnvoi,
                NumeroBon = bon.NumeroBon,
                AffilieId = bon.AffilieId,
                AffilieNom = $"{bon.Affilie?.Nom} {bon.Affilie?.Prenom}".Trim(),
                PrestationId = bon.PrestationId,
                PrestationNom = bon.Prestation?.NomPrestation,
                DateEmission = bon.DateEmission,
                DateUtilisation = bon.DateUtilisation,
                EstUtilise = bon.EstUtilise,
                Statut = bon.Statut,
                DateCreation = bon.DateCreation,
                DateModification = bon.DateModification
            };
            return Ok(dto);
        }

        [HttpGet("by-affilie/{affilieId}/simple")]
        public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetByAffilie(
            int affilieId, 
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.BonsEnvoi
                    .Include(b => b.Affilie)
                    .Include(b => b.Prestation)
                    .Where(b => b.AffilieId == affilieId)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(b => new BonEnvoiReadDto
                {
                    IdBonEnvoi = b.IdBonEnvoi,
                    NumeroBon = b.NumeroBon,
                    AffilieId = b.AffilieId,
                    AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                    PrestationId = b.PrestationId,
                    PrestationNom = b.Prestation?.NomPrestation,
                    DateEmission = b.DateEmission,
                    DateUtilisation = b.DateUtilisation,
                    EstUtilise = b.EstUtilise,
                    Statut = b.Statut,
                    DateCreation = b.DateCreation,
                    DateModification = b.DateModification
                }).ToList();

                var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi pour l'affilié ",
                    ex);
            }
        }

        [HttpGet("by-prestation/{prestationId}")]
        public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetByPrestation(
            int prestationId, 
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.BonsEnvoi
                    .Include(b => b.Affilie)
                    .Include(b => b.Prestation)
                    .Where(b => b.PrestationId == prestationId)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(b => new BonEnvoiReadDto
                {
                    IdBonEnvoi = b.IdBonEnvoi,
                    NumeroBon = b.NumeroBon,
                    AffilieId = b.AffilieId,
                    AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                    PrestationId = b.PrestationId,
                    PrestationNom = b.Prestation?.NomPrestation,
                    DateEmission = b.DateEmission,
                    DateUtilisation = b.DateUtilisation,
                    EstUtilise = b.EstUtilise,
                    Statut = b.Statut,
                    DateCreation = b.DateCreation,
                    DateModification = b.DateModification
                }).ToList();

                var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi pour la prestation ",
                    ex);
            }
        }

        [HttpGet("non-utilises")]
        public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetNonUtilises(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.BonsEnvoi
                    .Include(b => b.Affilie)
                    .Include(b => b.Prestation)
                    .Where(b => b.EstUtilise == false)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(b => new BonEnvoiReadDto
                {
                    IdBonEnvoi = b.IdBonEnvoi,
                    NumeroBon = b.NumeroBon,
                    AffilieId = b.AffilieId,
                    AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                    PrestationId = b.PrestationId,
                    PrestationNom = b.Prestation?.NomPrestation,
                    DateEmission = b.DateEmission,
                    DateUtilisation = b.DateUtilisation,
                    EstUtilise = b.EstUtilise,
                    Statut = b.Statut,
                    DateCreation = b.DateCreation,
                    DateModification = b.DateModification
                }).ToList();

                var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi non utilisés paginés",
                    ex);
            }
        }

        [HttpGet("utilises")]
        public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetUtilises(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.BonsEnvoi
                    .Include(b => b.Affilie)
                    .Include(b => b.Prestation)
                    .Where(b => b.EstUtilise == true)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(b => new BonEnvoiReadDto
                {
                    IdBonEnvoi = b.IdBonEnvoi,
                    NumeroBon = b.NumeroBon,
                    AffilieId = b.AffilieId,
                    AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                    PrestationId = b.PrestationId,
                    PrestationNom = b.Prestation?.NomPrestation,
                    DateEmission = b.DateEmission,
                    DateUtilisation = b.DateUtilisation,
                    EstUtilise = b.EstUtilise,
                    Statut = b.Statut,
                    DateCreation = b.DateCreation,
                    DateModification = b.DateModification
                }).ToList();

                var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi utilisés paginés",
                    ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<BonEnvoiReadDto>> Create([FromBody] BonEnvoiCreateDto createDto, CancellationToken ct = default)
        {
            return BadRequest(new
            {
                success = false,
                message = "La création standalone de BonEnvoi est désactivée. Utilisez le workflow DemandeBonEnvoi puis /api/DemandeBonEnvoi/{id}/confirmer."
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BonEnvoiReadDto>> Update(int id, [FromBody] BonEnvoiUpdateDto updateDto, CancellationToken ct = default)
        {
            var bon = new BonEnvoi
            {
                NumeroBon = updateDto.NumeroBon,
                AffilieId = updateDto.AffilieId,
                PrestationId = updateDto.PrestationId,
                DateUtilisation = updateDto.DateUtilisation,
                EstUtilise = updateDto.EstUtilise,
                Statut = updateDto.Statut,
                DateModification = DateTime.Now
            };

            var updated = await _bonEnvoiService.UpdateAsync(id, bon, ct);
            if (updated == null)
                return NotFound();

            var dto = new BonEnvoiReadDto
            {
                IdBonEnvoi = updated.IdBonEnvoi,
                NumeroBon = updated.NumeroBon,
                AffilieId = updated.AffilieId,
                AffilieNom = $"{updated.Affilie?.Nom} {updated.Affilie?.Prenom}".Trim(),
                PrestationId = updated.PrestationId,
                PrestationNom = updated.Prestation?.NomPrestation,
                DateEmission = updated.DateEmission,
                DateUtilisation = updated.DateUtilisation,
                EstUtilise = updated.EstUtilise,
                Statut = updated.Statut,
                DateCreation = updated.DateCreation,
                DateModification = updated.DateModification
            };
            
            return Ok(dto);
        }

        [HttpPost("{id}/utiliser")]
        public async Task<ActionResult> MarquerCommeUtilise(int id, CancellationToken ct = default)
        {
            var success = await _bonEnvoiService.MarquerCommeUtiliseAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _bonEnvoiService.DeleteAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }

    /// <summary>
    /// Récupère les bons d'envoi paginés
    /// </summary>
    [HttpGet("paginated")]
    public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetPaginated(
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var query = _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsQueryable();

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

            // Mapper les entités vers les DTOs et créer une nouvelle réponse
            var dtos = result.Data.Select(b => new BonEnvoiReadDto
            {
                IdBonEnvoi = b.IdBonEnvoi,
                NumeroBon = b.NumeroBon,
                AffilieId = b.AffilieId,
                AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                PrestationId = b.PrestationId,
                PrestationNom = b.Prestation?.NomPrestation,
                DateEmission = b.DateEmission,
                DateUtilisation = b.DateUtilisation,
                EstUtilise = b.EstUtilise,
                Statut = b.Statut,
                DateCreation = b.DateCreation,
                DateModification = b.DateModification
            }).ToList();

            var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi paginés",
                    ex);
            }
    }

    /// <summary>
    /// Récupère les bons d'envoi avec filtres avancés
    /// </summary>
    [HttpPost("advanced")]
    public async Task<ActionResult<ExtendedPaginatedResponse<BonEnvoiReadDto>>> GetBonsEnvoiAdvanced(
        [FromBody] AdvancedPaginationRequest request)
    {
        try
        {
            // Construire la requête de base
            var query = _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsQueryable();

            // Appliquer les filtres de base
            if (request.FilterList != null && request.FilterList.Any())
            {
                foreach (var filter in request.FilterList)
                {
                    switch (filter.Field.ToLower())
                    {
                        case "statut":
                            if (filter.Operator == "eq")
                                query = query.Where(b => b.Statut.ToString() == filter.Value);
                            break;
                        case "estutilise":
                            if (filter.Operator == "eq")
                                query = query.Where(b => b.EstUtilise == bool.Parse(filter.Value));
                            break;
                        case "numerobon":
                            if (filter.Operator == "contains")
                                query = query.Where(b => b.NumeroBon.Contains(filter.Value));
                            else if (filter.Operator == "eq")
                                query = query.Where(b => b.NumeroBon == filter.Value);
                            break;
                        case "affilieid":
                            if (filter.Operator == "eq")
                                query = query.Where(b => b.AffilieId == int.Parse(filter.Value));
                            break;
                        case "prestationid":
                            if (filter.Operator == "eq")
                                query = query.Where(b => b.PrestationId == int.Parse(filter.Value));
                            break;
                        case "affilienom":
                            if (filter.Operator == "contains")
                                query = query.Where(b => b.Affilie != null && 
                                    (b.Affilie.Nom.Contains(filter.Value) || b.Affilie.Prenom.Contains(filter.Value)));
                            break;
                        case "prestationnom":
                            if (filter.Operator == "contains")
                                query = query.Where(b => b.Prestation != null && b.Prestation.NomPrestation.Contains(filter.Value));
                            break;
                        case "dateemission":
                            if (filter.Operator == "eq")
                                query = query.Where(b => b.DateEmission.Date == DateTime.Parse(filter.Value).Date);
                            else if (filter.Operator == "gt")
                                query = query.Where(b => b.DateEmission > DateTime.Parse(filter.Value));
                            else if (filter.Operator == "lt")
                                query = query.Where(b => b.DateEmission < DateTime.Parse(filter.Value));
                            break;
                        case "dateutilisation":
                            if (filter.Operator == "eq")
                                query = query.Where(b => b.DateUtilisation.HasValue && b.DateUtilisation.Value.Date == DateTime.Parse(filter.Value).Date);
                            else if (filter.Operator == "gt")
                                query = query.Where(b => b.DateUtilisation.HasValue && b.DateUtilisation.Value > DateTime.Parse(filter.Value));
                            else if (filter.Operator == "lt")
                                query = query.Where(b => b.DateUtilisation.HasValue && b.DateUtilisation.Value < DateTime.Parse(filter.Value));
                            break;
                    }
                }
            }

            // Appliquer la pagination
            var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);

            // Mapper les entités vers les DTOs
            var bonDtos = response.Data.Select(b => new BonEnvoiReadDto
            {
                IdBonEnvoi = b.IdBonEnvoi,
                NumeroBon = b.NumeroBon,
                AffilieId = b.AffilieId,
                AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                PrestationId = b.PrestationId,
                PrestationNom = b.Prestation?.NomPrestation,
                DateEmission = b.DateEmission,
                DateUtilisation = b.DateUtilisation,
                EstUtilise = b.EstUtilise,
                Statut = b.Statut,
                DateCreation = b.DateCreation,
                DateModification = b.DateModification
            }).ToList();
            
            // Créer une nouvelle réponse avec les DTOs
            var dtoResponse = new ExtendedPaginatedResponse<BonEnvoiReadDto>
            {
                Data = bonDtos,
                CurrentPage = response.CurrentPage,
                PageSize = response.PageSize,
                TotalItems = response.TotalItems,
                TotalPages = response.TotalPages,
                HasNextPage = response.HasNextPage,
                HasPreviousPage = response.HasPreviousPage,
                AppliedFilters = request.FilterList?.Select(f => $"{f.Field} {f.Operator} {f.Value}").ToList() ?? new(),
                AppliedSorting = $"{request.SortBy} {request.SortDirection}"
            };

            return Ok(dtoResponse);
        }
        catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi avancés",
                    ex);
            }
    }

    /// <summary>
    /// Récupère les bons d'envoi par affilié avec pagination
    /// </summary>
    [HttpGet("by-affilie/{affilieId}/paginated")]
    public async Task<ActionResult<PaginatedResponse<BonEnvoiReadDto>>> GetByAffiliePaginated(
        int affilieId, 
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var query = _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .Where(b => b.AffilieId == affilieId)
                .AsQueryable();

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

            // Mapper les entités vers les DTOs et créer une nouvelle réponse
            var dtos = result.Data.Select(b => new BonEnvoiReadDto
            {
                IdBonEnvoi = b.IdBonEnvoi,
                NumeroBon = b.NumeroBon,
                AffilieId = b.AffilieId,
                AffilieNom = $"{b.Affilie?.Nom} {b.Affilie?.Prenom}".Trim(),
                PrestationId = b.PrestationId,
                PrestationNom = b.Prestation?.NomPrestation,
                DateEmission = b.DateEmission,
                DateUtilisation = b.DateUtilisation,
                EstUtilise = b.EstUtilise,
                Statut = b.Statut,
                DateCreation = b.DateCreation,
                DateModification = b.DateModification
            }).ToList();

            var paginatedDtos = new PaginatedResponse<BonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des bons d'envoi pour l'affilié ",
                    ex);
            }
    }
    }
}
