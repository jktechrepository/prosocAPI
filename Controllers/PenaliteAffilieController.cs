using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
  [ApiController]
  [Route("api/penalite-affilie")]
  [Authorize]
  public class PenaliteAffilieController : ControllerBase
  {
    private readonly IPenaliteAffilieService _penaliteService;
    private readonly ProsocDbContext _db;
    private readonly ILogger<PenaliteAffilieController> _logger;

    public PenaliteAffilieController(
        IPenaliteAffilieService penaliteService,
        ProsocDbContext db,
        ILogger<PenaliteAffilieController> logger)
    {
      _penaliteService = penaliteService;
      _db = db;
      _logger = logger;
    }

    /// <summary>
    /// Liste toutes les pénalités d'un affilié. L'id est passé en paramètre de requête (idAffilie).
    /// </summary>
    [HttpGet("Affilie")]
    public async Task<ActionResult<List<PenaliteAffilie>>> GetAllByAffilie(
        [FromQuery] int idAffilie,
        CancellationToken ct = default)
    {
      if (idAffilie <= 0)
        return BadRequest("Le paramètre idAffilie est obligatoire et doit être > 0.");

      try
      {
        var exists = await _db.Affilies.AnyAsync(a => a.IdAffilie == idAffilie, ct);
        if (!exists)
          return NotFound($"Affilié avec ID {idAffilie} introuvable.");

        return Ok(await _penaliteService.GetByAffilieAsync(idAffilie, ct));
      }
      catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur récupération pénalités affilié ",
                    ex);
            }
    }

    [HttpGet("affilie/{affilieId}")]
    public async Task<ActionResult<List<PenaliteAffilie>>> GetByAffilie(int affilieId)
    {
      try
      {
        return Ok(await _penaliteService.GetByAffilieAsync(affilieId));
      }
      catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur récupération pénalités affilié ",
                    ex);
            }
    }

    [HttpGet("mes-penalites")]
    public async Task<ActionResult<List<PenaliteAffilie>>> GetMesPenalites(CancellationToken ct)
    {
      try
      {
        var affilieId = await GetCurrentAffilieIdAsync(ct);
        if (affilieId == 0)
          return Unauthorized("Utilisateur non authentifié ou non affilié");

        return Ok(await _penaliteService.GetByAffilieAsync(affilieId, ct));
      }
      catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur récupération pénalités affilié connecté",
                    ex);
            }
    }

    [HttpGet("arriere/{arrieresAffilieId}")]
    public async Task<ActionResult<List<PenaliteAffilie>>> GetByArriere(int arrieresAffilieId)
    {
      try
      {
        return Ok(await _penaliteService.GetByArriereAsync(arrieresAffilieId));
      }
      catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur récupération pénalités arriéré ",
                    ex);
            }
    }

    [HttpGet("resume")]
    public async Task<ActionResult<PenaliteResumeDto>> GetResume()
    {
      try
      {
        return Ok(await _penaliteService.GetResumeAsync());
      }
      catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur résumé pénalités",
                    ex);
            }
    }

    [HttpPost("appliquer")]
    public async Task<ActionResult<List<PenaliteAffilie>>> AppliquerPenalites(
        [FromBody] AppliquerPenalitesDto? dto)
    {
      try
      {
        var date = dto?.Date?.Date ?? DateTime.Today;
        return Ok(await _penaliteService.AppliquerPenalitesRetardCotisationAsync(date));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erreur application pénalités");
        return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
      }
    }

    [HttpPut("{id}/annuler")]
    public async Task<ActionResult<PenaliteAffilie>> AnnulerPenalite(
        int id,
        [FromBody] AnnulerPenaliteDto dto)
    {
      try
      {
        return Ok(await _penaliteService.AnnulerPenaliteAsync(id, dto.MotifAnnulation));
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return Conflict(ex.Message);
      }
      catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur annulation pénalité ",
                    ex);
            }
    }

    private Task<int> GetCurrentAffilieIdAsync(CancellationToken ct) =>
        CurrentUserAffilieResolver.ResolveAffilieIdAsync(User, _db, ct);
  }

  public class AppliquerPenalitesDto
  {
    public DateTime? Date { get; set; }
  }

  public class AnnulerPenaliteDto
  {
    public string MotifAnnulation { get; set; } = string.Empty;
  }
}
