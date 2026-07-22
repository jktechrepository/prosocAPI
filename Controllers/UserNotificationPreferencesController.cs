using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserNotificationPreferencesController : BaseApiController
    {
        private readonly ProsocDbContext _db;

        public UserNotificationPreferencesController(
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<UserNotificationPreferencesController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value);

                var preferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.Statut);

                if (preferences == null)
                {
                    // Créer les préférences par défaut
                    preferences = new UserNotificationPreferences
                    {
                        UserId = userId,
                        EmailNotification = true,
                        SmsNotification = true,
                        PushNotification = true,
                        InAppNotification = true,
                        Language = "fr",
                        Timezone = "Africa/Kinshasa",
                        QuietHoursEnabled = false,
                        QuietHoursStart = 22,
                        QuietHoursEnd = 7
                    };

                    _db.UserNotificationPreferences.Add(preferences);
                    await _db.SaveChangesAsync();
                }

                var dto = new NotificationPreferencesDto
                {
                    EmailNotification = preferences.EmailNotification,
                    SmsNotification = preferences.SmsNotification,
                    PushNotification = preferences.PushNotification,
                    InAppNotification = preferences.InAppNotification,
                    Language = preferences.Language,
                    Timezone = preferences.Timezone,
                    QuietHoursEnabled = preferences.QuietHoursEnabled,
                    QuietHoursStart = preferences.QuietHoursStart,
                    QuietHoursEnd = preferences.QuietHoursEnd
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des préférences de notification",
                    ex);
            }
        }

        [HttpPut]
        public async Task<ActionResult<NotificationPreferencesDto>> UpdatePreferences([FromBody] NotificationPreferencesDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value);

                var preferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.Statut);

                if (preferences == null)
                {
                    preferences = new UserNotificationPreferences
                    {
                        UserId = userId,
                        EmailNotification = dto.EmailNotification,
                        SmsNotification = dto.SmsNotification,
                        PushNotification = dto.PushNotification,
                        InAppNotification = dto.InAppNotification,
                        Language = dto.Language,
                        Timezone = dto.Timezone,
                        QuietHoursEnabled = dto.QuietHoursEnabled,
                        QuietHoursStart = dto.QuietHoursStart,
                        QuietHoursEnd = dto.QuietHoursEnd,
                        DateModification = DateTime.Now
                    };

                    _db.UserNotificationPreferences.Add(preferences);
                }
                else
                {
                    preferences.EmailNotification = dto.EmailNotification;
                    preferences.SmsNotification = dto.SmsNotification;
                    preferences.PushNotification = dto.PushNotification;
                    preferences.InAppNotification = dto.InAppNotification;
                    preferences.Language = dto.Language;
                    preferences.Timezone = dto.Timezone;
                    preferences.QuietHoursEnabled = dto.QuietHoursEnabled;
                    preferences.QuietHoursStart = dto.QuietHoursStart;
                    preferences.QuietHoursEnd = dto.QuietHoursEnd;
                    preferences.DateModification = DateTime.Now;
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation("Préférences de notification mises à jour pour l'utilisateur {UserId}", userId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la mise à jour des préférences de notification",
                    ex);
            }
        }
    }
}
