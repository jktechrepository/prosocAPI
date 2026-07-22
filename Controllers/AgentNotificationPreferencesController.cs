using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgentNotificationPreferencesController : ControllerBase
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<AgentNotificationPreferencesController> _logger;

        public AgentNotificationPreferencesController(
            ProsocDbContext db,
            ILogger<AgentNotificationPreferencesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Récupérer les préférences de notification de l'utilisateur connecté
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserNotificationPreferences>> GetMyPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("Utilisateur non identifié");
                }

                var preferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (preferences == null)
                {
                    // Créer les préférences par défaut
                    preferences = CreateDefaultPreferences(userId);
                    _db.UserNotificationPreferences.Add(preferences);
                    await _db.SaveChangesAsync();
                }

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des préférences de notification",
                    ex);
            }
        }

        /// <summary>
        /// Mettre à jour les préférences de notification
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<UserNotificationPreferences>> UpdatePreferences([FromBody] UserNotificationPreferences preferences)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("Utilisateur non identifié");
                }

                var existingPreferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingPreferences == null)
                {
                    // Créer si n'existe pas
                    preferences.UserId = userId;
                    preferences.DateCreation = DateTime.Now;
                    preferences.DateModification = DateTime.Now;
                    _db.UserNotificationPreferences.Add(preferences);
                }
                else
                {
                    // Mettre à jour les champs spécifiques
                    existingPreferences.CommissionEmail = preferences.CommissionEmail;
                    existingPreferences.CommissionSms = preferences.CommissionSms;
                    existingPreferences.CommissionPush = preferences.CommissionPush;
                    existingPreferences.CommissionInApp = preferences.CommissionInApp;
                    existingPreferences.MinCommissionAmount = preferences.MinCommissionAmount;
                    existingPreferences.CommissionCurrency = preferences.CommissionCurrency;
                    existingPreferences.CommissionMessageTemplate = preferences.CommissionMessageTemplate;
                    existingPreferences.QuietHoursEnabled = preferences.QuietHoursEnabled;
                    existingPreferences.QuietHoursStart = preferences.QuietHoursStart;
                    existingPreferences.QuietHoursEnd = preferences.QuietHoursEnd;
                    existingPreferences.Language = preferences.Language;
                    existingPreferences.Timezone = preferences.Timezone;
                    existingPreferences.DateModification = DateTime.Now;
                }

                await _db.SaveChangesAsync();

                var result = existingPreferences ?? preferences;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la mise à jour des préférences de notification",
                    ex);
            }
        }

        /// <summary>
        /// Réinitialiser les préférences aux valeurs par défaut
        /// </summary>
        [HttpPost("reset")]
        public async Task<ActionResult<UserNotificationPreferences>> ResetToDefault()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("Utilisateur non identifié");
                }

                var existingPreferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingPreferences != null)
                {
                    _db.UserNotificationPreferences.Remove(existingPreferences);
                    await _db.SaveChangesAsync();
                }

                var defaultPreferences = CreateDefaultPreferences(userId);
                _db.UserNotificationPreferences.Add(defaultPreferences);
                await _db.SaveChangesAsync();

                return Ok(defaultPreferences);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la réinitialisation des préférences de notification",
                    ex);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("IdUtilisateur")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private UserNotificationPreferences CreateDefaultPreferences(int userId)
        {
            return new UserNotificationPreferences
            {
                UserId = userId,
                EmailNotification = true,
                SmsNotification = true,
                PushNotification = true,
                InAppNotification = true,
                CommissionEmail = true,
                CommissionSms = false,
                CommissionPush = true,
                CommissionInApp = true,
                MinCommissionAmount = 1.0m,
                CommissionCurrency = "USD",
                CommissionMessageTemplate = null,
                Language = "fr",
                Timezone = "Africa/Kinshasa",
                QuietHoursEnabled = false,
                QuietHoursStart = 22,
                QuietHoursEnd = 7,
                DateCreation = DateTime.Now,
                Statut = true
            };
        }
    }
}
