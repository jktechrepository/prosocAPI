using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HopitalPartenaireController : BaseApiController
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<HopitalPartenaireController> _logger;

        public HopitalPartenaireController(
            ProsocDbContext db,
            ILogger<HopitalPartenaireController> logger,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions)
            : base(paginationService, paginationOptions, logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les hôpitaux partenaires
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<HopitalPartenaireReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.HopitalPartenaires
                    .AsQueryable();

                if (CurrentUserHopitalResolver.IsAgentHopital(User))
                {
                    var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct: default);
                    if (hopitalId <= 0)
                        return Forbid();

                    query = query.Where(h => h.IdHopital == hopitalId);
                }

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(h => new HopitalPartenaireReadDto
                {
                    IdHopital = h.IdHopital,
                    Nom = h.Nom,
                    Adresse = h.Adresse,
                    Telephone = h.Telephone,
                    Email = h.Email,
                    ContactPersonne = h.ContactPersonne,
                    CodeAcces = h.CodeAcces,
                    Niveau = h.Niveau,
                    EstActif = h.EstActif,
                    ServicesOfferts = h.ServicesOfferts,
                    PlafondJournalier = h.PlafondJournalier,
                    DateCreation = h.DateCreation,
                    DateModification = h.DateModification,
                    Statut = h.Statut
                }).ToList();

                var paginatedDtos = new PaginatedResponse<HopitalPartenaireReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des hôpitaux partenaires paginés",
                    ex);
            }
        }

        /// <summary>
        /// Récupère un hôpital partenaire par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<HopitalPartenaireReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var hopital = await _db.HopitalPartenaires
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.IdHopital == id, ct);

            if (hopital == null)
                return NotFound();

            if (CurrentUserHopitalResolver.IsAgentHopital(User))
            {
                var hopitalId = await CurrentUserHopitalResolver.ResolveHopitalPartenaireIdAsync(User, _db, ct);
                if (hopitalId <= 0 || hopital.IdHopital != hopitalId)
                    return Forbid();
            }

            var dto = new HopitalPartenaireReadDto
            {
                IdHopital = hopital.IdHopital,
                Nom = hopital.Nom,
                Adresse = hopital.Adresse,
                Telephone = hopital.Telephone,
                Email = hopital.Email,
                ContactPersonne = hopital.ContactPersonne,
                CodeAcces = hopital.CodeAcces,
                Niveau = hopital.Niveau,
                EstActif = hopital.EstActif,
                ServicesOfferts = hopital.ServicesOfferts,
                PlafondJournalier = hopital.PlafondJournalier,
                DateCreation = hopital.DateCreation,
                DateModification = hopital.DateModification,
                Statut = hopital.Statut
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crée un nouvel hôpital partenaire
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HopitalPartenaireReadDto>> Create([FromBody] HopitalPartenaireCreateDto createDto, CancellationToken ct = default)
        {
            try
            {
                var hopital = new HopitalPartenaire
                {
                    Nom = createDto.Nom,
                    Adresse = createDto.Adresse,
                    Telephone = createDto.Telephone,
                    Email = createDto.Email,
                    ContactPersonne = createDto.ContactPersonne,
                    CodeAcces = createDto.CodeAcces,
                    Niveau = createDto.Niveau,
                    EstActif = createDto.EstActif,
                    ServicesOfferts = createDto.ServicesOfferts,
                    PlafondJournalier = createDto.PlafondJournalier,
                    Statut = true,
                    DateCreation = DateTime.Now
                };

                _db.HopitalPartenaires.Add(hopital);
                await _db.SaveChangesAsync(ct);

                var responseDto = new HopitalPartenaireReadDto
                {
                    IdHopital = hopital.IdHopital,
                    Nom = hopital.Nom,
                    Adresse = hopital.Adresse,
                    Telephone = hopital.Telephone,
                    Email = hopital.Email,
                    ContactPersonne = hopital.ContactPersonne,
                    CodeAcces = hopital.CodeAcces,
                    Niveau = hopital.Niveau,
                    EstActif = hopital.EstActif,
                    ServicesOfferts = hopital.ServicesOfferts,
                    PlafondJournalier = hopital.PlafondJournalier,
                    DateCreation = hopital.DateCreation,
                    DateModification = hopital.DateModification,
                    Statut = hopital.Statut
                };

                return CreatedAtAction(nameof(GetById), new { id = hopital.IdHopital }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'hôpital partenaire");
                return this.TechnicalErrorResponse("Erreur serveur lors de la création", ex);
            }
        }

        /// <summary>
        /// Met à jour un hôpital partenaire
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<HopitalPartenaireReadDto>> Update(int id, [FromBody] HopitalPartenaireUpdateDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var hopital = await _db.HopitalPartenaires.FirstOrDefaultAsync(h => h.IdHopital == id, ct);
                if (hopital == null)
                    return NotFound();

                hopital.Nom = updateDto.Nom;
                hopital.Adresse = updateDto.Adresse;
                hopital.Telephone = updateDto.Telephone;
                hopital.Email = updateDto.Email;
                hopital.ContactPersonne = updateDto.ContactPersonne;
                hopital.CodeAcces = updateDto.CodeAcces;
                hopital.Niveau = updateDto.Niveau;
                hopital.EstActif = updateDto.EstActif;
                hopital.ServicesOfferts = updateDto.ServicesOfferts;
                hopital.PlafondJournalier = updateDto.PlafondJournalier;
                hopital.Statut = updateDto.Statut;
                hopital.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                var responseDto = new HopitalPartenaireReadDto
                {
                    IdHopital = hopital.IdHopital,
                    Nom = hopital.Nom,
                    Adresse = hopital.Adresse,
                    Telephone = hopital.Telephone,
                    Email = hopital.Email,
                    ContactPersonne = hopital.ContactPersonne,
                    CodeAcces = hopital.CodeAcces,
                    Niveau = hopital.Niveau,
                    EstActif = hopital.EstActif,
                    ServicesOfferts = hopital.ServicesOfferts,
                    PlafondJournalier = hopital.PlafondJournalier,
                    DateCreation = hopital.DateCreation,
                    DateModification = hopital.DateModification,
                    Statut = hopital.Statut
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'hôpital partenaire {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur lors de la mise à jour", ex);
            }
        }

        /// <summary>
        /// Supprime un hôpital partenaire
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                var hopital = await _db.HopitalPartenaires.FirstOrDefaultAsync(h => h.IdHopital == id, ct);
                if (hopital == null)
                    return NotFound();

                _db.HopitalPartenaires.Remove(hopital);
                await _db.SaveChangesAsync(ct);

                return Ok(new { message = "Hôpital partenaire supprimé avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'hôpital partenaire {Id}", id);
                return this.TechnicalErrorResponse("Erreur serveur lors de la suppression", ex);
            }
        }

        /// <summary>
        /// Récupère les hôpitaux actifs
        /// </summary>
        [HttpGet("actifs")]
        public async Task<ActionResult<List<HopitalPartenaireReadDto>>> GetActifs(CancellationToken ct = default)
        {
            var hopitaux = await _db.HopitalPartenaires
                .Where(h => h.EstActif && h.Statut)
                .AsNoTracking()
                .OrderBy(h => h.Nom)
                .ToListAsync(ct);

            var dtos = hopitaux.Select(h => new HopitalPartenaireReadDto
            {
                IdHopital = h.IdHopital,
                Nom = h.Nom,
                Adresse = h.Adresse,
                Telephone = h.Telephone,
                Email = h.Email,
                ContactPersonne = h.ContactPersonne,
                CodeAcces = h.CodeAcces,
                Niveau = h.Niveau,
                EstActif = h.EstActif,
                ServicesOfferts = h.ServicesOfferts,
                PlafondJournalier = h.PlafondJournalier,
                DateCreation = h.DateCreation,
                DateModification = h.DateModification,
                Statut = h.Statut
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Valide le code d'accès d'un hôpital
        /// </summary>
        [HttpPost("valider-acces")]
        [AllowAnonymous] // Public pour les hôpitaux partenaires
        public async Task<ActionResult> ValiderAcces([FromBody] HopitalAccesValidationDto validationDto, CancellationToken ct = default)
        {
            try
            {
                var hopital = await _db.HopitalPartenaires
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.CodeAcces == validationDto.CodeAcces && h.EstActif && h.Statut, ct);

                if (hopital == null)
                {
                    return BadRequest(new { success = false, message = "Code d'accès invalide ou hôpital inactif" });
                }

                return Ok(new { 
                    success = true, 
                    message = "Accès validé",
                    hopital = new {
                        IdHopital = hopital.IdHopital,
                        Nom = hopital.Nom,
                        Niveau = hopital.Niveau
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du code d'accès {Code}", validationDto.CodeAcces);
                return this.TechnicalErrorResponse("Erreur serveur lors de la validation", ex);
            }
        }
    }
}
