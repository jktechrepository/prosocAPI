using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : BaseApiController
    {
        private readonly IPermissionRepository _repo;
        private readonly ProsocDbContext _db;

        public PermissionController(
            IPermissionRepository repo,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<PermissionController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<PermissionReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Permissions
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<PermissionReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des permissions paginées",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PermissionReadDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(ToReadDto(item));
        }

        [HttpPost]
        public async Task<ActionResult<PermissionReadDto>> Create([FromBody] PermissionCreateDto input, CancellationToken ct)
        {
            var entity = new Permission
            {
                Nom = input.Nom,
                Description = input.Description,
                Categorie = input.Categorie,
                Action = input.Action,
                Statut = input.Statut
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdPermission }, ToReadDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PermissionReadDto>> Update([FromRoute] int id, [FromBody] PermissionUpdateDto input, CancellationToken ct)
        {
            var entity = new Permission
            {
                Nom = input.Nom,
                Description = input.Description,
                Categorie = input.Categorie,
                Action = input.Action,
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

        private static PermissionReadDto ToReadDto(Permission p)
        {
            return new PermissionReadDto
            {
                IdPermission = p.IdPermission,
                Nom = p.Nom,
                Description = p.Description,
                Categorie = p.Categorie,
                Action = p.Action,
                Statut = p.Statut,
                DateCreation = p.DateCreation
            };
        }
    }
}
