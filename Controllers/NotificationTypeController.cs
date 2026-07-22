using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationTypeController : ControllerBase
    {
        private readonly INotificationTypeService _notificationTypeService;
        private readonly ILogger<NotificationTypeController> _logger;

        public NotificationTypeController(
            INotificationTypeService notificationTypeService,
            ILogger<NotificationTypeController> logger)
        {
            _notificationTypeService = notificationTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Récupérer tous les types de notifications actifs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<NotificationType>>> GetAll()
        {
            try
            {
                var types = await _notificationTypeService.GetAllAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des types de notifications",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer un type de notification par son code
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<NotificationType>> GetByCode(string code)
        {
            try
            {
                var type = await _notificationTypeService.GetByCodeAsync(code);
                if (type == null)
                    return NotFound($"Type de notification '{code}' non trouvé");

                return Ok(type);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération du type de notification ",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les types de notifications par catégorie
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<NotificationType>>> GetByCategory(string category)
        {
            try
            {
                var types = await _notificationTypeService.GetByCategoryAsync(category);
                return Ok(types);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des types de notifications pour la catégorie ",
                    ex);
            }
        }

        /// <summary>
        /// Créer un nouveau type de notification (admin seulement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult<NotificationType>> Create([FromBody] NotificationType type)
        {
            try
            {
                var createdType = await _notificationTypeService.CreateAsync(type);
                return CreatedAtAction(nameof(GetByCode), new { code = createdType.Code }, createdType);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la création du type de notification",
                    ex);
            }
        }

        /// <summary>
        /// Mettre à jour un type de notification (admin seulement)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult<NotificationType>> Update(int id, [FromBody] NotificationType type)
        {
            try
            {
                if (id != type.IdNotificationType)
                    return BadRequest("ID mismatch");

                var updatedType = await _notificationTypeService.UpdateAsync(type);
                return Ok(updatedType);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la mise à jour du type de notification ",
                    ex);
            }
        }

        /// <summary>
        /// Désactiver un type de notification (admin seulement)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _notificationTypeService.DeleteAsync(id);
                if (!success)
                    return NotFound($"Type de notification avec ID {id} non trouvé");

                return Ok(new { message = "Type de notification désactivé avec succès" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la suppression du type de notification ",
                    ex);
            }
        }

        /// <summary>
        /// Créer une notification avec un type spécifique
        /// </summary>
        [HttpPost("create-notification")]
        public async Task<ActionResult<Notification>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                    return Unauthorized("Utilisateur non identifié");

                var notification = await _notificationTypeService.CreateNotificationAsync(
                    request.TypeCode,
                    request.RecepteurId,
                    request.Titre,
                    request.Message,
                    request.Metadata,
                    currentUserId);

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la création de la notification",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les notifications d'un utilisateur par type
        /// </summary>
        [HttpGet("user/{userId}/type/{typeCode}")]
        public async Task<ActionResult<List<Notification>>> GetUserNotificationsByType(int userId, string typeCode)
        {
            try
            {
                // Vérifier que l'utilisateur ne peut voir que ses propres notifications
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                    return Unauthorized("Utilisateur non identifié");

                if (currentUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                    return StatusCode(403, new { success = false, message = "Accès non autorisé" });

                var notifications = await _notificationTypeService.GetNotificationsByTypeAsync(typeCode, userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des notifications de l'utilisateur ",
                    ex);
            }
        }

        /// <summary>
        /// Initialiser les types de notifications par défaut (admin seulement)
        /// </summary>
        [HttpPost("seed")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult> SeedDefaultTypes()
        {
            try
            {
                await _notificationTypeService.SeedDefaultTypesAsync();
                return Ok(new { message = "Types de notifications par défaut initialisés avec succès" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de l'initialisation des types de notifications par défaut",
                    ex);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("IdUtilisateur")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    public class CreateNotificationRequest
    {
        public string TypeCode { get; set; } = string.Empty;
        public int RecepteurId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Metadata { get; set; }
    }
}
