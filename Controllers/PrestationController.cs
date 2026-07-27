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
    public class PrestationController : BaseApiController
    {
        private readonly IPrestationRepository _prestationRepository;
        private readonly ProsocDbContext _db;

        public PrestationController(
            IPrestationRepository prestationRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<PrestationController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _prestationRepository = prestationRepository;
            _db = db;
        }

        private static IQueryable<Prestation> WithIncludes(IQueryable<Prestation> query) =>
            query
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .Include(p => p.Devise);

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponse<PrestationReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = WithIncludes(_db.Prestations.AsQueryable());
                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
                var dtos = result.Data.Select(PrestationHelpers.ToReadDto).ToList();

                return Ok(new PaginatedResponse<PrestationReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des prestations paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PrestationReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var prestation = await _prestationRepository.GetByIdAsync(id, ct);
            if (prestation == null)
                return NotFound();

            return Ok(PrestationHelpers.ToReadDto(prestation));
        }

        [HttpGet("by-produit-mutuel/{produitMutuelId}")]
        public async Task<ActionResult<List<PrestationReadDto>>> GetByProduitMutuel(int produitMutuelId, CancellationToken ct = default)
        {
            var prestations = await _prestationRepository.GetByProduitMutuelAsync(produitMutuelId, ct);
            return Ok(prestations.Select(PrestationHelpers.ToReadDto).ToList());
        }

        [HttpGet("by-produit-assureur/{produitAssureurId}")]
        public async Task<ActionResult<List<PrestationReadDto>>> GetByProduitAssureur(int produitAssureurId, CancellationToken ct = default)
        {
            var prestations = await _prestationRepository.GetByProduitAssureurAsync(produitAssureurId, ct);
            return Ok(prestations.Select(PrestationHelpers.ToReadDto).ToList());
        }

        [HttpPost]
        public ActionResult<PrestationReadDto> Create([FromBody] PrestationCreateDto _)
        {
            return PrestationsWriteForbidden();
        }

        [HttpPut("{id}")]
        public ActionResult<PrestationReadDto> Update(int id, [FromBody] PrestationUpdateDto _)
        {
            return PrestationsWriteForbidden();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            return PrestationsWriteForbidden();
        }

        /// <summary>
        /// Écriture standalone fermée : sync via CREATE/UPDATE Produit Mutuel / Assureur.
        /// </summary>
        private ActionResult PrestationsWriteForbidden() =>
            ErrorResponse(
                "Les prestations sont gérées via les endpoints Produit (CREATE/UPDATE_PRODUIT_MUTUEL | CREATE/UPDATE_PRODUIT_ASSUREUR).",
                403);

        /// <summary>
        /// Récupère les prestations avec filtres avancés
        /// </summary>
        [HttpPost("advanced")]
        public async Task<ActionResult<ExtendedPaginatedResponse<PrestationReadDto>>> GetPrestationsAdvanced(
            [FromBody] AdvancedPaginationRequest request)
        {
            try
            {
                var query = WithIncludes(_db.Prestations.AsQueryable());

                if (request.FilterList != null && request.FilterList.Any())
                {
                    foreach (var filter in request.FilterList)
                    {
                        switch (filter.Field.ToLower())
                        {
                            case "produitmutuelid":
                                if (filter.Operator == "eq")
                                    query = query.Where(p => p.ProduitMutuelId == int.Parse(filter.Value));
                                break;
                            case "produitassureurid":
                                if (filter.Operator == "eq")
                                    query = query.Where(p => p.ProduitAssureurId == int.Parse(filter.Value));
                                break;
                            case "nomprestation":
                                if (filter.Operator == "contains")
                                    query = query.Where(p => p.NomPrestation.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(p => p.NomPrestation == filter.Value);
                                break;
                            case "description":
                                if (filter.Operator == "contains")
                                    query = query.Where(p => p.Description != null && p.Description.Contains(filter.Value));
                                break;
                            case "periodicite":
                                if (filter.Operator == "contains")
                                    query = query.Where(p => p.Periodicite.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(p => p.Periodicite == filter.Value);
                                break;
                            case "produitmutuelnom":
                                if (filter.Operator == "contains")
                                    query = query.Where(p => p.ProduitMutuel != null && p.ProduitMutuel.Nom.Contains(filter.Value));
                                break;
                            case "produitassureurnom":
                                if (filter.Operator == "contains")
                                    query = query.Where(p => p.ProduitAssureur != null && p.ProduitAssureur.Nom.Contains(filter.Value));
                                break;
                        }
                    }
                }

                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);
                var prestationDtos = response.Data.Select(PrestationHelpers.ToReadDto).ToList();
                
                return Ok(new ExtendedPaginatedResponse<PrestationReadDto>
                {
                    Data = prestationDtos,
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
                    "Une erreur technique est survenue lors de la récupération des prestations avancées",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les prestations par produit mutuel avec pagination
        /// </summary>
        [HttpGet("by-produit-mutuel/{produitMutuelId}/paginated")]
        public async Task<ActionResult<PaginatedResponse<PrestationReadDto>>> GetByProduitMutuel(
            int produitMutuelId, 
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = WithIncludes(_db.Prestations.AsQueryable())
                    .Where(p => p.ProduitMutuelId == produitMutuelId);

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
                var dtos = result.Data.Select(PrestationHelpers.ToReadDto).ToList();

                return Ok(new PaginatedResponse<PrestationReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des prestations pour le produit mutuel ",
                    ex);
            }
        }
    }
}
