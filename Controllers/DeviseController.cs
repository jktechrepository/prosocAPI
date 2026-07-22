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
    public class DeviseController : BaseApiController
    {
        private readonly IDeviseRepository _deviseRepository;
        private readonly ProsocDbContext _db;

        public DeviseController(
            IDeviseRepository deviseRepository,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<DeviseController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _deviseRepository = deviseRepository;
            _db = db;
        }

        private static DeviseReadDto MapDevise(Devise d) => new()
        {
            IdDevise = d.IdDevise,
            Code = d.Code,
            Nom = d.Nom,
            Symbole = d.Symbole,
            EstDevisePrincipale = d.EstDevisePrincipale,
            Statut = d.Statut
        };

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponse<DeviseReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Devises.AsQueryable();
                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
                var dtos = result.Data.Select(MapDevise).ToList();

                return Ok(new PaginatedResponse<DeviseReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des devises paginées",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<DeviseReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var devise = await _deviseRepository.GetByIdAsync(id, ct);
            if (devise == null)
                return NotFound();
            return Ok(MapDevise(devise));
        }

        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<DeviseReadDto>> GetByCode(string code, CancellationToken ct = default)
        {
            var devise = await _deviseRepository.GetByCodeAsync(code, ct);
            if (devise == null)
                return NotFound();
            return Ok(MapDevise(devise));
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<DeviseReadDto>>> GetActives(CancellationToken ct = default)
        {
            var devises = await _deviseRepository.GetActivesAsync(ct);
            return Ok(devises.Select(MapDevise).ToList());
        }

        [HttpGet("preview-conversion")]
        public async Task<ActionResult<PreviewConversionDto>> PreviewConversion(
            [FromQuery] string codeDeviseSource,
            [FromQuery] decimal montant,
            [FromQuery] string? codeDeviseCible,
            [FromQuery] DateTime? datePaiement,
            [FromServices] IDeviseConversionService conversionService,
            CancellationToken ct = default)
        {
            try
            {
                var date = datePaiement ?? DateTime.UtcNow;
                var result = await conversionService.PreviewConversionAsync(
                    codeDeviseSource, montant, codeDeviseCible, date, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("taux-change")]
        public async Task<ActionResult<TauxChangeDeviseReadDto>> GetTauxChange(
            [FromQuery] string source,
            [FromQuery] string cible,
            [FromQuery] DateTime? dateReference,
            [FromServices] IDeviseConversionService conversionService,
            CancellationToken ct = default)
        {
            try
            {
                var deviseSource = await _db.Devises.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Code == source.ToUpperInvariant() && d.Statut, ct);
                var deviseCible = await _db.Devises.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Code == cible.ToUpperInvariant() && d.Statut, ct);

                if (deviseSource == null || deviseCible == null)
                    return NotFound("Devise source ou cible introuvable.");

                var date = dateReference ?? DateTime.UtcNow;
                var taux = await conversionService.GetTauxActifAsync(
                    deviseSource.IdDevise, deviseCible.IdDevise, date, ct);

                if (taux != null)
                {
                    return Ok(new TauxChangeDeviseReadDto
                    {
                        IdTauxChangeDevise = taux.IdTauxChangeDevise,
                        DeviseSourceId = taux.DeviseSourceId,
                        CodeDeviseSource = deviseSource.Code,
                        DeviseCibleId = taux.DeviseCibleId,
                        CodeDeviseCible = deviseCible.Code,
                        Taux = taux.Taux,
                        DateEffet = taux.DateEffet,
                        Statut = taux.Statut
                    });
                }

                var (_, tauxCalcule) = await conversionService.ConvertirAsync(
                    1m, deviseSource.IdDevise, deviseCible.IdDevise, date, ct);

                return Ok(new TauxChangeDeviseReadDto
                {
                    DeviseSourceId = deviseSource.IdDevise,
                    CodeDeviseSource = deviseSource.Code,
                    DeviseCibleId = deviseCible.IdDevise,
                    CodeDeviseCible = deviseCible.Code,
                    Taux = tauxCalcule,
                    DateEffet = date,
                    Statut = true
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("taux-change/historique")]
        public async Task<ActionResult<List<TauxChangeDeviseReadDto>>> GetHistoriqueTaux(
            [FromQuery] string? source,
            [FromQuery] string? cible,
            CancellationToken ct = default)
        {
            var query = _db.TauxChangeDevises
                .AsNoTracking()
                .Include(t => t.DeviseSource)
                .Include(t => t.DeviseCible)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(source))
                query = query.Where(t => t.DeviseSource.Code == source.ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(cible))
                query = query.Where(t => t.DeviseCible.Code == cible.ToUpperInvariant());

            var list = await query
                .OrderByDescending(t => t.DateEffet)
                .Take(100)
                .Select(t => new TauxChangeDeviseReadDto
                {
                    IdTauxChangeDevise = t.IdTauxChangeDevise,
                    DeviseSourceId = t.DeviseSourceId,
                    CodeDeviseSource = t.DeviseSource.Code,
                    DeviseCibleId = t.DeviseCibleId,
                    CodeDeviseCible = t.DeviseCible.Code,
                    Taux = t.Taux,
                    DateEffet = t.DateEffet,
                    Statut = t.Statut
                })
                .ToListAsync(ct);

            return Ok(list);
        }

        [HttpPost("taux-change")]
        public async Task<ActionResult<TauxChangeDeviseReadDto>> CreateTauxChange(
            [FromBody] TauxChangeDeviseCreateDto dto,
            CancellationToken ct = default)
        {
            var codeSource = dto.CodeDeviseSource.ToUpperInvariant();
            var codeCible = dto.CodeDeviseCible.ToUpperInvariant();

            if (codeSource == codeCible)
                return BadRequest("La devise source et la devise cible doivent être différentes.");

            var deviseSource = await _db.Devises.FirstOrDefaultAsync(d => d.Code == codeSource && d.Statut, ct);
            var deviseCible = await _db.Devises.FirstOrDefaultAsync(d => d.Code == codeCible && d.Statut, ct);

            if (deviseSource == null || deviseCible == null)
                return BadRequest("Devise source ou cible introuvable ou inactive.");

            var taux = new TauxChangeDevise
            {
                DeviseSourceId = deviseSource.IdDevise,
                DeviseCibleId = deviseCible.IdDevise,
                Taux = dto.Taux,
                DateEffet = dto.DateEffet ?? DateTime.UtcNow,
                Statut = dto.Statut
            };

            _db.TauxChangeDevises.Add(taux);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetTauxChange), new { source = codeSource, cible = codeCible },
                new TauxChangeDeviseReadDto
                {
                    IdTauxChangeDevise = taux.IdTauxChangeDevise,
                    DeviseSourceId = taux.DeviseSourceId,
                    CodeDeviseSource = deviseSource.Code,
                    DeviseCibleId = taux.DeviseCibleId,
                    CodeDeviseCible = deviseCible.Code,
                    Taux = taux.Taux,
                    DateEffet = taux.DateEffet,
                    Statut = taux.Statut
                });
        }

        [HttpPost]
        public async Task<ActionResult<DeviseReadDto>> Create([FromBody] DeviseCreateDto createDto, CancellationToken ct = default)
        {
            try
            {
                var devise = new Devise
                {
                    Code = createDto.Code,
                    Nom = createDto.Nom,
                    Symbole = createDto.Symbole,
                    EstDevisePrincipale = createDto.EstDevisePrincipale,
                    Statut = createDto.Statut
                };

                var created = await _deviseRepository.CreateAsync(devise, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.IdDevise }, MapDevise(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<DeviseReadDto>> Update(int id, [FromBody] DeviseUpdateDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var devise = new Devise
                {
                    Code = updateDto.Code,
                    Nom = updateDto.Nom,
                    Symbole = updateDto.Symbole,
                    EstDevisePrincipale = updateDto.EstDevisePrincipale,
                    Statut = updateDto.Statut
                };

                var updated = await _deviseRepository.UpdateAsync(id, devise, ct);
                if (updated == null)
                    return NotFound();

                return Ok(MapDevise(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var success = await _deviseRepository.DeleteAsync(id, ct);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
