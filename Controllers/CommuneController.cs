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
    public class CommuneController : BaseApiController
    {
        private readonly ICommuneRepository _repo;
        private readonly ITerritorialEncadrementService _encadrement;
        private readonly ProsocDbContext _db;

        public CommuneController(
            ICommuneRepository repo,
            ITerritorialEncadrementService encadrement,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<CommuneController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _encadrement = encadrement;
            _db = db;
        }

        private static CommuneReadDto MapToDto(Commune c) => new()
        {
            Id = c.IdCommune,
            Nom = c.Nom,
            ProvinceId = c.ProvinceId,
            ProvinceNom = c.Province?.Nom,
            NombreZones = c.Zones?.Count ?? 0,
            SuperviseurAgentId = c.SuperviseurAgentId,
            SuperviseurNom = c.Superviseur?.NomComplet
        };

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CommuneReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Communes
                    .Include(c => c.Province)
                    .Include(c => c.Superviseur)
                    .Include(c => c.Zones)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(MapToDto).ToList();

                var paginatedDtos = new PaginatedResponse<CommuneReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des communes paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommuneReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var commune = await _repo.GetByIdAsync(id, ct);
            if (commune == null)
                return NotFound();

            var dto = MapToDto(commune);

            return Ok(dto);
        }

        [HttpPut("{id}/superviseur")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult<TerritorialAffectationResultDto>> AssignSuperviseur(
            int id,
            [FromBody] TerritorialAffectationDto body,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _encadrement.AssignSuperviseurAsync(id, body.AgentId, GetCurrentUserId(), ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Erreur lors de l'affectation du superviseur", ex);
            }
        }

        [HttpDelete("{id}/superviseur")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult<TerritorialAffectationResultDto>> ClearSuperviseur(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _encadrement.ClearSuperviseurAsync(id, GetCurrentUserId(), ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Erreur lors du retrait du superviseur", ex);
            }
        }

        [HttpGet("by-province/{provinceId}")]
        public async Task<ActionResult<List<CommuneReadDto>>> GetByProvince(int provinceId, CancellationToken ct = default)
        {
            var communes = await _repo.GetByProvinceAsync(provinceId, ct);
            var dtos = communes.Select(MapToDto).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<CommuneReadDto>> Create([FromBody] CommuneCreateDto createDto, CancellationToken ct = default)
        {
            var entity = new Commune
            {
                Nom = createDto.Nom,
                ProvinceId = createDto.ProvinceId,
                Statut = createDto.Statut,
                DateCreation = DateTime.Now
            };

            var created = await _repo.CreateAsync(entity, ct);

            var dto = MapToDto(created);
            dto.ProvinceNom = null;
            dto.NombreZones = 0;

            return CreatedAtAction(nameof(GetById), new { id = created.IdCommune }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CommuneReadDto>> Update(int id, [FromBody] CommuneUpdateDto updateDto, CancellationToken ct = default)
        {
            var entity = new Commune
            {
                Nom = updateDto.Nom,
                ProvinceId = updateDto.ProvinceId,
                Statut = updateDto.Statut
            };

            var updated = await _repo.UpdateAsync(id, entity, ct);
            if (updated == null)
                return NotFound();

            var dto = MapToDto(updated);

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
