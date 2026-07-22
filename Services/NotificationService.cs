using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProsocAPI.Hubs;
using ProsocAPI.Models.Core;
using Prosoc.Data;

namespace ProsocAPI.Services
{
    public interface INotificationService
    {
        Task SendAdhesionConfirmationAsync(int affilieId, string affilieName, string codeAdhesion, string typeAdhesion);
        Task SendPaymentReminderAsync(int affilieId, decimal amount, DateTime dueDate);
        Task SendAppointmentReminderAsync(int affilieId, DateTime appointmentDate, string location);
        Task SendCustomNotificationAsync(int userId, string title, string message, string type = "info");
        Task SendBroadcastToAffiliesAsync(string title, string message, string type = "info");
        Task SendToUserPreferredChannelsAsync(int userId, string title, string message, string type = "info");
    }

    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IPushNotificationService _pushService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ProsocDbContext _db;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IEmailService emailService,
            ISmsService smsService,
            IPushNotificationService pushService,
            IHubContext<NotificationHub> hubContext,
            ProsocDbContext db,
            ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _pushService = pushService;
            _hubContext = hubContext;
            _db = db;
            _logger = logger;
        }

        public async Task SendAdhesionConfirmationAsync(int affilieId, string affilieName, string codeAdhesion, string typeAdhesion)
        {
            try
            {
                var title = "🎉 Adhésion Confirmée";
                var message = $"Félicitations {affilieName}! Votre adhésion N°{codeAdhesion} est active.";
                var type = "success";

                // Créer la notification en base de données
                await CreateNotificationInDatabase(affilieId, title, message, type);

                // Envoyer selon les préférences
                await SendToAffiliePreferredChannelsAsync(affilieId, title, message, type);

                // Envoyer via SignalR en temps réel
                await _hubContext.Clients.Group($"affilie_{affilieId}").SendAsync("NewNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now,
                    data = new { type = "adhesion_confirmation", codeAdhesion, affilieId }
                });

                _logger.LogInformation("Notification d'adhésion envoyée pour l'affilié {AffilieId}", affilieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification d'adhésion pour l'affilié {AffilieId}", affilieId);
            }
        }

        public async Task SendPaymentReminderAsync(int affilieId, decimal amount, DateTime dueDate)
        {
            try
            {
                var affilie = await _db.Affilies
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId);

                if (affilie == null) return;

                var title = "💰 Rappel de Paiement";
                var message = $"Votre cotisation de {amount:CDF} est due pour le {dueDate:dd/MM/yyyy}";
                var type = "warning";

                // Créer la notification en base de données
                await CreateNotificationInDatabase(affilieId, title, message, type);

                // Envoyer selon les préférences
                await SendToAffiliePreferredChannelsAsync(affilieId, title, message, type);

                // Envoyer via SignalR
                await _hubContext.Clients.Group($"affilie_{affilieId}").SendAsync("NewNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now,
                    data = new { type = "payment_reminder", amount, dueDate = dueDate.ToString("yyyy-MM-dd"), affilieId }
                });

                _logger.LogInformation("Rappel de paiement envoyé pour l'affilié {AffilieId}", affilieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi du rappel de paiement pour l'affilié {AffilieId}", affilieId);
            }
        }

        public async Task SendAppointmentReminderAsync(int affilieId, DateTime appointmentDate, string location)
        {
            try
            {
                var affilie = await _db.Affilies
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId);

                if (affilie == null) return;

                var title = "📅 Rappel de Rendez-vous";
                var message = $"Rendez-vous le {appointmentDate:dd/MM/yyyy à HH:mm} à {location}";
                var type = "info";

                // Créer la notification en base de données
                await CreateNotificationInDatabase(affilieId, title, message, type);

                // Envoyer selon les préférences
                await SendToAffiliePreferredChannelsAsync(affilieId, title, message, type);

                // Envoyer via SignalR
                await _hubContext.Clients.Group($"affilie_{affilieId}").SendAsync("NewNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now,
                    data = new { type = "appointment_reminder", appointmentDate = appointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"), location, affilieId }
                });

                _logger.LogInformation("Rappel de rendez-vous envoyé pour l'affilié {AffilieId}", affilieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi du rappel de rendez-vous pour l'affilié {AffilieId}", affilieId);
            }
        }

        public async Task SendCustomNotificationAsync(int userId, string title, string message, string type = "info")
        {
            try
            {
                // Créer la notification en base de données
                await CreateNotificationForUserAsync(userId, title, message, type);

                // Envoyer selon les préférences
                await SendToUserPreferredChannelsAsync(userId, title, message, type);

                // Envoyer via SignalR
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("NewNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now,
                    data = new { type = "custom", userId }
                });

                _logger.LogInformation("Notification personnalisée envoyée à l'utilisateur {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification personnalisée à l'utilisateur {UserId}", userId);
            }
        }

        public async Task SendBroadcastToAffiliesAsync(string title, string message, string type = "info")
        {
            try
            {
                // Envoyer via SignalR à tous les affiliés
                await _hubContext.Clients.Group("all_affilies").SendAsync("BroadcastNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now
                });

                // Créer des notifications pour tous les affiliés actifs
                var affilies = await _db.Affilies
                    .Include(a => a.Adhesions)
                    .ThenInclude(ad => ad.Utilisateur)
                    .Where(a => a.Statut && a.Adhesions.Any(ad => ad.Utilisateur != null))
                    .ToListAsync();

                foreach (var affilie in affilies)
                {
                    foreach (var adhesion in affilie.Adhesions)
                    {
                        if (adhesion.Utilisateur?.IdUtilisateur > 0)
                        {
                            await CreateNotificationForUserAsync(adhesion.Utilisateur.IdUtilisateur, title, message, type);
                            await SendToUserPreferredChannelsAsync(adhesion.Utilisateur.IdUtilisateur, title, message, type);
                        }
                    }
                }

                _logger.LogInformation("Diffusion envoyée à tous les affiliés: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la diffusion à tous les affiliés");
            }
        }

        public async Task SendToUserPreferredChannelsAsync(int userId, string title, string message, string type = "info")
        {
            try
            {
                var preferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.Statut);

                if (preferences == null)
                {
                    // Utiliser les préférences par défaut
                    preferences = new UserNotificationPreferences
                    {
                        UserId = userId,
                        EmailNotification = true,
                        SmsNotification = true,
                        PushNotification = true,
                        InAppNotification = true
                    };
                }

                var user = await _db.Utilisateurs
                    .Include(u => u.UserDevices)
                    .FirstOrDefaultAsync(u => u.IdUtilisateur == userId);

                if (user == null) return;

                // Vérifier les heures silencieuses
                var now = DateTime.Now;
                if (preferences.QuietHoursEnabled && IsInQuietHours(now, preferences))
                {
                    _logger.LogInformation("Notification en heures silencieuses pour l'utilisateur {UserId}", userId);
                    return;
                }

                // Envoyer par email
                if (preferences.EmailNotification && !string.IsNullOrEmpty(user.EmailUtilisateur))
                {
                    try
                    {
                        // TODO: Implémenter l'email de notification générale
                        _logger.LogInformation("Email notification sent to user {UserId}", userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de l'envoi de l'email à l'utilisateur {UserId}", userId);
                    }
                }

                // Envoyer par SMS
                if (preferences.SmsNotification)
                {
                    try
                    {
                        var affilie = await _db.Affilies
                            .Include(a => a.Adhesions)
                            .ThenInclude(ad => ad.Utilisateur)
                            .FirstOrDefaultAsync(a => a.Adhesions.Any(ad => ad.Utilisateur != null && ad.Utilisateur.IdUtilisateur == userId));

                        if (affilie != null && !string.IsNullOrEmpty(affilie.Telephone))
                        {
                            await _smsService.SendSmsAsync(affilie.Telephone, $"{title}: {message}");
                            _logger.LogInformation("SMS notification sent to user {UserId}", userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de l'envoi du SMS à l'utilisateur {UserId}", userId);
                    }
                }

                // Envoyer par push
                if (preferences.PushNotification)
                {
                    try
                    {
                        await _pushService.SendPushToUserAsync(userId, title, message, new Dictionary<string, object>
                        {
                            ["type"] = type,
                            ["timestamp"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                        });
                        _logger.LogInformation("Push notification sent to user {UserId}", userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de l'envoi de la push notification à l'utilisateur {UserId}", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification aux canaux préférés de l'utilisateur {UserId}", userId);
            }
        }

        private async Task SendToAffiliePreferredChannelsAsync(int affilieId, string title, string message, string type)
        {
            try
            {
                var affilie = await _db.Affilies
                    .Include(a => a.Adhesions)
                    .ThenInclude(ad => ad.Utilisateur)
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId);

                if (affilie?.Adhesions?.Any() != true) return;

                foreach (var adhesion in affilie.Adhesions)
                {
                    if (adhesion.Utilisateur?.IdUtilisateur > 0)
                    {
                        await SendToUserPreferredChannelsAsync(adhesion.Utilisateur.IdUtilisateur, title, message, type);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification aux canaux préférés de l'affilié {AffilieId}", affilieId);
            }
        }

        private async Task CreateNotificationInDatabase(int affilieId, string title, string message, string type)
        {
            try
            {
                var affilie = await _db.Affilies
                    .Include(a => a.Adhesions)
                    .ThenInclude(ad => ad.Utilisateur)
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId);

                if (affilie?.Adhesions?.Any() != true) return;

                foreach (var adhesion in affilie.Adhesions)
                {
                    if (adhesion.Utilisateur?.IdUtilisateur > 0)
                    {
                        await CreateNotificationForUserAsync(adhesion.Utilisateur.IdUtilisateur, title, message, type);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la notification en base de données pour l'affilié {AffilieId}", affilieId);
            }
        }

        private async Task CreateNotificationForUserAsync(int userId, string title, string message, string type)
        {
            try
            {
                var notification = new Notification
                {
                    Titre = title,
                    Message = message,
                    Type = type,
                    RecepteurId = userId,
                    DateCreation = DateTime.Now
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la notification en base de données pour l'utilisateur {UserId}", userId);
            }
        }

        private bool IsInQuietHours(DateTime now, UserNotificationPreferences preferences)
        {
            var currentHour = now.Hour;
            
            if (preferences.QuietHoursStart > preferences.QuietHoursEnd)
            {
                // Cas où les heures silencieuses traversent minuit (ex: 22h à 7h)
                return currentHour >= preferences.QuietHoursStart || currentHour < preferences.QuietHoursEnd;
            }
            else
            {
                // Cas normal (ex: 23h à 6h)
                return currentHour >= preferences.QuietHoursStart && currentHour < preferences.QuietHoursEnd;
            }
        }
    }
}
