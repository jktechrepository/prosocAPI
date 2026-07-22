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
    public class ZoneSocialeController : BaseApiController
    {
        private readonly IZoneSocialeRepository _repo;
        private readonly ITerritorialEncadrementService _encadrement;
        private readonly ProsocDbContext _db;

        public ZoneSocialeController(
            IZoneSocialeRepository repo,
            ITerritorialEncadrementService encadrement,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<ZoneSocialeController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _encadrement = encadrement;
            _db = db;
        }

        private static ZoneSocialeReadDto MapToDto(ZoneSociale z) => new()
        {
            Id = z.IdZoneSociale,
            Nom = z.Nom,
            CommuneId = z.CommuneId,
            CommuneNom = z.Commune?.Nom,
            Statut = z.Statut,
            ChefEquipeAgentId = z.ChefEquipeAgentId,
            ChefEquipeNom = z.ChefEquipe?.NomComplet
        };

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ZoneSocialeReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.ZonesSociales
                    .Include(z => z.Commune)
                    .Include(z => z.ChefEquipe)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(MapToDto).ToList();

                var paginatedDtos = new PaginatedResponse<ZoneSocialeReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des zones sociales paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ZoneSocialeReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var zone = await _repo.GetByIdAsync(id, ct);
            if (zone == null)
                return NotFound();

            var dto = MapToDto(zone);

            return Ok(dto);
        }

        [HttpPut("{id}/chef-equipe")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult<TerritorialAffectationResultDto>> AssignChefEquipe(
            int id,
            [FromBody] TerritorialAffectationDto body,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _encadrement.AssignChefEquipeAsync(id, body.AgentId, GetCurrentUserId(), ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Erreur lors de l'affectation du chef d'équipe", ex);
            }
        }

        [HttpDelete("{id}/chef-equipe")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult<TerritorialAffectationResultDto>> ClearChefEquipe(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _encadrement.ClearChefEquipeAsync(id, GetCurrentUserId(), ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Erreur lors du retrait du chef d'équipe", ex);
            }
        }

        [HttpGet("by-commune/{communeId}")]
        public async Task<ActionResult<List<ZoneSocialeReadDto>>> GetByCommune(int communeId, CancellationToken ct = default)
        {
            var zones = await _repo.GetByCommuneAsync(communeId, ct);
            var dtos = zones.Select(MapToDto).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<ZoneSocialeReadDto>> Create([FromBody] ZoneSocialeCreateDto createDto, CancellationToken ct = default)
        {
            var entity = new ZoneSociale
            {
                Nom = createDto.Nom,
                CommuneId = createDto.CommuneId,
                Statut = createDto.Statut,
                DateCreation = DateTime.Now
            };

            var created = await _repo.CreateAsync(entity, ct);

            var dto = MapToDto(created);
            dto.CommuneNom = null;

            return CreatedAtAction(nameof(GetById), new { id = created.IdZoneSociale }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ZoneSocialeReadDto>> Update(int id, [FromBody] ZoneSocialeUpdateDto updateDto, CancellationToken ct = default)
        {
            var entity = new ZoneSociale
            {
                Nom = updateDto.Nom,
                CommuneId = updateDto.CommuneId,
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
