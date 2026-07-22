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
    public class TypeAdhesionController : BaseApiController
    {
        private readonly ITypeAdhesionRepository _repo;
        private readonly ProsocDbContext _db;

        public TypeAdhesionController(
            ITypeAdhesionRepository repo,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<TypeAdhesionController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponse<TypeAdhesionReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.TypeAdhesions
                    .Include(t => t.CategorieAdhesion)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<TypeAdhesionReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des types d'adhésion paginés",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<TypeAdhesionReadDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(ToReadDto(item));
        }

        [HttpPost]
        public async Task<ActionResult<TypeAdhesionReadDto>> Create([FromBody] TypeAdhesionCreateDto input, CancellationToken ct)
        {
            var entity = new TypeAdhesion
            {
                Libelle = input.Libelle,
                CategorieAdhesionId = input.CategorieAdhesionId,
                MaxDependants = input.MaxDependants,
                Description = input.Description,
                Montant = input.Montant,
                DeviseId = input.DeviseId,
                Statut = input.Statut
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdTypeAdhesion }, ToReadDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TypeAdhesionReadDto>> Update([FromRoute] int id, [FromBody] TypeAdhesionUpdateDto input, CancellationToken ct)
        {
            var entity = new TypeAdhesion
            {
                Libelle = input.Libelle,
                CategorieAdhesionId = input.CategorieAdhesionId,
                MaxDependants = input.MaxDependants,
                Description = input.Description,
                Montant = input.Montant,
                DeviseId = input.DeviseId,
                Statut = input.Statut
            };

            var updated = await _repo.UpdateAsync(id, entity, ct);
            return updated == null ? NotFound() : Ok(ToReadDto(updated));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        private static TypeAdhesionReadDto ToReadDto(TypeAdhesion entity)
        {
            return new TypeAdhesionReadDto
            {
                Id = entity.IdTypeAdhesion,
                Libelle = entity.Libelle,
                MaxDependants = entity.MaxDependants,
                Description = entity.Description,
                Montant = entity.Montant,
                DeviseId = entity.DeviseId,
                Statut = entity.Statut,
                DateCreation = entity.DateCreation,
                CategorieAdhesionId = entity.CategorieAdhesionId
            };
        }
    }
}
