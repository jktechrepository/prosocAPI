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
    public class ProduitAssureurController : BaseApiController
    {
        private readonly IProduitAssureurRepository _produitAssureurRepository;
        private readonly IPrestationRepository _prestationRepository;
        private readonly ProsocDbContext _db;

        public ProduitAssureurController(
            IProduitAssureurRepository produitAssureurRepository,
            IPrestationRepository prestationRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<ProduitAssureurController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _produitAssureurRepository = produitAssureurRepository;
            _prestationRepository = prestationRepository;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ProduitAssureurReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.ProduitsAssureurs
                    .Include(p => p.Devise)
                    .Include(p => p.Partenaire)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var paginatedDtos = new PaginatedResponse<ProduitAssureurReadDto>
                {
                    Data = result.Data.Select(MapToReadDto).ToList(),
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
                    "Une erreur technique est survenue lors de la récupération des produits assureurs paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProduitAssureurReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var produit = await _produitAssureurRepository.GetByIdAsync(id, ct);
            return produit == null ? NotFound() : Ok(MapToReadDto(produit));
        }

        [HttpGet("by-assureur/{assureurId}")]
        public async Task<ActionResult<List<ProduitAssureurReadDto>>> GetByAssureur(int assureurId, CancellationToken ct = default)
        {
            var produits = await _produitAssureurRepository.GetByAssureurAsync(assureurId, ct);
            return Ok(produits.Select(MapToReadDto).ToList());
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<ProduitAssureurReadDto>>> GetActives(CancellationToken ct = default)
        {
            var produits = await _produitAssureurRepository.GetActivesAsync(ct);
            return Ok(produits.Select(MapToReadDto).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<ProduitAssureurReadDto>> Create([FromBody] ProduitAssureurCreateDto createDto, CancellationToken ct = default)
        {
            try
            {
                var created = await _produitAssureurRepository.CreateAsync(MapFromCreateDto(createDto), ct);

                var prestation = await _prestationRepository.GetByProduitAssureurAsync(created.IdProduit, ct);
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
        public async Task<ActionResult<ProduitAssureurReadDto>> Update(int id, [FromBody] ProduitAssureurUpdateDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var updated = await _produitAssureurRepository.UpdateAsync(id, MapFromUpdateDto(updateDto), ct);
                return updated == null
                    ? NotFound()
                    : Ok(await MapToReadDtoWithPrestationAsync(updated, ct));
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
                var success = await _produitAssureurRepository.DeleteAsync(id, ct);
                return success ? NoContent() : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<ProduitAssureurReadDto> MapToReadDtoWithPrestationAsync(ProduitAssureur p, CancellationToken ct)
        {
            var dto = MapToReadDto(p);
            var prestations = await _prestationRepository.GetByProduitAssureurAsync(p.IdProduit, ct);
            dto.PrestationCree = prestations.Any();
            dto.PrestationId = prestations.FirstOrDefault()?.IdPrestation;
            return dto;
        }

        private static ProduitAssureurReadDto MapToReadDto(ProduitAssureur p)
        {
            var dto = new ProduitAssureurReadDto
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
                AssureurId = p.AssureurId,
                AssureurNom = p.Partenaire?.Nom,
                DeviseId = p.DeviseId,
                DeviseCode = p.Devise?.Code,
                DeviseNom = p.Devise?.Nom
            };
            ProduitCommissionMapping.CopyRatesToDto(dto, p);
            return dto;
        }

        private static ProduitAssureur MapFromCreateDto(ProduitAssureurCreateDto dto)
        {
            var produit = new ProduitAssureur
            {
                Nom = dto.Nom,
                Montant = dto.Montant,
                Periodicite = dto.Periodicite,
                AgeMin = dto.AgeMin,
                AgeMax = dto.AgeMax,
                EstGratuit = dto.EstGratuit,
                Statut = dto.Statut,
                DateCreation = DateTime.Now,
                AssureurId = dto.AssureurId,
                DeviseId = dto.DeviseId
            };
            ProduitCommissionMapping.ApplyRates(produit, dto);
            return produit;
        }

        private static ProduitAssureur MapFromUpdateDto(ProduitAssureurUpdateDto dto)
        {
            var produit = new ProduitAssureur
            {
                Nom = dto.Nom,
                Montant = dto.Montant,
                Periodicite = dto.Periodicite,
                AgeMin = dto.AgeMin,
                AgeMax = dto.AgeMax,
                EstGratuit = dto.EstGratuit,
                Statut = dto.Statut,
                AssureurId = dto.AssureurId,
                DeviseId = dto.DeviseId
            };
            ProduitCommissionMapping.ApplyRates(produit, dto);
            return produit;
        }
    }
}
