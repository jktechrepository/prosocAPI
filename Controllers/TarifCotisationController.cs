using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    /// <summary>
    /// Catalogue des tarifs de cotisation (sans notion d'affilié).
    /// Utiliser Collecte(TypeCollecte=Cotisation) pour enregistrer un paiement d'affilié.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TarifCotisationController : BaseApiController
    {
        private readonly ITarifCotisationRepository _repo;
        private readonly ITarifCotisationMetierService _cotisationMetier;
        private readonly ProsocDbContext _db;

        public TarifCotisationController(
            ITarifCotisationRepository repo,
            ITarifCotisationMetierService cotisationMetier,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<TarifCotisationController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _cotisationMetier = cotisationMetier;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TarifCotisationReadDto>>> GetAll(
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _db.TarifsCotisation
                .Include(c => c.TypeAdhesion)
                .Include(c => c.Devise)
                .AsQueryable();
            var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
            return Ok(new PaginatedResponse<TarifCotisationReadDto>
            {
                Data = result.Data.Select(ToReadDto).ToList(),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TarifCotisationReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(ToReadDto(item));
        }

        /// <summary>
        /// Calcule le montant total de cotisation (montant unitaire × nombre de personnes assurées).
        /// </summary>
        [HttpGet("{id:int}/montant-total")]
        public async Task<ActionResult<TarifCotisationMontantCalculDto>> CalculerMontantTotal(
            [FromRoute] int id,
            [FromQuery][Range(0, int.MaxValue)] int nombreDependants = 0,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _cotisationMetier.CalculerMontantTotalAsync(id, nombreDependants, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("type-adhesion/{typeAdhesionId:int}")]
        public async Task<ActionResult<List<TarifCotisationReadDto>>> GetByTypeAdhesion(
            [FromRoute] int typeAdhesionId,
            CancellationToken ct = default)
        {
            var typeExists = await _db.TypeAdhesions.AnyAsync(t => t.IdTypeAdhesion == typeAdhesionId, ct);
            if (!typeExists)
                return NotFound($"TypeAdhesion avec ID {typeAdhesionId} introuvable.");

            var items = await _repo.GetByTypeAdhesionIdAsync(typeAdhesionId, ct);
            return Ok(items.Select(ToReadDto).ToList());
        }

        /// <summary>
        /// Grille de cotisation applicable à un affilié (selon son adhésion active).
        /// L'identifiant affilié est fourni en paramètre de requête (<c>idAffilie</c>), pas via le JWT.
        /// </summary>
        [HttpGet("Affilie")]
        public async Task<ActionResult<List<TarifCotisationReadDto>>> GetByAffilie(
            [FromQuery] int idAffilie,
            CancellationToken ct = default)
        {
            if (idAffilie <= 0)
                return BadRequest("Le paramètre idAffilie est obligatoire et doit être > 0.");

            try
            {
                var items = await _repo.GetByAffilieIdAsync(idAffilie, ct);
                return Ok(items.Select(ToReadDto).ToList());
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des cotisations pour l'affilié",
                    ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TarifCotisationReadDto>> Create(
            [FromBody] TarifCotisationCreateDto input,
            CancellationToken ct = default)
        {
            try
            {
                var entity = new TarifCotisation
                {
                    Montant = input.Montant,
                    Periodicite = input.Periodicite,
                    TypeAdhesionId = input.TypeAdhesionId,
                    DeviseId = input.DeviseId,
                    LibelleTarifCotisation = input.LibelleTarifCotisation,
                    Statut = input.Statut
                };
                var created = await _repo.CreateAsync(entity, ct);
                var withNav = await _repo.GetByIdAsync(created.IdCotisationAffilie, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.IdCotisationAffilie }, ToReadDto(withNav!));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TarifCotisationReadDto>> Update(
            int id,
            [FromBody] TarifCotisationUpdateDto input,
            CancellationToken ct = default)
        {
            try
            {
                var entity = new TarifCotisation
                {
                    Montant = input.Montant,
                    Periodicite = input.Periodicite,
                    TypeAdhesionId = input.TypeAdhesionId,
                    DeviseId = input.DeviseId,
                    LibelleTarifCotisation = input.LibelleTarifCotisation,
                    Statut = input.Statut
                };
                var updated = await _repo.UpdateAsync(id, entity, ct);
                if (updated == null) return NotFound();
                var withNav = await _repo.GetByIdAsync(id, ct);
                return Ok(ToReadDto(withNav!));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        private static TarifCotisationReadDto ToReadDto(TarifCotisation entity)
        {
            return new TarifCotisationReadDto
            {
                Id = entity.IdCotisationAffilie,
                Montant = entity.Montant,
                Periodicite = entity.Periodicite,
                TypeAdhesionId = entity.TypeAdhesionId,
                TypeAdhesionLibelle = entity.TypeAdhesion?.Libelle,
                DeviseId = entity.DeviseId,
                LibelleTarifCotisation = entity.LibelleTarifCotisation,
                DeviseCode = entity.Devise?.Code,
                DeviseSymbole = entity.Devise?.Symbole,
                Statut = entity.Statut,
                DateCreation = entity.DateCreation,
                DateModification = entity.DateModification
            };
        }
    }
}
