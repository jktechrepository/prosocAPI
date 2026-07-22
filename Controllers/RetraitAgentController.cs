using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RetraitAgentController : BaseApiController
    {
        private readonly IDemandeRetraitAgentRepository _retraitAgentRepository;
        private readonly ProsocDbContext _db;
        private readonly RetraitAgentService _retraitAgentService;
        private readonly IDeviseConversionService _deviseConversionService;
        private readonly ILogger<RetraitAgentController> _logger;

        public RetraitAgentController(
            IDemandeRetraitAgentRepository retraitAgentRepository, 
            ProsocDbContext db,
            ILogger<RetraitAgentController> logger,
            RetraitAgentService retraitAgentService,
            IDeviseConversionService deviseConversionService,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions)
            : base(paginationService, paginationOptions, logger)
        {
            _retraitAgentRepository = retraitAgentRepository;
            _db = db;
            _retraitAgentService = retraitAgentService;
            _deviseConversionService = deviseConversionService;
            _logger = logger;
        }

        private static DemandeRetraitAgentReadDto MapToReadDto(DemandeRetraitAgent demande, Devise devisePrincipale)
        {
            return new DemandeRetraitAgentReadDto
            {
                IdDemande = demande.IdDemande,
                AgentId = demande.AgentId,
                AgentNom = demande.Agent?.NomComplet,
                AgentMatricule = demande.Agent?.Matricule,
                MontantDemande = demande.MontantDemande,
                TypeRetrait = demande.TypeRetrait,
                StatutDemande = demande.StatutDemande,
                MotifRetrait = demande.MotifRetrait,
                MotifRejet = demande.MotifRejet,
                DeviseId = devisePrincipale.IdDevise,
                DeviseCode = devisePrincipale.Code,
                DeviseSymbole = devisePrincipale.Symbole,
                DateDemande = demande.DateDemande,
                DateValidation = demande.DateValidation,
                DateTraitement = demande.DateTraitement,
                AgentValidationId = demande.AgentValidationId,
                AgentValidationNom = demande.AgentValidation?.NomComplet,
                JetonRetraitId = demande.JetonRetraitId,
                JetonRetraitCode = demande.JetonRetrait?.CodeJeton,
                DateCreation = demande.DateCreation,
                DateModification = demande.DateModification,
                Statut = demande.Statut
            };
        }

        /// <summary>
        /// Récupère toutes les demandes de retrait
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<DemandeRetraitAgentReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.DemandesRetraitAgents
                    .Include(d => d.Agent)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync();
                var dtos = result.Data.Select(d => MapToReadDto(d, devisePrincipale)).ToList();

                var paginatedDtos = new PaginatedResponse<DemandeRetraitAgentReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des demandes de retrait paginées",
                    ex);
            }
        }

        /// <summary>
        /// Récupère une demande de retrait par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DemandeRetraitAgentReadDto>> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                var demande = await _retraitAgentRepository.GetByIdAsync(id, ct);
                if (demande == null)
                {
                    return NotFound(new { error = "Demande de retrait non trouvée" });
                }

                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dto = MapToReadDto(demande, devisePrincipale);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la demande de retrait {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes de retrait d'un agent
        /// </summary>
        [HttpGet("by-agent/{agentId}")]
        public async Task<ActionResult<List<DemandeRetraitAgentReadDto>>> GetByAgent(int agentId, CancellationToken ct = default)
        {
            try
            {
                var demandes = await _retraitAgentRepository.GetByAgentIdAsync(agentId, ct);
                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dtos = demandes.Select(d => MapToReadDto(d, devisePrincipale)).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes de retrait pour l'agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes de retrait par statut
        /// </summary>
        [HttpGet("by-statut/{statut}")]
        public async Task<ActionResult<List<DemandeRetraitAgentReadDto>>> GetByStatut(string statut, CancellationToken ct = default)
        {
            try
            {
                var demandes = await _retraitAgentRepository.GetByStatutAsync(statut, ct);
                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dtos = demandes.Select(d => MapToReadDto(d, devisePrincipale)).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes de retrait avec statut {Statut}", statut);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Crée une nouvelle demande de retrait.
        /// Fenêtre 1 (15–20) : PARTIEL, montantDemande obligatoire.
        /// Fenêtre 2 (fin de mois) : TOTAL, montant = soldeDisponible (montantDemande ignoré).
        /// Le typeRetrait envoyé par le client est ignoré s'il ne correspond pas à la fenêtre courante.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RetraitWorkflowResultDto>> Create([FromBody] DemandeRetraitAgentCreateDto createDto, CancellationToken ct = default)
        {
            try
            {
                var resultat = await _retraitAgentService.CreerDemandeRetraitAsync(createDto, ct);

                if (resultat.Succes)
                {
                    return Ok(resultat);
                }
                else
                {
                    return BadRequest(resultat);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la demande de retrait");
                return StatusCode(500, new RetraitWorkflowResultDto
                {
                    Succes = false,
                    Message = "Erreur technique lors de la création de la demande"
                });
            }
        }

        /// <summary>
        /// Vérifie si la période de retrait est autorisée
        /// </summary>
        [HttpPost("verifier-periode")]
        public async Task<ActionResult<PeriodeRetraitVerificationDto>> VerifierPeriodeRetrait([FromBody] DateTime date, CancellationToken ct = default)
        {
            try
            {
                var verification = await _retraitAgentService.VerifierPeriodeRetraitAsync(date, ct);
                return Ok(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la période de retrait");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Retourne la période de retrait courante et les fenêtres du mois en cours
        /// </summary>
        [HttpGet("periode-courante")]
        public async Task<ActionResult<PeriodeRetraitCouranteDto>> GetPeriodeCourante(CancellationToken ct = default)
        {
            try
            {
                return Ok(await _retraitAgentService.GetPeriodeCouranteAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la période de retrait courante");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Vérifie si le solde est suffisant pour le retrait
        /// </summary>
        [HttpPost("verifier-solde")]
        public async Task<ActionResult<SoldeVerificationDto>> VerifierSoldeDisponible([FromBody] SoldeVerificationDto verificationDto, CancellationToken ct = default)
        {
            try
            {
                var verification = await _retraitAgentService.VerifierSoldeDisponible(verificationDto.AgentId, verificationDto.MontantDemande, ct);
                return Ok(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du solde pour l'agent {AgentId}", verificationDto.AgentId);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Valide une demande de retrait et génère le jeton
        /// </summary>
        [HttpPost("valider-et-generer-jeton")]
        public async Task<ActionResult<RetraitWorkflowResultDto>> ValiderEtGenererJeton([FromBody] DemandeRetraitAgentValidationDto validationDto, CancellationToken ct = default)
        {
            try
            {
                var resultat = await _retraitAgentService.ValiderEtGenererJetonAsync(
                    validationDto.IdDemande, 
                    validationDto.AgentValidationId, 
                    ct);

                if (resultat.Succes)
                {
                    return Ok(resultat);
                }
                else
                {
                    return BadRequest(resultat);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la demande {DemandeId}", validationDto.IdDemande);
                return StatusCode(500, new RetraitWorkflowResultDto
                {
                    Succes = false,
                    Message = "Erreur technique lors de la validation"
                });
            }
        }

        /// <summary>
        /// Utilise un jeton de retrait (paiement caisse au guichet).
        /// </summary>
        [HttpPost("utiliser-jeton")]
        [Authorize(Roles = "Admin,Caissier,Financier,Percepteur")]
        public async Task<ActionResult<RetraitPaiementResultDto>> UtiliserJetonRetrait(
            [FromBody] JetonRetraitUtilisationDto utilisationDto,
            CancellationToken ct = default)
        {
            return await ExecuterPaiementRetraitAsync(utilisationDto, ct);
        }

        /// <summary>
        /// Alias métier : marquer une demande de retrait comme payée via le jeton.
        /// </summary>
        [HttpPost("marquer-paye")]
        [Authorize(Roles = "Admin,Caissier,Financier,Percepteur")]
        public async Task<ActionResult<RetraitPaiementResultDto>> MarquerPaye(
            [FromBody] JetonRetraitUtilisationDto utilisationDto,
            CancellationToken ct = default)
        {
            return await ExecuterPaiementRetraitAsync(utilisationDto, ct);
        }

        private async Task<ActionResult<RetraitPaiementResultDto>> ExecuterPaiementRetraitAsync(
            JetonRetraitUtilisationDto utilisationDto,
            CancellationToken ct)
        {
            try
            {
                var operateurUtilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var resultat = await _retraitAgentService.UtiliserJetonRetraitAsync(
                    utilisationDto,
                    operateurUtilisateurId,
                    ct);

                if (resultat.Succes)
                    return Ok(resultat);

                if (resultat.CodeErreur == "JETON_DEJA_UTILISE")
                    return Conflict(resultat);

                return BadRequest(resultat);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du paiement retrait jeton {CodeJeton}", utilisationDto.CodeJeton);
                return this.TechnicalErrorResponse("Erreur serveur lors du paiement du retrait", ex);
            }
        }

        /// <summary>
        /// Récupère les statistiques de retrait pour un mois donné
        /// </summary>
        [HttpGet("stats/{date:datetime}")]
        public async Task<ActionResult<DemandeRetraitAgentStatsDto>> GetStats(DateTime date, CancellationToken ct = default)
        {
            try
            {
                var stats = await _retraitAgentService.GetStatsAsync(date, ct);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques pour {Date}", date);
                return this.TechnicalErrorResponse("Erreur serveur lors de la récupération des statistiques", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes en attente de validation
        /// </summary>
        [HttpGet("en-attente")]
        public async Task<ActionResult<List<DemandeRetraitAgentReadDto>>> GetEnAttente(CancellationToken ct = default)
        {
            try
            {
                var demandes = await _retraitAgentRepository.GetByStatutAsync("EN_ATTENTE", ct);
                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dtos = demandes.Select(d => MapToReadDto(d, devisePrincipale)).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes en attente");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes validées (avec jeton généré)
        /// </summary>
        [HttpGet("validees")]
        public async Task<ActionResult<List<DemandeRetraitAgentReadDto>>> GetValidees(CancellationToken ct = default)
        {
            try
            {
                var demandes = await _retraitAgentRepository.GetByStatutAsync("VALIDEE", ct);
                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dtos = demandes.Select(d => MapToReadDto(d, devisePrincipale)).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes validées");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Récupère les demandes traitées (retrait effectué)
        /// </summary>
        [HttpGet("traitees")]
        public async Task<ActionResult<List<DemandeRetraitAgentReadDto>>> GetTraitees(CancellationToken ct = default)
        {
            try
            {
                var demandes = await _retraitAgentRepository.GetByStatutAsync("TRAITEE", ct);
                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dtos = demandes.Select(d => MapToReadDto(d, devisePrincipale)).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes traitées");
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Met à jour le statut d'une demande (rejet/validation)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<DemandeRetraitAgentReadDto>> Update(int id, [FromBody] DemandeRetraitAgentValidationDto validationDto, CancellationToken ct = default)
        {
            try
            {
                var demande = await _retraitAgentRepository.GetByIdAsync(id, ct);
                if (demande == null)
                {
                    return NotFound(new { error = "Demande de retrait non trouvée" });
                }

                demande.StatutDemande = validationDto.StatutDemande;
                demande.DateValidation = DateTime.Now;
                demande.AgentValidationId = validationDto.AgentValidationId;
                demande.MotifRejet = validationDto.MotifValidation;
                demande.DateModification = DateTime.Now;

                var updatedDemande = await _retraitAgentRepository.UpdateAsync(id, demande, ct);
                
                if (updatedDemande == null)
                {
                    return StatusCode(500, new { error = "Erreur lors de la mise à jour" });
                }

                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var dto = MapToReadDto(updatedDemande, devisePrincipale);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la demande de retrait {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }

        /// <summary>
        /// Supprime une demande de retrait
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var succes = await _retraitAgentRepository.DeleteAsync(id, ct);
                if (succes)
                {
                    return Ok(new { success = true, message = "Demande de retrait supprimée avec succès" });
                }
                else
                {
                    return NotFound(new { error = "Demande de retrait non trouvée" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la demande de retrait {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur", ex);
            }
        }
    }
}
