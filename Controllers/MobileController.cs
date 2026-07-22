using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Mobile;
using ProsocAPI.Services.Mobile;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MobileController : ControllerBase
    {
        private readonly IMobileAppServiceSimple _mobileAppService;
        private readonly ILogger<MobileController> _logger;

        public MobileController(
            IMobileAppServiceSimple mobileAppService,
            ILogger<MobileController> logger)
        {
            _mobileAppService = mobileAppService;
            _logger = logger;
        }

        /// <summary>
        /// Récupérer la configuration de l'application mobile
        /// </summary>
        [HttpGet("config")]
        public async Task<ActionResult<MobileAppConfig>> GetAppConfig(
            [FromHeader] string platform = "Android",
            [FromHeader] string version = "1.0.0")
        {
            try
            {
                var config = await _mobileAppService.GetAppConfigAsync(platform, version);
                
                // Vérifier si l'application est en maintenance
                if (config.IsMaintenanceMode)
                {
                    return StatusCode(503, new
                    {
                        message = "Application en maintenance",
                        maintenanceStart = config.MaintenanceStart,
                        maintenanceEnd = config.MaintenanceEnd,
                        maintenanceMessage = config.MaintenanceMessage
                    });
                }

                // Vérifier si une mise à jour est requise
                if (config.IsForceUpdateRequired)
                {
                    return StatusCode(426, new
                    {
                        message = "Mise à jour requise",
                        currentVersion = version,
                        minVersion = config.Version,
                        updateUrl = platform == "Android" ? config.PlayStoreUrl : config.AppStoreUrl,
                        updateMessage = config.UpdateMessage
                    });
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération de la configuration mobile",
                    ex);
            }
        }

        /// <summary>
        /// Authentifier un utilisateur mobile
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<MobileLoginResponseDto>> Login([FromBody] MobileLoginRequestDto request)
        {
            try
            {
                // TODO: Valider les identifiants avec le service d'authentification
                // Pour l'instant, nous allons simuler une authentification réussie
                var utilisateurId = 1; // Simulé

                var session = await _mobileAppService.CreateSessionAsync(
                    utilisateurId,
                    request.DeviceId,
                    request.Platform,
                    request.AppVersion,
                    GetClientIpAddress(),
                    Request.Headers["User-Agent"].ToString());

                return Ok(new MobileLoginResponseDto
                {
                    Success = true,
                    SessionToken = session.SessionToken,
                    UtilisateurId = utilisateurId,
                    NomUtilisateur = "Utilisateur Test", // Simulé
                    DateExpiration = session.DateExpiration,
                    Message = "Authentification réussie"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'authentification mobile");
                return StatusCode(500, new MobileLoginResponseDto
                {
                    Success = false,
                    Message = "Erreur d'authentification"
                });
            }
        }

        /// <summary>
        /// Valider une session mobile
        /// </summary>
        [HttpPost("validate-session")]
        public async Task<ActionResult<MobileSessionValidationDto>> ValidateSession([FromBody] MobileSessionValidationRequestDto request)
        {
            try
            {
                var session = await _mobileAppService.ValidateSessionAsync(request.SessionToken, request.DeviceId);
                
                if (session == null)
                {
                    return Ok(new MobileSessionValidationDto
                    {
                        Valid = false,
                        Message = "Session invalide ou expirée"
                    });
                }

                return Ok(new MobileSessionValidationDto
                {
                    Valid = true,
                    UtilisateurId = session.UtilisateurId,
                    SessionToken = session.SessionToken,
                    DateExpiration = session.DateExpiration,
                    Message = "Session valide"
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la validation de session mobile",
                    ex);
            }
        }

        /// <summary>
        /// Déconnexion mobile
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] MobileLogoutRequestDto request)
        {
            try
            {
                var success = await _mobileAppService.TerminateSessionAsync(request.SessionToken);
                
                if (success)
                {
                    return Ok(new { message = "Déconnexion réussie" });
                }

                return BadRequest(new { message = "Session non trouvée" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la déconnexion mobile",
                    ex);
            }
        }

        /// <summary>
        /// Synchroniser les données utilisateur
        /// </summary>
        [HttpPost("sync")]
        public async Task<ActionResult<ProsocAPI.Models.Mobile.MobileSyncResultDto>> SyncData([FromBody] MobileSyncRequestDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized("Utilisateur non authentifié");
                }

                var result = await _mobileAppService.SyncUserDataAsync(currentUserId, request.LastSyncDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la synchronisation des données",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les données de synchronisation en attente
        /// </summary>
        [HttpGet("sync/pending")]
        public async Task<ActionResult<List<MobileSyncData>>> GetPendingSyncData()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized("Utilisateur non authentifié");
                }

                var pendingData = await _mobileAppService.GetPendingSyncDataAsync(currentUserId);
                return Ok(pendingData);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des données de synchronisation en attente",
                    ex);
            }
        }

        /// <summary>
        /// Marquer des données comme synchronisées
        /// </summary>
        [HttpPost("sync/mark-synced")]
        public async Task<ActionResult> MarkAsSynced([FromBody] MobileMarkSyncedRequestDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized("Utilisateur non authentifié");
                }

                var success = await _mobileAppService.MarkSyncDataAsSyncedAsync(request.SyncDataId);
                
                if (success)
                {
                    return Ok(new { message = "Données marquées comme synchronisées" });
                }

                return BadRequest(new { message = "Données de synchronisation non trouvées" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur lors du marquage des données comme synchronisées",
                    ex);
            }
        }

        /// <summary>
        /// Envoyer une notification push de test
        /// </summary>
        [HttpPost("push/test")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult> SendTestPush([FromBody] MobileTestPushRequestDto request)
        {
            try
            {
                var success = await _mobileAppService.SendPushNotificationAsync(
                    request.UtilisateurId,
                    request.Titre,
                    request.Message,
                    request.Data);

                if (success)
                {
                    return Ok(new { message = "Notification push envoyée avec succès" });
                }

                return BadRequest(new { message = "Échec de l'envoi de la notification push" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de l'envoi de la notification push de test",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les statistiques d'utilisation mobile
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ProsocAPI.Models.Mobile.MobileUsageStatsDto>> GetUsageStats(
            [FromQuery] DateTime? debut = null,
            [FromQuery] DateTime? fin = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized("Utilisateur non authentifié");
                }

                var startDate = debut ?? DateTime.Now.AddDays(-30);
                var endDate = fin ?? DateTime.Now;

                var stats = await _mobileAppService.GetUsageStatsAsync(currentUserId, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des statistiques d'utilisation",
                    ex);
            }
        }

        /// <summary>
        /// Nettoyer les sessions expirées (admin seulement)
        /// </summary>
        [HttpPost("cleanup-sessions")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult> CleanupExpiredSessions()
        {
            try
            {
                var cleanedCount = await _mobileAppService.CleanupExpiredSessionsAsync();
                return Ok(new 
                { 
                    message = "Nettoyage des sessions expirées terminé",
                    cleanedCount = cleanedCount
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur lors du nettoyage des sessions expirées",
                    ex);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("IdUtilisateur")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    // DTOs pour les requêtes/réponses mobiles
    public class MobileLoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public class MobileLoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SessionToken { get; set; }
        public int? UtilisateurId { get; set; }
        public string? NomUtilisateur { get; set; }
        public DateTime? DateExpiration { get; set; }
    }

    public class MobileSessionValidationRequestDto
    {
        public string SessionToken { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
    }

    public class MobileSessionValidationDto
    {
        public bool Valid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UtilisateurId { get; set; }
        public string? SessionToken { get; set; }
        public DateTime? DateExpiration { get; set; }
    }

    public class MobileLogoutRequestDto
    {
        public string SessionToken { get; set; } = string.Empty;
    }

    public class MobileSyncRequestDto
    {
        public string LastSyncDate { get; set; } = string.Empty;
    }

    public class MobileMarkSyncedRequestDto
    {
        public int SyncDataId { get; set; }
    }

    public class MobileTestPushRequestDto
    {
        public int UtilisateurId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
