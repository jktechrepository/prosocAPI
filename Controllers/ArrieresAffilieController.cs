using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/arrieres-affilie")]
    [Authorize]
    public class ArrieresAffilieController : BaseApiController
    {
        private readonly IArrieresAffilieService _arrieresService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<ArrieresAffilieController> _logger;

        public ArrieresAffilieController(
            IArrieresAffilieService arrieresService,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<ArrieresAffilieController> logger) : base(paginationService, paginationOptions, logger)
        {
            _arrieresService = arrieresService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<PaginatedResponse<ArrieresAffilie>>> GetAllArrieres(
            [FromQuery] PaginationRequest? pagination = null,
            [FromQuery] int? affilieId = null,
            [FromQuery] TypeCollecte? typeObligation = null,
            [FromQuery] int? mois = null,
            [FromQuery] int? annee = null,
            [FromQuery] string? statutPaiement = null,
            [FromQuery] bool? statut = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _db.ArrieresAffilie
                    .Include(a => a.Affilie)
                    .Include(a => a.Frais)
                    .Include(a => a.SouscriptionPrestation)!.ThenInclude(sp => sp!.Prestation)
                    .Include(a => a.CotisationAffilie)
                    .AsQueryable();

                if (affilieId.HasValue)
                    query = query.Where(a => a.AffilieId == affilieId.Value);

                if (typeObligation.HasValue)
                    query = query.Where(a => a.TypeObligation == typeObligation.Value);

                if (mois.HasValue)
                    query = query.Where(a => a.Mois == mois.Value);

                if (annee.HasValue)
                    query = query.Where(a => a.Annee == annee.Value);

                if (!string.IsNullOrEmpty(statutPaiement))
                    query = query.Where(a => a.StatutPaiement == statutPaiement);

                if (statut.HasValue)
                    query = query.Where(a => a.Statut == statut.Value);

                if (!string.IsNullOrEmpty(search))
                {
                    var searchTerm = search.ToLower();
                    query = query.Where(a =>
                        a.Description.ToLower().Contains(searchTerm) ||
                        a.StatutPaiement.ToLower().Contains(searchTerm) ||
                        a.Affilie.NomComplet.ToLower().Contains(searchTerm));
                }

                query = query.OrderByDescending(a => a.DateCreation);
                pagination = ValidatePaginationRequest(pagination ?? new PaginationRequest());
                var response = await _paginationService.CreatePaginatedResponseAsync(query, pagination);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des arriérés",
                    ex);
            }
        }

        [HttpGet("mes-arrieres")]
        public async Task<ActionResult<List<ArrieresAffilie>>> GetMesArrieres(CancellationToken ct = default)
        {
            try
            {
                var affilieId = await GetCurrentAffilieIdAsync(ct);
                if (affilieId == 0)
                    return Unauthorized("Utilisateur non authentifié ou non affilié");

                return Ok(await _arrieresService.GetArrieresByAffilieAsync(affilieId));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des arriérés de l'affilié connecté",
                    ex);
            }
        }

        [HttpGet("affilie/{affilieId}")]
        [HttpGet("by-affilie/{affilieId}")]
        public async Task<ActionResult<List<ArrieresAffilie>>> GetArrieresByAffilie(int affilieId)
        {
            try
            {
                return Ok(await _arrieresService.GetArrieresByAffilieAsync(affilieId));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des arriérés pour l'affilié ",
                    ex);
            }
        }

        [HttpGet("periode/{mois}/{annee}")]
        [HttpGet("by-periode/{mois}/{annee}")]
        public async Task<ActionResult<List<ArrieresAffilie>>> GetArrieresByPeriode(int mois, int annee)
        {
            try
            {
                return Ok(await _arrieresService.GetArrieresByPeriodeAsync(mois, annee));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des arriérés pour /",
                    ex);
            }
        }

        [HttpGet("by-statut/{statutPaiement}")]
        public async Task<ActionResult<List<ArrieresAffilie>>> GetArrieresByStatut(string statutPaiement)
        {
            try
            {
                return Ok(await _arrieresService.GetArrieresByStatutPaiementAsync(statutPaiement));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des arriérés avec statut ",
                    ex);
            }
        }

        [HttpGet("stats/{affilieId}")]
        public async Task<ActionResult<ArrieresStatsDto>> GetArrieresStatsByAffilie(int affilieId)
        {
            try
            {
                return Ok(await _arrieresService.GetArrieresStatsByAffilieAsync(affilieId));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur stats arriérés affilié ",
                    ex);
            }
        }

        [HttpPost("generate")]
        public async Task<ActionResult<List<ArrieresAffilie>>> GenerateArrieres([FromBody] GenerateArrieresDto dto)
        {
            try
            {
                var date = dto.Date ?? ArrieresAffilieRules.CalculerDateEcheance(
                    "Mensuel", dto.Mois, dto.Annee, 1);
                var arrieres = await _arrieresService.GenerateArrieresForDateAsync(date);
                return Ok(arrieres);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur génération arriérés",
                    ex);
            }
        }

        [HttpPost("generate-monthly")]
        public async Task<ActionResult<List<ArrieresAffilie>>> GenerateMonthlyArrieres([FromBody] GenerateArrieresDto dto)
        {
            try
            {
                return Ok(await _arrieresService.GenerateMonthlyArrieresAsync(dto.Mois, dto.Annee));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur génération arriérés mensuels /",
                    ex);
            }
        }

        [HttpPut("{id}/statut")]
        public async Task<ActionResult<ArrieresAffilie>> UpdateStatutArriere(int id, [FromBody] UpdateStatutArriereDto dto)
        {
            try
            {
                return Ok(await _arrieresService.UpdateStatutArriereAsync(id, dto.StatutPaiement));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur mise à jour statut arriéré ",
                    ex);
            }
        }

        [HttpGet("resume")]
        public async Task<ActionResult<ArrieresResumeDto>> GetArrieresResume()
        {
            try
            {
                return Ok(await _arrieresService.GetArrieresResumeAsync());
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur résumé arriérés",
                    ex);
            }
        }

        private Task<int> GetCurrentAffilieIdAsync(CancellationToken ct) =>
            CurrentUserAffilieResolver.ResolveAffilieIdAsync(User, _db, ct);
    }

    public class GenerateArrieresDto
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public DateTime? Date { get; set; }
    }

    public class UpdateStatutArriereDto
    {
        public string StatutPaiement { get; set; } = string.Empty;
    }
}
