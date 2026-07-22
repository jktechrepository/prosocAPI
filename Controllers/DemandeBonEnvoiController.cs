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
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DemandeBonEnvoiController : BaseApiController
    {
        private readonly IDemandeBonEnvoiRepository _demandeBonEnvoiRepository;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DemandeBonEnvoiController> _logger;
        private readonly DemandeBonEnvoiService _demandeBonEnvoiService;

        public DemandeBonEnvoiController(
            IDemandeBonEnvoiRepository demandeBonEnvoiRepository,
            ProsocDbContext db,
            ILogger<DemandeBonEnvoiController> logger,
            DemandeBonEnvoiService demandeBonEnvoiService,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions)
            : base(paginationService, paginationOptions, logger)
        {
            _demandeBonEnvoiRepository = demandeBonEnvoiRepository;
            _db = db;
            _demandeBonEnvoiService = demandeBonEnvoiService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère toutes les demandes de bon d'envoi
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<DemandeBonEnvoiReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.DemandesBonEnvoi
                    .Include(d => d.Affilie)
                    .Include(d => d.Prestation)
                    .Include(d => d.Agent)
                    .Include(d => d.JetonMedical)
                    .AsQueryable();

                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct: default);
                    if (hopitalId <= 0)
                        return Forbid();

                    query = query.Where(d =>
                        d.JetonMedicalId != null
                        && d.JetonMedical != null
                        && d.JetonMedical.HopitalPartenaireId == hopitalId);
                }

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(d => new DemandeBonEnvoiReadDto
                {
                    IdDemande = d.IdDemande,
                    AffilieId = d.AffilieId,
                    AffilieNom = d.Affilie?.NomComplet,
                    PrestationId = d.PrestationId,
                    PrestationNom = d.Prestation?.NomPrestation,
                    MotifDemande = d.MotifDemande,
                    AgentId = d.AgentId,
                    AgentNom = d.Agent?.NomComplet,
                    ObservationAgent = d.ObservationAgent,
                    DateDemande = d.DateDemande,
                DateValidation = d.DateValidation,
                StatutDemande = d.StatutDemande,
                BonEnvoiId = d.BonEnvoiId,
                BonEnvoiNumero = d.BonEnvoi?.NumeroBon,
                JetonMedicalId = d.JetonMedicalId,
                JetonMedicalCode = d.JetonMedical?.CodeJeton,
                }).ToList();

                var paginatedDtos = new PaginatedResponse<DemandeBonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des demandes de bon d'envoi paginées",
                    ex);
            }
        }
        /// <summary>
        /// Récupère une demande par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DemandeBonEnvoiReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var demande = await _demandeBonEnvoiRepository.GetByIdAsync(id, ct);
            if (demande == null)
                return NotFound();

            if (CurrentUserHopitalResolver.IsAgentHopital(User))
            {
                var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                if (hopitalId <= 0
                    || demande.JetonMedical?.HopitalPartenaireId != hopitalId)
                    return Forbid();
            }

            var dto = new DemandeBonEnvoiReadDto
            {
                IdDemande = demande.IdDemande,
                AffilieId = demande.AffilieId,
                AffilieNom = demande.Affilie?.NomComplet,
                PrestationId = demande.PrestationId,
                PrestationNom = demande.Prestation?.NomPrestation,
                MotifDemande = demande.MotifDemande,
                AgentId = demande.AgentId,
                AgentNom = demande.Agent?.NomComplet,
                ObservationAgent = demande.ObservationAgent,
                DateDemande = demande.DateDemande,
                DateValidation = demande.DateValidation,
                StatutDemande = demande.StatutDemande,
                BonEnvoiId = demande.BonEnvoiId,
                BonEnvoiNumero = demande.BonEnvoi?.NumeroBon,
                JetonMedicalId = demande.JetonMedicalId,
                JetonMedicalCode = demande.JetonMedical?.CodeJeton,
                QrCodePayload = demande.BonEnvoi?.QrCodePayload,
                QrCodeImageBase64 = demande.BonEnvoi?.QrCodeImageBase64,
                DateCreation = demande.DateCreation,
                DateModification = demande.DateModification,
                Statut = demande.Statut
            };

            return Ok(dto);
        }

        /// <summary>
        /// Confirmation agent en un clic : valide (bon + jeton + QR) ou rejette la demande.
        /// </summary>
        [HttpPost("{id}/confirmer")]
        public async Task<ActionResult<DemandeBonEnvoiConfirmationResultDto>> Confirmer(
            int id,
            [FromBody] DemandeBonEnvoiConfirmerDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var resultat = await _demandeBonEnvoiService.ConfirmerDemandeAsync(id, dto, ct);
                if (!resultat.Succes && resultat.StatutDemande != "REJETEE")
                    return BadRequest(resultat);

                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur confirmation demande {DemandeId}", id);
                return this.TechnicalErrorResponse("Erreur serveur lors de la confirmation", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes d'un affilié (paginé).
        /// </summary>
        [HttpGet("by-affilie/{affilieId}")]
        public async Task<ActionResult<PaginatedResponse<DemandeBonEnvoiReadDto>>> GetByAffilie(
            int affilieId,
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.DemandesBonEnvoi
                    .Include(d => d.Affilie)
                    .Include(d => d.Prestation)
                    .Include(d => d.Agent)
                    .Include(d => d.BonEnvoi)
                    .Include(d => d.JetonMedical)
                    .Where(d => d.AffilieId == affilieId)
                    .OrderByDescending(d => d.DateDemande)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(d => new DemandeBonEnvoiReadDto
                {
                    IdDemande = d.IdDemande,
                    AffilieId = d.AffilieId,
                    AffilieNom = d.Affilie?.NomComplet,
                    PrestationId = d.PrestationId,
                    PrestationNom = d.Prestation?.NomPrestation,
                    MotifDemande = d.MotifDemande,
                    AgentId = d.AgentId,
                    AgentNom = d.Agent?.NomComplet,
                    ObservationAgent = d.ObservationAgent,
                    DateDemande = d.DateDemande,
                    DateValidation = d.DateValidation,
                    StatutDemande = d.StatutDemande,
                    BonEnvoiId = d.BonEnvoiId,
                    BonEnvoiNumero = d.BonEnvoi?.NumeroBon,
                    JetonMedicalId = d.JetonMedicalId,
                    JetonMedicalCode = d.JetonMedical?.CodeJeton,
                    DateCreation = d.DateCreation,
                    DateModification = d.DateModification,
                    Statut = d.Statut
                }).ToList();

                var paginatedDtos = new PaginatedResponse<DemandeBonEnvoiReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des demandes pour l'affilié ",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les demandes par statut
        /// </summary>
        [HttpGet("by-statut/{statut}/simple")]
        public async Task<ActionResult<List<DemandeBonEnvoiReadDto>>> GetByStatut(string statut, CancellationToken ct = default)
        {
            var demandes = await _demandeBonEnvoiRepository.GetByStatutAsync(statut, ct);
            var dtos = demandes.Select(d => new DemandeBonEnvoiReadDto
            {
                IdDemande = d.IdDemande,
                AffilieId = d.AffilieId,
                AffilieNom = d.Affilie?.NomComplet,
                PrestationId = d.PrestationId,
                PrestationNom = d.Prestation?.NomPrestation,
                MotifDemande = d.MotifDemande,
                AgentId = d.AgentId,
                AgentNom = d.Agent?.NomComplet,
                ObservationAgent = d.ObservationAgent,
                DateDemande = d.DateDemande,
                DateValidation = d.DateValidation,
                StatutDemande = d.StatutDemande,
                BonEnvoiId = d.BonEnvoiId,
                BonEnvoiNumero = d.BonEnvoi?.NumeroBon,
                JetonMedicalId = d.JetonMedicalId,
                JetonMedicalCode = d.JetonMedical?.CodeJeton,
                DateCreation = d.DateCreation,
                DateModification = d.DateModification,
                Statut = d.Statut
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Crée une nouvelle demande de bon d'envoi
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DemandeBonEnvoiReadDto>> Create([FromBody] DemandeBonEnvoiCreateDto createDto, CancellationToken ct = default)
        {
            try
            {
                var demande = new DemandeBonEnvoi
                {
                    AffilieId = createDto.AffilieId,
                    PrestationId = createDto.PrestationId,
                    MotifDemande = createDto.MotifDemande,
                    AgentId = createDto.AgentId,
                    ObservationAgent = createDto.ObservationAgent,
                    StatutDemande = "EN_ATTENTE",
                    DateCreation = DateTime.Now
                };

                var created = await _demandeBonEnvoiRepository.CreateAsync(demande, ct);
                
                var responseDto = new DemandeBonEnvoiReadDto
                {
                    IdDemande = created.IdDemande,
                    AffilieId = created.AffilieId,
                    AffilieNom = created.Affilie?.NomComplet,
                    PrestationId = created.PrestationId,
                    PrestationNom = created.Prestation?.NomPrestation,
                    MotifDemande = created.MotifDemande,
                    AgentId = created.AgentId,
                    AgentNom = created.Agent?.NomComplet,
                    ObservationAgent = created.ObservationAgent,
                    DateDemande = created.DateDemande,
                    DateValidation = created.DateValidation,
                    StatutDemande = created.StatutDemande,
                    BonEnvoiId = created.BonEnvoiId,
                    BonEnvoiNumero = created.BonEnvoi?.NumeroBon,
                    JetonMedicalId = created.JetonMedicalId,
                    JetonMedicalCode = created.JetonMedical?.CodeJeton,
                    DateCreation = created.DateCreation,
                    DateModification = created.DateModification,
                    Statut = created.Statut
                };

                return CreatedAtAction(nameof(GetById), new { id = created.IdDemande }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la demande de bon d'envoi");
                return this.TechnicalErrorResponse("Erreur serveur lors de la création", ex);
            }
        }

        /// <summary>
        /// Vérifie l'éligibilité d'un affilié
        /// </summary>
        [HttpGet("verifier-eligibilite/{affilieId}")]
        public async Task<ActionResult> VerifierEligibilite(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var verification = await _demandeBonEnvoiService.VerifierEligibiliteAsync(affilieId, ct);
                return Ok(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification d'éligibilité pour l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur serveur lors de la vérification", ex);
            }
        }

        /// <summary>
        /// Valide une demande et génère le bon d'envoi + jeton médical
        /// </summary>
        [HttpPost("valider-et-generer")]
        public async Task<ActionResult> ValiderEtGenerer([FromBody] DemandeBonEnvoiGenerationDto generationDto, CancellationToken ct = default)
        {
            try
            {
                if (generationDto.AgentId <= 0)
                    return BadRequest(new { success = false, message = "agentId est obligatoire." });

                var resultat = await _demandeBonEnvoiService.ValiderEtGenererAsync(
                    generationDto.IdDemande,
                    generationDto.AgentId,
                    generationDto.HopitalPartenaireId,
                    ct);

                if (resultat.Succes)
                {
                    return Ok(new {
                        success = true,
                        message = resultat.Message,
                        bonEnvoiId = resultat.BonEnvoiId,
                        bonEnvoiNumero = resultat.BonEnvoiNumero,
                        jetonMedicalId = resultat.JetonMedicalId,
                        jetonMedicalCode = resultat.JetonMedicalCode
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = resultat.Message 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation et génération pour la demande {DemandeId}", generationDto.IdDemande);
                return this.TechnicalErrorResponse("Erreur serveur lors de la génération", ex);
            }
        }

        /// <summary>
        /// Récupère les statistiques des demandes
        /// </summary>
        [HttpGet("stats/{date:datetime}")]
        public async Task<ActionResult<DemandeBonEnvoiStatsDto>> GetStats(DateTime date, CancellationToken ct = default)
        {
            try
            {
                var stats = await _demandeBonEnvoiService.GetStatsAsync(date, ct);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques pour {Date}", date);
                return this.TechnicalErrorResponse("Erreur serveur lors de la récupération des statistiques", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes en attente
        /// </summary>
        [HttpGet("en-attente")]
        public async Task<ActionResult<List<DemandeBonEnvoiReadDto>>> GetEnAttente(CancellationToken ct = default)
        {
            return await GetByStatut("EN_ATTENTE", ct);
        }

        /// <summary>
        /// Récupère les demandes validées
        /// </summary>
        [HttpGet("validees")]
        public async Task<ActionResult<List<DemandeBonEnvoiReadDto>>> GetValidees(CancellationToken ct = default)
        {
            return await GetByStatut("VALIDEE", ct);
        }

        /// <summary>
        /// Récupère les demandes rejetées
        /// </summary>
        [HttpGet("rejetees")]
        public async Task<ActionResult<List<DemandeBonEnvoiReadDto>>> GetRejetees(CancellationToken ct = default)
        {
            return await GetByStatut("REJETEE", ct);
        }
    }
}
