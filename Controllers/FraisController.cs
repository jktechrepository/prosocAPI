using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Temporairement désactivé pour test
    public class FraisController : ControllerBase
    {
        private readonly IFraisService _fraisService;
        private readonly ILogger<FraisController> _logger;

        public FraisController(IFraisService fraisService, ILogger<FraisController> logger)
        {
            _fraisService = fraisService;
            _logger = logger;
        }

        /// <summary>
        /// Récupérer tous les frais
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Frais>>> GetAll()
        {
            try
            {
                var frais = await _fraisService.GetAllAsync();
                return Ok(frais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération de tous les frais",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer un frais par son ID
        /// </summary>
        /// <summary>Récupère un frais par son code métier (ex. PENALITE_RETARD_COTISATION).</summary>
        [HttpGet("by-code/{code}")]
        [AllowAnonymous]
        public async Task<ActionResult<Frais>> GetByCode(string code, CancellationToken ct = default)
        {
            try
            {
                var frais = await _fraisService.GetByCodeAsync(code, ct);
                return frais == null ? NotFound($"Frais avec le code « {code} » non trouvé") : Ok(frais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération du frais par code ",
                    ex);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Frais>> GetById(int id)
        {
            try
            {
                var frais = await _fraisService.GetByIdAsync(id);
                if (frais == null)
                {
                    return NotFound($"Frais avec ID {id} non trouvé");
                }

                return Ok(frais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des frais ID: ",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les frais par devise
        /// </summary>
        [HttpGet("devise/{deviseId}")]
        public async Task<ActionResult<List<Frais>>> GetByDevise(int deviseId)
        {
            try
            {
                var frais = await _fraisService.GetByDeviseAsync(deviseId);
                return Ok(frais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des frais par devise ID: ",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer le total des frais par devise
        /// </summary>
        [HttpGet("total/{deviseId}")]
        public async Task<ActionResult<double>> GetTotalByDevise(int deviseId)
        {
            try
            {
                var total = await _fraisService.GetTotalByDeviseAsync(deviseId);
                return Ok(total);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur lors du calcul du total des frais par devise ID: ",
                    ex);
            }
        }

        /// <summary>
        /// Créer un nouveau frais
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,IT,Financier")]
        public async Task<ActionResult<Frais>> Create([FromBody] CreateFraisDto dto)
        {
            try
            {
                var frais = new Frais
                {
                    Code = dto.Code,
                    Libelle = dto.Libelle,
                    Montant = dto.Montant,
                    DeviseId = dto.DeviseId,
                    TauxCommission = dto.TauxCommission,
                    CreeParId = GetCurrentUserId(),
                    DateCreation = DateTime.Now
                };

                var createdFrais = await _fraisService.CreateAsync(frais);
                return CreatedAtAction(nameof(GetById), new { id = createdFrais.IdFrais }, createdFrais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la création des frais",
                    ex);
            }
        }

        /// <summary>
        /// Mettre à jour un frais
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,IT,Financier")]
        public async Task<ActionResult<Frais>> Update(int id, [FromBody] UpdateFraisDto dto)
        {
            try
            {
                var existingFrais = await _fraisService.GetByIdAsync(id);
                if (existingFrais == null)
                {
                    return NotFound($"Frais avec ID {id} non trouvé");
                }

                var frais = new Frais
                {
                    IdFrais = id,
                    Code = dto.Code ?? existingFrais.Code,
                    Libelle = dto.Libelle,
                    Montant = dto.Montant,
                    DeviseId = dto.DeviseId,
                    TauxCommission = dto.TauxCommission,
                    ModifieParId = GetCurrentUserId(),
                    DateModification = DateTime.Now
                };

                var updatedFrais = await _fraisService.UpdateAsync(frais);
                return Ok(updatedFrais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la mise à jour des frais ID: ",
                    ex);
            }
        }

        /// <summary>
        /// Supprimer un frais
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _fraisService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound($"Frais avec ID {id} non trouvé");
                }

                return Ok(new { message = "Frais supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la suppression des frais ID: ",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les frais actifs par devise
        /// </summary>
        [HttpGet("active/{deviseId}")]
        public async Task<ActionResult<List<Frais>>> GetActiveByDevise(int deviseId)
        {
            try
            {
                var frais = await _fraisService.GetActiveByDeviseAsync(deviseId);
                return Ok(frais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des frais actifs par devise ID: ",
                    ex);
            }
        }
        
        // NOUVEAUX ENDPOINTS POUR LES COLLECTES ASSOCIÉES
        
        /// <summary>
        /// Récupérer les collectes pour un frais spécifique
        /// </summary>
        [HttpGet("{id}/collectes")]
        public async Task<ActionResult<List<Collecte>>> GetCollectesByFrais(int id)
        {
            try
            {
                var collectes = await _fraisService.GetCollectesByFraisAsync(id);
                return Ok(collectes);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des collectes pour les frais ID: ",
                    ex);
            }
        }
        
        /// <summary>
        /// Récupérer les statistiques des collectes pour un frais spécifique
        /// </summary>
        [HttpGet("{id}/collectes/stats")]
        public async Task<ActionResult<object>> GetCollectesStatsByFrais(int id)
        {
            try
            {
                var total = await _fraisService.GetTotalCollectesByFraisAsync(id);
                var count = await _fraisService.GetCountCollectesByFraisAsync(id);
                
                return Ok(new
                {
                    FraisId = id,
                    TotalMontantCollectes = total,
                    NombreCollectes = count,
                    MontantMoyen = count > 0 ? total / count : 0
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des statistiques des collectes pour les frais ID: ",
                    ex);
            }
        }
        
        /// <summary>
        /// Récupérer un frais avec toutes ses collectes
        /// </summary>
        [HttpGet("{id}/with-collectes")]
        public async Task<ActionResult<Frais>> GetByIdWithCollectes(int id)
        {
            try
            {
                var frais = await _fraisService.GetByIdWithCollectesAsync(id);
                if (frais == null)
                    return NotFound($"Frais avec ID {id} non trouvé");
                
                return Ok(frais);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des frais avec collectes ID: ",
                    ex);
            }
        }
        
        /// <summary>
        /// Récupérer les statistiques globales des collectes par frais
        /// </summary>
        [HttpGet("collectes/stats")]
        public async Task<ActionResult<Dictionary<int, int>>> GetCollectesStatsByFrais()
        {
            try
            {
                var stats = await _fraisService.GetCollectesStatsByFraisAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des statistiques globales des collectes par frais",
                    ex);
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("IdUtilisateur")?.Value;
            return int.TryParse(userIdClaim, out var userId) && userId > 0 ? userId : null; // 🆕 Retourne null si pas authentifié
        }
    }

    // DTOs pour les opérations sur les frais
    public class CreateFraisDto
    {
        [StringLength(50)]
        public string? Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Libelle { get; set; } = string.Empty;

        [Range(0.01, 99999999)]
        public double Montant { get; set; }

        [Required]
        public int DeviseId { get; set; }

        [Range(0, 100)]
        public decimal TauxCommission { get; set; }
    }

    public class UpdateFraisDto
    {
        [StringLength(50)]
        public string? Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Libelle { get; set; } = string.Empty;

        [Range(0.01, 99999999)]
        public double Montant { get; set; }

        [Required]
        public int DeviseId { get; set; }

        [Range(0, 100)]
        public decimal TauxCommission { get; set; }
    }
}
