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
    public class ProvinceController : BaseApiController
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly ProsocDbContext _db;

        public ProvinceController(
            IProvinceRepository provinceRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<ProvinceController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _provinceRepository = provinceRepository;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ProvinceReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Provinces
                    .Include(p => p.Communes)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(p => new ProvinceReadDto
                {
                    Id = p.IdProvince,
                    Nom = p.Nom,
                    Statut = p.Statut,
                    NombreCommunes = p.Communes?.Count ?? 0
                }).ToList();

                var paginatedDtos = new PaginatedResponse<ProvinceReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des provinces paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProvinceReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var province = await _provinceRepository.GetByIdAsync(id, ct);
            if (province == null)
                return NotFound();

            var dto = new ProvinceReadDto
            {
                Id = province.IdProvince,
                Nom = province.Nom,
                Statut = province.Statut,
                NombreCommunes = province.Communes?.Count ?? 0
            };
            return Ok(dto);
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<ProvinceReadDto>>> GetActives(CancellationToken ct = default)
        {
            var provinces = await _provinceRepository.GetActivesAsync(ct);
            var dtos = provinces.Select(p => new ProvinceReadDto
            {
                Id = p.IdProvince,
                Nom = p.Nom,
                Statut = p.Statut,
                NombreCommunes = p.Communes?.Count ?? 0
            }).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<ProvinceReadDto>> Create([FromBody] ProvinceCreateDto createDto, CancellationToken ct = default)
        {
            var province = new Province
            {
                Nom = createDto.Nom,
                Statut = createDto.Statut
            };

            var created = await _provinceRepository.CreateAsync(province, ct);
            
            var dto = new ProvinceReadDto
            {
                Id = created.IdProvince,
                Nom = created.Nom,
                Statut = created.Statut,
                NombreCommunes = 0
            };
            
            return CreatedAtAction(nameof(GetById), new { id = created.IdProvince }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProvinceReadDto>> Update(int id, [FromBody] ProvinceUpdateDto updateDto, CancellationToken ct = default)
        {
            var province = new Province
            {
                Nom = updateDto.Nom,
                Statut = updateDto.Statut
            };

            var updated = await _provinceRepository.UpdateAsync(id, province, ct);
            if (updated == null)
                return NotFound();

            var dto = new ProvinceReadDto
            {
                Id = updated.IdProvince,
                Nom = updated.Nom,
                Statut = updated.Statut,
                NombreCommunes = updated.Communes?.Count ?? 0
            };
            
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _provinceRepository.DeleteAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }
    }
}
