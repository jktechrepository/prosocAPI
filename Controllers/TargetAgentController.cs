using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TargetAgentController : BaseApiController
    {
        private readonly ITargetAgentRepository _targetAgentRepository;
        private readonly ProsocDbContext _db;

        public TargetAgentController(
            ITargetAgentRepository targetAgentRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<TargetAgentController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _targetAgentRepository = targetAgentRepository;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TargetAgentReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.TargetsAgents
                    .Include(t => t.Role)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
                var paginatedDtos = new PaginatedResponse<TargetAgentReadDto>
                {
                    Data = result.Data.Select(MapToDto).ToList(),
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
                    "Une erreur technique est survenue lors de la récupération des targets d'agents paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TargetAgentReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var target = await _targetAgentRepository.GetByIdAsync(id, ct);
            if (target == null)
                return NotFound();

            return Ok(MapToDto(target));
        }

        [HttpGet("by-role/{roleNom}")]
        public async Task<ActionResult<List<TargetAgentReadDto>>> GetByRole(string roleNom, CancellationToken ct = default)
        {
            var role = await TargetAgentRoleResolver.ResolveRoleByNomAsync(_db, roleNom, ct);
            if (role == null)
                return NotFound(new { error = $"Rôle '{roleNom}' introuvable ou inactif." });

            var targets = await _targetAgentRepository.GetByRoleAsync(role.IdRole, ct);
            return Ok(targets.Select(MapToDto).ToList());
        }

        [HttpGet("actifs")]
        public async Task<ActionResult<List<TargetAgentReadDto>>> GetActifs(CancellationToken ct = default)
        {
            var targets = await _targetAgentRepository.GetActifsAsync(ct);
            return Ok(targets.Select(MapToDto).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<TargetAgentReadDto>> Create(
            [FromBody] TargetAgentCreateDto createDto,
            CancellationToken ct = default)
        {
            var role = await TargetAgentRoleResolver.ResolveRoleByNomAsync(_db, createDto.RoleNom, ct);
            if (role == null)
                return BadRequest(new { error = $"Rôle '{createDto.RoleNom}' introuvable ou inactif." });

            if (createDto.Statut && await _targetAgentRepository.HasActiveConflictAsync(
                    role.IdRole, createDto.Periodicite, null, ct))
            {
                return BadRequest(new
                {
                    error = $"Une cible active existe déjà pour le rôle '{role.Nom}' et la périodicité '{createDto.Periodicite}'."
                });
            }

            var target = MapFromCreateDto(createDto, role.IdRole);
            var created = await _targetAgentRepository.CreateAsync(target, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdTargetAgent }, MapToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TargetAgentReadDto>> Update(
            int id,
            [FromBody] TargetAgentUpdateDto updateDto,
            CancellationToken ct = default)
        {
            var role = await TargetAgentRoleResolver.ResolveRoleByNomAsync(_db, updateDto.RoleNom, ct);
            if (role == null)
                return BadRequest(new { error = $"Rôle '{updateDto.RoleNom}' introuvable ou inactif." });

            if (updateDto.Statut && await _targetAgentRepository.HasActiveConflictAsync(
                    role.IdRole, updateDto.Periodicite, id, ct))
            {
                return BadRequest(new
                {
                    error = $"Une cible active existe déjà pour le rôle '{role.Nom}' et la périodicité '{updateDto.Periodicite}'."
                });
            }

            var target = MapFromUpdateDto(updateDto, role.IdRole);
            var updated = await _targetAgentRepository.UpdateAsync(id, target, ct);
            if (updated == null)
                return NotFound();

            return Ok(MapToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _targetAgentRepository.DeleteAsync(id, ct);
            if (!success)
                return NotFound();

            return NoContent();
        }

        private static TargetAgentReadDto MapToDto(TargetAgent t) => new()
        {
            IdTargetAgent = t.IdTargetAgent,
            RoleId = t.RoleId,
            RoleNom = t.Role?.Nom,
            LibelleTarget = t.LibelleTarget,
            Periodicite = t.Periodicite,
            Nombre = t.Nombre,
            Statut = t.Statut
        };

        private static TargetAgent MapFromCreateDto(TargetAgentCreateDto dto, int roleId) => new()
        {
            RoleId = roleId,
            LibelleTarget = dto.LibelleTarget,
            Periodicite = dto.Periodicite,
            Nombre = dto.Nombre,
            Statut = dto.Statut
        };

        private static TargetAgent MapFromUpdateDto(TargetAgentUpdateDto dto, int roleId) => new()
        {
            RoleId = roleId,
            LibelleTarget = dto.LibelleTarget,
            Periodicite = dto.Periodicite,
            Nombre = dto.Nombre,
            Statut = dto.Statut
        };
    }
}
