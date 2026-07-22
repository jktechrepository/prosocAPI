using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProsocAPI.Services.Queue;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin,IT")]
    public class NotificationQueueController : ControllerBase
    {
        private readonly INotificationQueueService _queueService;
        private readonly ILogger<NotificationQueueController> _logger;

        public NotificationQueueController(
            INotificationQueueService queueService,
            ILogger<NotificationQueueController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        /// <summary>
        /// Récupérer les statistiques de la queue de notifications
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<NotificationQueueStatsDto>> GetStats()
        {
            try
            {
                var stats = await _queueService.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des statistiques de la queue",
                    ex);
            }
        }

        /// <summary>
        /// Démarrer le traitement de la queue (admin seulement)
        /// </summary>
        [HttpPost("start")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> StartProcessing()
        {
            try
            {
                await _queueService.StartProcessingAsync();
                return Ok(new { message = "Traitement de la queue démarré" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur lors du démarrage du traitement de la queue",
                    ex);
            }
        }

        /// <summary>
        /// Arrêter le traitement de la queue (admin seulement)
        /// </summary>
        [HttpPost("stop")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> StopProcessing()
        {
            try
            {
                await _queueService.StopProcessingAsync();
                return Ok(new { message = "Traitement de la queue arrêté" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de l'arrêt du traitement de la queue",
                    ex);
            }
        }

        /// <summary>
        /// Mettre en file d'attente une notification de test
        /// </summary>
        [HttpPost("test")]
        [Authorize(Roles = "Admin,SuperAdmin,IT")]
        public async Task<ActionResult> QueueTestNotification([FromBody] TestNotificationDto testDto)
        {
            try
            {
                await _queueService.QueueNotificationAsync(
                    testDto.UserId,
                    testDto.Title,
                    testDto.Message,
                    testDto.Type);

                return Ok(new { message = "Notification de test mise en queue" });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la mise en queue de la notification de test",
                    ex);
            }
        }
    }

    public class TestNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "INFO";
    }
}
