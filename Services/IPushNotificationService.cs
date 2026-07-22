using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace ProsocAPI.Services
{
    public interface IPushNotificationService
    {
        Task SendPushNotificationAsync(string deviceToken, string title, string message, Dictionary<string, object>? data = null);
        Task SendPushToUserAsync(int userId, string title, string message, Dictionary<string, object>? data = null);
        Task SendPushToAffilieAsync(int affilieId, string title, string message, Dictionary<string, object>? data = null);
        Task SendAdhesionConfirmationPushAsync(int affilieId, string affilieName, string codeAdhesion);
        Task SendPaymentReminderPushAsync(int affilieId, decimal amount, DateTime dueDate);
        Task SendAppointmentReminderPushAsync(int affilieId, DateTime appointmentDate, string location);
    }

    public class PushNotificationService : IPushNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly ProsocDbContext _db;

        public PushNotificationService(
            IConfiguration configuration, 
            ILogger<PushNotificationService> logger,
            ProsocDbContext db)
        {
            _configuration = configuration;
            _logger = logger;
            _db = db;
        }

        public async Task SendPushNotificationAsync(string deviceToken, string title, string message, Dictionary<string, object>? data = null)
        {
            try
            {
                var pushSettings = _configuration.GetSection("PushNotificationSettings");
                
                var serverKey = pushSettings["FcmServerKey"];
                var apiUrl = pushSettings["FcmApiUrl"];

                var notificationPayload = new
                {
                    to = deviceToken,
                    notification = new
                    {
                        title = title,
                        body = message,
                        sound = "default",
                        badge = 1,
                        icon = "ic_notification"
                    },
                    data = data ?? new Dictionary<string, object>()
                };

                var jsonPayload = JsonSerializer.Serialize(notificationPayload);
                
                // TODO: Implémenter l'appel API FCM réel
                // Pour l'instant, simulation
                _logger.LogInformation("Push notification envoyée à {DeviceToken}: {Title} - {Message}", deviceToken, title, message);
                
                // Simulation d'envoi push
                await Task.Delay(100);
                
                _logger.LogInformation("Push notification envoyée avec succès à {DeviceToken}", deviceToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la push notification à {DeviceToken}", deviceToken);
                throw;
            }
        }

        public async Task SendPushToUserAsync(int userId, string title, string message, Dictionary<string, object>? data = null)
        {
            var userDevices = await _db.Utilisateurs
                .Where(u => u.IdUtilisateur == userId)
                .SelectMany(u => u.UserDevices ?? new List<Models.Authentication.UserDevice>())
                .Where(d => d.FcmToken != null)
                .ToListAsync();

            foreach (var device in userDevices)
            {
                await SendPushNotificationAsync(device.FcmToken!, title, message, data);
            }
        }

        public async Task SendPushToAffilieAsync(int affilieId, string title, string message, Dictionary<string, object>? data = null)
        {
            var affilie = await _db.Affilies
                .Include(a => a.Adhesions)
                .ThenInclude(ad => ad.Utilisateur)
                .FirstOrDefaultAsync(a => a.IdAffilie == affilieId);

            if (affilie?.Adhesions?.Any() == true)
            {
                var userId = affilie.Adhesions.First().Utilisateur?.IdUtilisateur;
                if (userId.HasValue)
                {
                    await SendPushToUserAsync(userId.Value, title, message, data);
                }
            }
        }

        public async Task SendAdhesionConfirmationPushAsync(int affilieId, string affilieName, string codeAdhesion)
        {
            var title = "🎉 Adhésion Confirmée";
            var message = $"Félicitations {affilieName}! Votre adhésion N°{codeAdhesion} est active.";
            var data = new Dictionary<string, object>
            {
                ["type"] = "adhesion_confirmation",
                ["codeAdhesion"] = codeAdhesion,
                ["affilieId"] = affilieId
            };

            await SendPushToAffilieAsync(affilieId, title, message, data);
        }

        public async Task SendPaymentReminderPushAsync(int affilieId, decimal amount, DateTime dueDate)
        {
            var title = "💰 Rappel de Paiement";
            var message = $"Votre cotisation de {amount:CDF} est due pour le {dueDate:dd/MM/yyyy}";
            var data = new Dictionary<string, object>
            {
                ["type"] = "payment_reminder",
                ["amount"] = amount,
                ["dueDate"] = dueDate.ToString("yyyy-MM-dd"),
                ["affilieId"] = affilieId
            };

            await SendPushToAffilieAsync(affilieId, title, message, data);
        }

        public async Task SendAppointmentReminderPushAsync(int affilieId, DateTime appointmentDate, string location)
        {
            var title = "📅 Rappel de Rendez-vous";
            var message = $"Rendez-vous le {appointmentDate:dd/MM/yyyy à HH:mm} à {location}";
            var data = new Dictionary<string, object>
            {
                ["type"] = "appointment_reminder",
                ["appointmentDate"] = appointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                ["location"] = location,
                ["affilieId"] = affilieId
            };

            await SendPushToAffilieAsync(affilieId, title, message, data);
        }
    }
}
