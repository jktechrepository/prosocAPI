using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Caissier,Financier,Percepteur")]
    public class CaisseController : ControllerBase
    {
        private readonly ICaisseService _caisseService;
        private readonly ILogger<CaisseController> _logger;

        public CaisseController(ICaisseService caisseService, ILogger<CaisseController> logger)
        {
            _caisseService = caisseService;
            _logger = logger;
        }

        [HttpPost("session/ouvrir")]
        public async Task<ActionResult<SessionCaisseReadDto>> OuvrirSession(
            [FromBody] SessionCaisseOuvrirDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var session = await _caisseService.OuvrirSessionAsync(utilisateurId, dto, ct);
                return Ok(session);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur ouverture session caisse");
                return StatusCode(500, new { error = "Erreur lors de l'ouverture de la session de caisse" });
            }
        }

        [HttpPost("session/{id}/cloturer")]
        public async Task<ActionResult<SessionCaisseReadDto>> CloturerSession(
            int id,
            [FromBody] SessionCaisseCloturerDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var session = await _caisseService.CloturerSessionAsync(utilisateurId, id, dto, ct);
                return Ok(session);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur clôture session caisse {SessionId}", id);
                return StatusCode(500, new { error = "Erreur lors de la clôture de la session de caisse" });
            }
        }

        [HttpGet("session/courante")]
        public async Task<ActionResult<SessionCaisseReadDto>> GetSessionCourante(CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var session = await _caisseService.GetSessionCouranteAsync(utilisateurId, ct);
                if (session == null)
                    return NotFound(new { error = "Aucune session de caisse ouverte" });

                return Ok(session);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur récupération session caisse courante");
                return StatusCode(500, new { error = "Erreur lors de la récupération de la session courante" });
            }
        }

        /// <summary>
        /// Liste paginée des sessions de caisse de l'utilisateur connecté (ouvertes et clôturées).
        /// </summary>
        [HttpGet("sessions")]
        public async Task<ActionResult<PaginatedResponse<SessionCaisseReadDto>>> GetSessions(
            [FromQuery] DateTime? dateDebut,
            [FromQuery] DateTime? dateFin,
            [FromQuery] string? statut,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var sessions = await _caisseService.GetSessionsAsync(
                    utilisateurId, dateDebut, dateFin, statut, request, ct);
                return Ok(sessions);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur liste sessions caisse");
                return StatusCode(500, new { error = "Erreur lors de la récupération des sessions de caisse" });
            }
        }

        [HttpGet("session/{id}/solde")]
        public async Task<ActionResult<SessionCaisseSoldeDto>> GetSolde(int id, CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var solde = await _caisseService.GetSoldeSessionAsync(utilisateurId, id, ct);
                if (solde == null)
                    return NotFound(new { error = "Session de caisse introuvable" });

                return Ok(solde);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur solde session caisse {SessionId}", id);
                return StatusCode(500, new { error = "Erreur lors du calcul du solde de caisse" });
            }
        }

        [HttpGet("session/{id}/mouvements")]
        public async Task<ActionResult<PaginatedResponse<MouvementCaisseReadDto>>> GetMouvements(
            int id,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var utilisateurId = CurrentUserResolver.GetCurrentUtilisateurId(User);
                var mouvements = await _caisseService.GetMouvementsAsync(utilisateurId, id, request, ct);
                return Ok(mouvements);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mouvements session caisse {SessionId}", id);
                return StatusCode(500, new { error = "Erreur lors de la récupération des mouvements de caisse" });
            }
        }
    }
}
