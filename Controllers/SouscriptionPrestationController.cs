using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Models.Pagination;
using Prosoc.Utilities;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SouscriptionPrestationController : BaseApiController
    {
        private readonly ISouscriptionPrestationRepository _souscriptionRepository;
        private readonly ISouscriptionPrestationAchatService _achatService;
        private readonly IFlexPaySouscriptionAchatService _flexPaySouscriptionAchat;
        private readonly ProsocDbContext _db;

        public SouscriptionPrestationController(
            ISouscriptionPrestationRepository souscriptionRepository,
            ISouscriptionPrestationAchatService achatService,
            IFlexPaySouscriptionAchatService flexPaySouscriptionAchat,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<SouscriptionPrestationController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _souscriptionRepository = souscriptionRepository;
            _achatService = achatService;
            _flexPaySouscriptionAchat = flexPaySouscriptionAchat;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<SouscriptionPrestationReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.SouscriptionsPrestations
                    .Include(s => s.Affilie)
                    .Include(s => s.Prestation)
                    .Include(s => s.Collectes)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(s => new SouscriptionPrestationReadDto
                {
                    Id = s.IdSouscriptionPrestation,
                    AffilieId = s.AffilieId,
                    AffilieNom = s.Affilie?.Nom,
                    AffiliePrenom = s.Affilie?.Prenom,
                    PrestationId = s.PrestationId,
                    PrestationNom = s.Prestation?.NomPrestation,
                    PrestationDescription = s.Prestation?.Description,
                    DateSouscription = s.DateSouscription,
                    DateCreation = s.DateCreation,
                    DateModification = s.DateModification,
                    Statut = s.Statut,
                    NombreCollectes = s.Collectes?.Count ?? 0,
                    TotalCollectes = s.Collectes?.Sum(c => c.Montant) ?? 0
                }).ToList();

                var paginatedDtos = new PaginatedResponse<SouscriptionPrestationReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des souscriptions prestations paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SouscriptionPrestationReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var souscription = await _souscriptionRepository.GetByIdAsync(id, ct);
            if (souscription == null)
                return NotFound();

            var dto = new SouscriptionPrestationReadDto
            {
                Id = souscription.IdSouscriptionPrestation,
                AffilieId = souscription.AffilieId,
                AffilieNom = souscription.Affilie?.Nom,
                AffiliePrenom = souscription.Affilie?.Prenom,
                PrestationId = souscription.PrestationId,
                PrestationNom = souscription.Prestation?.NomPrestation,
                PrestationDescription = souscription.Prestation?.Description,
                DateSouscription = souscription.DateSouscription,
                DateCreation = souscription.DateCreation,
                DateModification = souscription.DateModification,
                Statut = souscription.Statut,
                NombreCollectes = souscription.Collectes?.Count ?? 0,
                TotalCollectes = souscription.Collectes?.Sum(c => c.Montant) ?? 0
            };
            return Ok(dto);
        }

        /// <summary>
        /// Récupère les souscriptions prestations d'un affilié (paginé).
        /// </summary>
        [HttpGet("by-affilie/{affilieId}")]
        public async Task<ActionResult<PaginatedResponse<SouscriptionPrestationReadDto>>> GetByAffilie(
            int affilieId,
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.SouscriptionsPrestations
                    .Include(s => s.Affilie)
                    .Include(s => s.Prestation)
                    .Include(s => s.Collectes)
                    .Where(s => s.AffilieId == affilieId)
                    .OrderByDescending(s => s.DateCreation)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(s => new SouscriptionPrestationReadDto
                {
                    Id = s.IdSouscriptionPrestation,
                    AffilieId = s.AffilieId,
                    AffilieNom = s.Affilie?.Nom,
                    AffiliePrenom = s.Affilie?.Prenom,
                    PrestationId = s.PrestationId,
                    PrestationNom = s.Prestation?.NomPrestation,
                    PrestationDescription = s.Prestation?.Description,
                    DateSouscription = s.DateSouscription,
                    DateCreation = s.DateCreation,
                    DateModification = s.DateModification,
                    Statut = s.Statut,
                    NombreCollectes = s.Collectes?.Count ?? 0,
                    TotalCollectes = s.Collectes?.Sum(c => c.Montant) ?? 0
                }).ToList();

                var paginatedDtos = new PaginatedResponse<SouscriptionPrestationReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des souscriptions pour l'affilié ",
                    ex);
            }
        }

        [HttpGet("by-affilie/{affilieId}/active")]
        public async Task<ActionResult<List<SouscriptionPrestationReadDto>>> GetByAffilieActives(int affilieId, CancellationToken ct = default)
        {
            var souscriptions = await _souscriptionRepository.GetByAffilieActivesAsync(affilieId, ct);
            var dtos = souscriptions.Select(s => new SouscriptionPrestationReadDto
            {
                Id = s.IdSouscriptionPrestation,
                AffilieId = s.AffilieId,
                AffilieNom = s.Affilie?.Nom,
                AffiliePrenom = s.Affilie?.Prenom,
                PrestationId = s.PrestationId,
                PrestationNom = s.Prestation?.NomPrestation,
                PrestationDescription = s.Prestation?.Description,
                DateSouscription = s.DateSouscription,
                DateCreation = s.DateCreation,
                DateModification = s.DateModification,
                Statut = s.Statut,
                NombreCollectes = s.Collectes?.Count ?? 0,
                TotalCollectes = s.Collectes?.Sum(c => c.Montant) ?? 0
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("by-prestation/{prestationId}")]
        public async Task<ActionResult<List<SouscriptionPrestationReadDto>>> GetByPrestation(int prestationId, CancellationToken ct = default)
        {
            var souscriptions = await _souscriptionRepository.GetByPrestationAsync(prestationId, ct);
            var dtos = souscriptions.Select(s => new SouscriptionPrestationReadDto
            {
                Id = s.IdSouscriptionPrestation,
                AffilieId = s.AffilieId,
                AffilieNom = s.Affilie?.Nom,
                AffiliePrenom = s.Affilie?.Prenom,
                PrestationId = s.PrestationId,
                PrestationNom = s.Prestation?.NomPrestation,
                PrestationDescription = s.Prestation?.Description,
                DateSouscription = s.DateSouscription,
                DateCreation = s.DateCreation,
                DateModification = s.DateModification,
                Statut = s.Statut,
                NombreCollectes = s.Collectes?.Count ?? 0,
                TotalCollectes = s.Collectes?.Sum(c => c.Montant) ?? 0
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<SouscriptionPrestationStatsDto>> GetStats(CancellationToken ct = default)
        {
            var stats = await _souscriptionRepository.GetStatsAsync(ct);
            return Ok(stats);
        }

        /// <summary>
        /// Crée une souscription prestation et enregistre le paiement de la première période en une transaction.
        /// </summary>
        /// <remarks>
        /// **Breaking change** : le body doit inclure un bloc <c>collecte</c> (montant, devise, modePaiement, mois, annee).
        /// Modes synchrones : VIRTUAL_ACCOUNT, ESPECE, CHEQUE, VIREMENT_BANCAIRE.
        /// FlexPay (MOBILE_MONEY / CARTE_BANCAIRE) : utiliser
        /// <c>POST /api/SouscriptionPrestation/paiement-electronique</c>.
        ///
        /// Exemple :
        /// ```json
        /// {
        ///   "prestationId": 26,
        ///   "dateSouscription": "2026-06-12T10:00:00Z",
        ///   "statut": true,
        ///   "collecte": {
        ///     "agentId": 3,
        ///     "montant": 5000,
        ///     "deviseId": 1,
        ///     "modePaiement": "VIRTUAL_ACCOUNT",
        ///     "mois": 3,
        ///     "annee": 2026,
        ///     "observation": "Première période"
        ///   }
        /// }
        /// ```
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<SouscriptionPrestationAchatReadDto>> Create(
            [FromBody] SouscriptionPrestationAchatCreateDto createDto,
            [FromQuery] int affilieId,
            CancellationToken ct = default)
        {
            try
            {
                var currentAffilieId = await CurrentUserAffilieResolver.ResolveAffilieIdAsync(User, _db, ct);
                if (currentAffilieId > 0 && currentAffilieId != affilieId)
                    return Forbid();

                var (souscription, collecte) = await _achatService.CreateWithCollecteAsync(
                    affilieId, createDto, ct);

                var reloadedSub = await _souscriptionRepository.GetByIdAsync(
                    souscription.IdSouscriptionPrestation, ct);

                var reloadedCollecte = await _db.Collectes
                    .AsNoTracking()
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.SouscriptionPrestationRef)
                        .ThenInclude(sp => sp!.Prestation)
                    .FirstAsync(c => c.IdCollecte == collecte.IdCollecte, ct);

                var response = new SouscriptionPrestationAchatReadDto
                {
                    Souscription = MapSouscriptionReadDto(reloadedSub ?? souscription, reloadedCollecte),
                    Collecte = MapCollecteReadDto(reloadedCollecte)
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = souscription.IdSouscriptionPrestation },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Initie l'achat d'une nouvelle prestation via FlexPay (Mobile Money / carte).
        /// La souscription et la collecte ne sont créées qu'au callback FlexPay (<c>code = 0</c>).
        /// </summary>
        [HttpPost("paiement-electronique")]
        [ProducesResponseType(typeof(InitiateFlexPayResponseDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<InitiateFlexPayResponseDto>> CreateWithPaiementElectronique(
            [FromBody] SouscriptionPrestationPaiementElectroniqueCreateDto input,
            CancellationToken ct = default)
        {
            try
            {
                var currentAffilieId = await CurrentUserAffilieResolver.ResolveAffilieIdAsync(User, _db, ct);
                if (currentAffilieId > 0)
                {
                    if (input.AffilieId > 0 && input.AffilieId != currentAffilieId)
                        return Forbid();
                    input.AffilieId = currentAffilieId;
                }

                if (input.AffilieId <= 0)
                    return BadRequest("affilieId est obligatoire.");

                var result = await _flexPaySouscriptionAchat.InitiateAsync(input, ct);
                return Accepted(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static SouscriptionPrestationReadDto MapSouscriptionReadDto(
            SouscriptionPrestation s,
            Collecte? premiereCollecte = null) => new()
        {
            Id = s.IdSouscriptionPrestation,
            AffilieId = s.AffilieId,
            AffilieNom = s.Affilie?.Nom,
            AffiliePrenom = s.Affilie?.Prenom,
            PrestationId = s.PrestationId,
            PrestationNom = s.Prestation?.NomPrestation,
            PrestationDescription = s.Prestation?.Description,
            DateSouscription = s.DateSouscription,
            DateCreation = s.DateCreation,
            DateModification = s.DateModification,
            Statut = s.Statut,
            NombreCollectes = s.Collectes is { Count: > 0 } collectes
                ? collectes.Count
                : (premiereCollecte != null ? 1 : 0),
            TotalCollectes = s.Collectes is { Count: > 0 } collectesAvecMontant
                ? collectesAvecMontant.Sum(c => c.Montant)
                : premiereCollecte?.Montant ?? 0
        };

        private static CollecteReadDto MapCollecteReadDto(Collecte c) => new()
        {
            IdCollecte = c.IdCollecte,
            TypeCollecte = c.TypeCollecte,
            AffilieId = c.AffilieId,
            AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
            AgentId = c.AgentId,
            AgentNom = c.Agent?.NomComplet,
            Montant = c.Montant,
            ReferencePaiement = c.ReferencePaiement,
            ModePaiement = c.ModePaiement,
            Operateur = c.Operateur,
            StatutPaiement = c.StatutPaiement,
            SouscriptionPrestationId = c.SouscriptionPrestationId,
            PrestationLibelle = c.SouscriptionPrestationRef?.Prestation?.NomPrestation,
            MontantRecu = c.MontantRecu,
            MontantAttendu = c.MontantAttendu,
            DeviseId = c.DeviseId,
            DeviseNom = c.Devise?.Nom,
            DeviseCode = c.Devise?.Code,
            Mois = c.Mois,
            Annee = c.Annee,
            DateCollecte = c.DateCollecte,
            Observation = c.Observation,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification,
            Statut = c.Statut
        };

        [HttpPut("{id}")]
        public async Task<ActionResult<SouscriptionPrestationReadDto>> Update(int id, [FromBody] SouscriptionPrestationUpdateDto updateDto, CancellationToken ct = default)
        {
            var souscription = new SouscriptionPrestation
            {
                AffilieId = updateDto.AffilieId,
                PrestationId = updateDto.PrestationId,
                Statut = updateDto.Statut
            };

            var updated = await _souscriptionRepository.UpdateAsync(id, souscription, ct);
            if (updated == null)
                return NotFound();

            var dto = new SouscriptionPrestationReadDto
            {
                Id = updated.IdSouscriptionPrestation,
                AffilieId = updated.AffilieId,
                AffilieNom = updated.Affilie?.Nom,
                AffiliePrenom = updated.Affilie?.Prenom,
                PrestationId = updated.PrestationId,
                PrestationNom = updated.Prestation?.NomPrestation,
                PrestationDescription = updated.Prestation?.Description,
                DateSouscription = updated.DateSouscription,
                DateCreation = updated.DateCreation,
                DateModification = updated.DateModification,
                Statut = updated.Statut,
                NombreCollectes = updated.Collectes?.Count ?? 0,
                TotalCollectes = updated.Collectes?.Sum(c => c.Montant) ?? 0
            };
            
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _souscriptionRepository.DeleteAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }
    }
}
