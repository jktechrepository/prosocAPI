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
    public class AssureurController : BaseApiController
    {
        private readonly IAssureurRepository _assureurRepository;
        private readonly ProsocDbContext _db;

        public AssureurController(
            IAssureurRepository assureurRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<AssureurController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _assureurRepository = assureurRepository;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<AssureurReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Assureurs
                    .Include(a => a.Produits)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(a => new AssureurReadDto
                {
                    Id = a.IdAssureur,
                    Nom = a.Nom,
                    Description = a.Description,
                    Statut = a.Statut,
                    DateCreation = a.DateCreation,
                    DateModification = a.DateModification,
                    NombreProduits = a.Produits?.Count ?? 0
                }).ToList();

                var paginatedDtos = new PaginatedResponse<AssureurReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des assureurs paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssureurReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var assureur = await _assureurRepository.GetByIdAsync(id, ct);
            if (assureur == null)
                return NotFound();

            var dto = new AssureurReadDto
            {
                Id = assureur.IdAssureur,
                Nom = assureur.Nom,
                Description = assureur.Description,
                Statut = assureur.Statut,
                DateCreation = assureur.DateCreation,
                DateModification = assureur.DateModification,
                NombreProduits = assureur.Produits?.Count ?? 0
            };
            return Ok(dto);
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<AssureurReadDto>>> GetActives(CancellationToken ct = default)
        {
            var assureurs = await _assureurRepository.GetActivesAsync(ct);
            var dtos = assureurs.Select(a => new AssureurReadDto
            {
                Id = a.IdAssureur,
                Nom = a.Nom,
                Description = a.Description,
                Statut = a.Statut,
                DateCreation = a.DateCreation,
                DateModification = a.DateModification,
                NombreProduits = a.Produits?.Count ?? 0
            }).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<AssureurReadDto>> Create([FromBody] AssureurCreateDto createDto, CancellationToken ct = default)
        {
            var assureur = new Assureur
            {
                Nom = createDto.Nom,
                Description = createDto.Description,
                Statut = createDto.Statut,
                DateCreation = DateTime.Now
            };

            var created = await _assureurRepository.CreateAsync(assureur, ct);
            
            var dto = new AssureurReadDto
            {
                Id = created.IdAssureur,
                Nom = created.Nom,
                Description = created.Description,
                Statut = created.Statut,
                DateCreation = created.DateCreation,
                DateModification = created.DateModification,
                NombreProduits = 0
            };
            
            return CreatedAtAction(nameof(GetById), new { id = created.IdAssureur }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AssureurReadDto>> Update(int id, [FromBody] AssureurUpdateDto updateDto, CancellationToken ct = default)
        {
            var assureur = new Assureur
            {
                Nom = updateDto.Nom,
                Description = updateDto.Description,
                Statut = updateDto.Statut,
                DateModification = DateTime.Now
            };

            var updated = await _assureurRepository.UpdateAsync(id, assureur, ct);
            if (updated == null)
                return NotFound();

            var dto = new AssureurReadDto
            {
                Id = updated.IdAssureur,
                Nom = updated.Nom,
                Description = updated.Description,
                Statut = updated.Statut,
                DateCreation = updated.DateCreation,
                DateModification = updated.DateModification,
                NombreProduits = updated.Produits?.Count ?? 0
            };
            
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _assureurRepository.DeleteAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }
    }
}
