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

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategorieAdhesionController : BaseApiController
    {
        private readonly ICategorieAdhesionRepository _repo;
        private readonly ProsocDbContext _db;

        public CategorieAdhesionController(
            ICategorieAdhesionRepository repo,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<CategorieAdhesionController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CategorieAdhesionReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.CategoriesAdhesions
                    .Include(c => c.TypeAdhesions)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<CategorieAdhesionReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des catégories d'adhésion paginées",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategorieAdhesionReadDto>> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(ToReadDto(item));
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<CategorieAdhesionReadDto>>> GetActives(CancellationToken ct = default)
        {
            var items = await _repo.GetActivesAsync(ct);
            var dtos = items.Select(ToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<CategorieAdhesionReadDto>> Create([FromBody] CategorieAdhesionCreateDto input, CancellationToken ct = default)
        {
            var entity = new CategorieAdhesion
            {
                Libelle = input.Libelle,
                Description = input.Description,
                Statut = input.Statut,
                DateCreation = DateTime.Now
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdCategorieAdhesion }, ToReadDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategorieAdhesionReadDto>> Update([FromRoute] int id, [FromBody] CategorieAdhesionUpdateDto input, CancellationToken ct = default)
        {
            var entity = new CategorieAdhesion
            {
                Libelle = input.Libelle,
                Description = input.Description,
                Statut = input.Statut,
                DateModification = DateTime.Now
            };

            var updated = await _repo.UpdateAsync(id, entity, ct);
            return updated == null ? NotFound() : Ok(ToReadDto(updated));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        private static CategorieAdhesionReadDto ToReadDto(CategorieAdhesion entity)
        {
            return new CategorieAdhesionReadDto
            {
                IdCategorieAdhesion = entity.IdCategorieAdhesion,
                Libelle = entity.Libelle,
                Description = entity.Description,
                Statut = entity.Statut,
                DateCreation = entity.DateCreation,
                DateModification = entity.DateModification,
                NombreAdhesions = 0
            };
        }
    }
}
