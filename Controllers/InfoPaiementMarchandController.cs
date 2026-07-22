using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin,IT,Financier")]
    public class InfoPaiementMarchandController : ControllerBase
    {
        private readonly IInfoPaiementMarchandService _service;
        private readonly ILogger<InfoPaiementMarchandController> _logger;

        public InfoPaiementMarchandController(
            IInfoPaiementMarchandService service,
            ILogger<InfoPaiementMarchandController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private static InfoPaiementMarchandReadDto Map(InfoPaiementMarchand e) => new()
        {
            IdInfoPaiementMarchand = e.IdInfoPaiementMarchand,
            CodeMarchand = e.CodeMarchand,
            ApiTokenMasked = FlexPayTokenMaskHelper.Mask(e.ApiToken),
            ActifMobileMoney = e.ActifMobileMoney,
            ActifCarteBancaire = e.ActifCarteBancaire,
            Statut = e.Statut,
            DateCreation = e.DateCreation,
            DateModification = e.DateModification
        };

        [HttpGet("actif")]
        public async Task<ActionResult<InfoPaiementMarchandReadDto>> GetActif(CancellationToken ct = default)
        {
            var entity = await _service.GetActifAsync(ct);
            if (entity == null)
                return NotFound("Aucune configuration marchand active.");
            return Ok(Map(entity));
        }

        [HttpGet]
        public async Task<ActionResult<List<InfoPaiementMarchandReadDto>>> GetAll(CancellationToken ct = default)
        {
            var list = await _service.GetAllAsync(ct);
            return Ok(list.Select(Map).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InfoPaiementMarchandReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var entity = await _service.GetByIdAsync(id, ct);
            if (entity == null)
                return NotFound();
            return Ok(Map(entity));
        }

        [HttpPost]
        public async Task<ActionResult<InfoPaiementMarchandReadDto>> Create(
            [FromBody] InfoPaiementMarchandCreateDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var entity = new InfoPaiementMarchand
                {
                    CodeMarchand = dto.CodeMarchand.Trim(),
                    ApiToken = dto.ApiToken.Trim(),
                    ActifMobileMoney = dto.ActifMobileMoney,
                    ActifCarteBancaire = dto.ActifCarteBancaire,
                    Statut = dto.Statut
                };
                var created = await _service.CreateAsync(entity, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.IdInfoPaiementMarchand }, Map(created));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur création configuration marchand FlexPay",
                    ex);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<InfoPaiementMarchandReadDto>> Update(
            int id,
            [FromBody] InfoPaiementMarchandUpdateDto dto,
            CancellationToken ct = default)
        {
            var existing = await _service.GetByIdAsync(id, ct);
            if (existing == null)
                return NotFound();

            var entity = new InfoPaiementMarchand
            {
                CodeMarchand = dto.CodeMarchand ?? existing.CodeMarchand,
                ApiToken = dto.ApiToken ?? existing.ApiToken,
                ActifMobileMoney = dto.ActifMobileMoney ?? existing.ActifMobileMoney,
                ActifCarteBancaire = dto.ActifCarteBancaire ?? existing.ActifCarteBancaire,
                Statut = dto.Statut ?? existing.Statut
            };

            var updated = await _service.UpdateAsync(id, entity, ct);
            if (updated == null)
                return NotFound();
            return Ok(Map(updated));
        }
    }
}
