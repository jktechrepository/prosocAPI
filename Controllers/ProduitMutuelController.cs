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
    public class ProduitMutuelController : BaseApiController
    {
        private readonly IProduitMutuelRepository _produitMutuelRepository;
        private readonly IPrestationRepository _prestationRepository;
        private readonly ProsocDbContext _db;

        public ProduitMutuelController(
            IProduitMutuelRepository produitMutuelRepository, 
            IPrestationRepository prestationRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<ProduitMutuelController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _produitMutuelRepository = produitMutuelRepository;
            _prestationRepository = prestationRepository;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ProduitMutuelReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.ProduitsMutuels
                    .Include(p => p.Devise)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(MapToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<ProduitMutuelReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des produits mutuels paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProduitMutuelReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var produit = await _produitMutuelRepository.GetByIdAsync(id, ct);
            if (produit == null)
                return NotFound();

            return Ok(MapToReadDto(produit));
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<ProduitMutuelReadDto>>> GetActives(CancellationToken ct = default)
        {
            var produits = await _produitMutuelRepository.GetActivesAsync(ct);
            var dtos = produits.Select(MapToReadDto).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<ProduitMutuelReadDto>> Create([FromBody] ProduitMutuelCreateDto createDto, CancellationToken ct = default)
        {
            if (!HasPermission("CREATE_PRODUIT_MUTUEL"))
                return ForbiddenPermission("CREATE_PRODUIT_MUTUEL");

            try
            {
                var produit = MapFromCreateDto(createDto);
                var created = await _produitMutuelRepository.CreateAsync(produit, ct);

                var prestation = await _prestationRepository.GetByProduitMutuelAsync(created.IdProduit, ct);
                var dto = MapToReadDto(created);
                dto.PrestationCree = prestation.Any();
                dto.PrestationId = prestation.FirstOrDefault()?.IdPrestation;

                return CreatedAtAction(nameof(GetById), new { id = created.IdProduit }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProduitMutuelReadDto>> Update(int id, [FromBody] ProduitMutuelUpdateDto updateDto, CancellationToken ct = default)
        {
            if (!HasPermission("UPDATE_PRODUIT_MUTUEL"))
                return ForbiddenPermission("UPDATE_PRODUIT_MUTUEL");

            try
            {
                var produit = MapFromUpdateDto(updateDto);
                var updated = await _produitMutuelRepository.UpdateAsync(id, produit, ct);
                if (updated == null)
                    return NotFound();

                return Ok(await MapToReadDtoWithPrestationAsync(updated, ct));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var success = await _produitMutuelRepository.DeleteAsync(id, ct);
                return success ? NoContent() : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<ProduitMutuelReadDto> MapToReadDtoWithPrestationAsync(ProduitMutuel p, CancellationToken ct)
        {
            var dto = MapToReadDto(p);
            var prestations = await _prestationRepository.GetByProduitMutuelAsync(p.IdProduit, ct);
            dto.PrestationCree = prestations.Any();
            dto.PrestationId = prestations.FirstOrDefault()?.IdPrestation;
            return dto;
        }

        private static ProduitMutuelReadDto MapToReadDto(ProduitMutuel p)
        {
            var dto = new ProduitMutuelReadDto
            {
                Id = p.IdProduit,
                Nom = p.Nom,
                Montant = p.Montant,
                Periodicite = p.Periodicite,
                AgeMin = p.AgeMin,
                AgeMax = p.AgeMax,
                EstGratuit = p.EstGratuit,
                Statut = p.Statut,
                DateCreation = p.DateCreation,
                DeviseId = p.DeviseId,
                DeviseCode = p.Devise?.Code,
                DeviseNom = p.Devise?.Nom
            };
            ProduitCommissionMapping.CopyRatesToDto(dto, p);
            return dto;
        }

        private static ProduitMutuel MapFromCreateDto(ProduitMutuelCreateDto dto)
        {
            var produit = new ProduitMutuel
            {
                Nom = dto.Nom,
                Montant = dto.Montant,
                Periodicite = dto.Periodicite,
                AgeMin = dto.AgeMin,
                AgeMax = dto.AgeMax,
                EstGratuit = dto.EstGratuit,
                DeviseId = dto.DeviseId,
                Statut = dto.Statut,
                DateCreation = DateTime.Now
            };
            ProduitCommissionMapping.ApplyRates(produit, dto);
            return produit;
        }

        private static ProduitMutuel MapFromUpdateDto(ProduitMutuelUpdateDto dto)
        {
            var produit = new ProduitMutuel
            {
                Nom = dto.Nom,
                Montant = dto.Montant,
                Periodicite = dto.Periodicite,
                AgeMin = dto.AgeMin,
                AgeMax = dto.AgeMax,
                EstGratuit = dto.EstGratuit,
                DeviseId = dto.DeviseId,
                Statut = dto.Statut
            };
            ProduitCommissionMapping.ApplyRates(produit, dto);
            return produit;
        }
    }
}
