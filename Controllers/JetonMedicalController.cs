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
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JetonMedicalController : BaseApiController
    {
        private readonly IJetonMedicalRepository _jetonMedicalRepository;
        private readonly ProsocDbContext _db;
        private readonly ILogger<JetonMedicalController> _logger;

        public JetonMedicalController(
            IJetonMedicalRepository jetonMedicalRepository,
            ProsocDbContext db,
            ILogger<JetonMedicalController> logger,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions)
            : base(paginationService, paginationOptions, logger)
        {
            _jetonMedicalRepository = jetonMedicalRepository;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les jetons médicaux
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<JetonMedicalReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.JetonsMedicaux
                    .Include(j => j.Affilie)
                    .Include(j => j.HopitalPartenaire)
                    .AsQueryable();

                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct: default);
                    if (hopitalId <= 0)
                        return Forbid();

                    query = query.Where(j => j.HopitalPartenaireId == hopitalId);
                }

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(j => new JetonMedicalReadDto
                {
                    IdJeton = j.IdJeton,
                    AffilieId = j.AffilieId,
                    AffilieNom = j.Affilie?.NomComplet,
                    CodeJeton = j.CodeJeton,
                    DateEmission = j.DateEmission,
                    DateUtilisation = j.DateUtilisation,
                    DateExpiration = j.DateExpiration,
                    EstValide = j.EstValide,
                    EstUtilise = j.EstUtilise,
                    HopitalPartenaireId = j.HopitalPartenaireId,
                    HopitalPartenaireNom = j.HopitalPartenaire?.Nom,
                    Observation = j.Observation,
                    DateCreation = j.DateCreation,
                    Statut = j.Statut
                }).ToList();

                var paginatedDtos = new PaginatedResponse<JetonMedicalReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des jetons médicaux paginés",
                    ex);
            }
        }

        /// <summary>
        /// Récupère un jeton médical par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<JetonMedicalReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var jeton = await _jetonMedicalRepository.GetByIdAsync(id, ct);
            if (jeton == null)
                return NotFound();

            if (CurrentUserHopitalResolver.IsAgentHopital(User))
            {
                var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                if (hopitalId <= 0 || jeton.HopitalPartenaireId != hopitalId)
                    return Forbid();
            }

            var dto = new JetonMedicalReadDto
            {
                IdJeton = jeton.IdJeton,
                AffilieId = jeton.AffilieId,
                AffilieNom = jeton.Affilie?.NomComplet,
                CodeJeton = jeton.CodeJeton,
                DateEmission = jeton.DateEmission,
                DateUtilisation = jeton.DateUtilisation,
                DateExpiration = jeton.DateExpiration,
                EstValide = jeton.EstValide,
                EstUtilise = jeton.EstUtilise,
                HopitalPartenaireId = jeton.HopitalPartenaireId,
                HopitalPartenaireNom = jeton.HopitalPartenaire?.Nom,
                Observation = jeton.Observation,
                DateCreation = jeton.DateCreation,
                Statut = jeton.Statut
            };

            return Ok(dto);
        }

        /// <summary>
        /// Récupère les jetons d'un affilié
        /// </summary>
        [HttpGet("by-affilie/{affilieId}")]
        public async Task<ActionResult<List<JetonMedicalReadDto>>> GetByAffilie(int affilieId, CancellationToken ct = default)
        {
            var jetons = await _jetonMedicalRepository.GetByAffilieAsync(affilieId, ct);
            var dtos = jetons.Select(j => new JetonMedicalReadDto
            {
                IdJeton = j.IdJeton,
                AffilieId = j.AffilieId,
                AffilieNom = j.Affilie?.NomComplet,
                CodeJeton = j.CodeJeton,
                DateEmission = j.DateEmission,
                DateUtilisation = j.DateUtilisation,
                DateExpiration = j.DateExpiration,
                EstValide = j.EstValide,
                EstUtilise = j.EstUtilise,
                HopitalPartenaireId = j.HopitalPartenaireId,
                HopitalPartenaireNom = j.HopitalPartenaire?.Nom,
                Observation = j.Observation,
                DateCreation = j.DateCreation,
                Statut = j.Statut
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Émet un nouveau jeton médical pour un affilié
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<JetonMedicalReadDto>> Create([FromBody] JetonMedicalCreateDto createDto, CancellationToken ct = default)
        {
            return BadRequest(new
            {
                success = false,
                message = "La création standalone de JetonMedical est désactivée. Utilisez le workflow DemandeBonEnvoi puis /api/DemandeBonEnvoi/{id}/confirmer."
            });
        }

        /// <summary>
        /// Valide un jeton médical (utilisé par les hôpitaux partenaires)
        /// </summary>
        [HttpPost("valider")]
        [Authorize(Roles = "Admin,SuperAdmin,IT,Agent Hôpital")]
        public async Task<ActionResult> ValiderJeton([FromBody] JetonMedicalValidationDto validationDto, CancellationToken ct = default)
        {
            try
            {
                if (!HasPermission("USE_JETON_MEDICAL") && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                    return ForbiddenPermission("USE_JETON_MEDICAL");

                var hopitalId = validationDto.HopitalPartenaireId;
                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                    if (hopitalId <= 0)
                        return Forbid();
                }

                var isValid = await _jetonMedicalRepository.ValiderJetonAsync(
                    validationDto.CodeJeton, hopitalId, ct);

                if (isValid)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Jeton valide",
                        code = validationDto.CodeJeton 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Jeton invalide, expiré ou déjà utilisé" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du jeton {Code}", validationDto.CodeJeton);
                return this.TechnicalErrorResponse("Erreur serveur lors de la validation", ex);
            }
        }

        /// <summary>
        /// Marque un jeton comme utilisé
        /// </summary>
        [HttpPost("utiliser")]
        [Authorize(Roles = "Admin,SuperAdmin,IT,Agent Hôpital")]
        public async Task<ActionResult> UtiliserJeton([FromBody] JetonMedicalUtilisationDto utilisationDto, CancellationToken ct = default)
        {
            try
            {
                if (!HasPermission("USE_JETON_MEDICAL") && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                    return ForbiddenPermission("USE_JETON_MEDICAL");

                var hopitalId = utilisationDto.HopitalPartenaireId;
                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                    if (hopitalId <= 0)
                        return Forbid();
                }

                var jetonAutorise = await HopitalScopeHelper.IsJetonLinkedToHopitalAsync(
                    _db, utilisationDto.IdJeton, hopitalId, ct);
                if (!jetonAutorise)
                    return Forbid();

                var bonLie = await _db.BonsEnvoi
                    .FirstOrDefaultAsync(b => b.JetonMedicalId == utilisationDto.IdJeton, ct);
                if (bonLie == null || !bonLie.Statut)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Jeton non conforme: aucun bon d'envoi actif lié."
                    });
                }

                var success = await _jetonMedicalRepository.UtiliserJetonAsync(
                    utilisationDto.IdJeton, utilisationDto.ObservationUtilisation ?? "", ct);

                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Jeton utilisé avec succès" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Impossible d'utiliser ce jeton" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'utilisation du jeton {Id}", utilisationDto.IdJeton);
                return this.TechnicalErrorResponse("Erreur serveur lors de l'utilisation", ex);
            }
        }

        /// <summary>
        /// Récupère les jetons valides
        /// </summary>
        [HttpGet("valides")]
        public async Task<ActionResult<List<JetonMedicalReadDto>>> GetValides(CancellationToken ct = default)
        {
            var jetons = await _jetonMedicalRepository.GetValidesAsync(ct);

            if (CurrentUserHopitalResolver.IsAgentHopital(User))
            {
                var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                if (hopitalId <= 0)
                    return Forbid();

                jetons = jetons.Where(j => j.HopitalPartenaireId == hopitalId).ToList();
            }

            var dtos = jetons.Select(j => new JetonMedicalReadDto
            {
                IdJeton = j.IdJeton,
                AffilieId = j.AffilieId,
                AffilieNom = j.Affilie?.NomComplet,
                CodeJeton = j.CodeJeton,
                DateEmission = j.DateEmission,
                DateUtilisation = j.DateUtilisation,
                DateExpiration = j.DateExpiration,
                EstValide = j.EstValide,
                EstUtilise = j.EstUtilise,
                HopitalPartenaireId = j.HopitalPartenaireId,
                HopitalPartenaireNom = j.HopitalPartenaire?.Nom,
                Observation = j.Observation,
                DateCreation = j.DateCreation,
                Statut = j.Statut
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Récupère les jetons expirés
        /// </summary>
        [HttpGet("expires")]
        public async Task<ActionResult<List<JetonMedicalReadDto>>> GetExpires(CancellationToken ct = default)
        {
            var jetons = await _jetonMedicalRepository.GetExpiresAsync(ct);

            if (CurrentUserHopitalResolver.IsAgentHopital(User))
            {
                var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                if (hopitalId <= 0)
                    return Forbid();

                jetons = jetons.Where(j => j.HopitalPartenaireId == hopitalId).ToList();
            }

            var dtos = jetons.Select(j => new JetonMedicalReadDto
            {
                IdJeton = j.IdJeton,
                AffilieId = j.AffilieId,
                AffilieNom = j.Affilie?.NomComplet,
                CodeJeton = j.CodeJeton,
                DateEmission = j.DateEmission,
                DateUtilisation = j.DateUtilisation,
                DateExpiration = j.DateExpiration,
                EstValide = j.EstValide,
                EstUtilise = j.EstUtilise,
                HopitalPartenaireId = j.HopitalPartenaireId,
                HopitalPartenaireNom = j.HopitalPartenaire?.Nom,
                Observation = j.Observation,
                DateCreation = j.DateCreation,
                Statut = j.Statut
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Récupère les statistiques des jetons
        /// </summary>
        [HttpGet("stats/{date:datetime}")]
        public async Task<ActionResult<JetonMedicalStatsDto>> GetStats(DateTime date, CancellationToken ct = default)
        {
            try
            {
                var stats = await _jetonMedicalRepository.GetStatsAsync(date, ct);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques pour {Date}", date);
                return this.TechnicalErrorResponse("Erreur serveur lors de la récupération des statistiques", ex);
            }
        }

        /// <summary>
        /// Archive les jetons expirés
        /// </summary>
        [HttpPost("archiver-expires")]
        public async Task<ActionResult> ArchiverJetonsExpires(CancellationToken ct = default)
        {
            try
            {
                var success = await _jetonMedicalRepository.ArchiverJetonsExpiresAsync(ct);
                
                return Ok(new { 
                    success = success, 
                    message = success ? "Jetons expirés archivés avec succès" : "Aucun jeton à archiver" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'archivage des jetons expirés");
                return this.TechnicalErrorResponse("Erreur serveur lors de l'archivage", ex);
            }
        }
    }
}
