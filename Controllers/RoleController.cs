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
    public class RoleController : BaseApiController
    {
        private readonly IRoleRepository _repo;
        private readonly ProsocDbContext _db;

        public RoleController(
            IRoleRepository repo,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<RoleController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<RoleReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Roles
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<RoleReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des rôles paginés",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoleReadDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(ToReadDto(item));
        }

        [HttpPost]
        public async Task<ActionResult<RoleReadDto>> Create([FromBody] RoleCreateDto input, CancellationToken ct)
        {
            var entity = new Role
            {
                Nom = input.Nom,
                Description = input.Description,
                Niveau = input.Niveau,
                Statut = input.Statut
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdRole }, ToReadDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<RoleReadDto>> Update([FromRoute] int id, [FromBody] RoleUpdateDto input, CancellationToken ct)
        {
            var entity = new Role
            {
                Nom = input.Nom,
                Description = input.Description,
                Niveau = input.Niveau,
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

        private static RoleReadDto ToReadDto(Role r)
        {
            return new RoleReadDto
            {
                IdRole = r.IdRole,
                Nom = r.Nom,
                Description = r.Description,
                Niveau = r.Niveau,
                Statut = r.Statut,
                DateCreation = r.DateCreation
            };
        }
    }
}
