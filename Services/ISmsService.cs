using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProsocAPI.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendAdhesionConfirmationSmsAsync(string phoneNumber, string affilieName, string codeAdhesion);
        Task SendPaymentReminderSmsAsync(string phoneNumber, string affilieName, decimal amount, DateTime dueDate);
        Task SendAppointmentReminderSmsAsync(string phoneNumber, string affilieName, DateTime appointmentDate, string location);
    }

    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var smsSettings = _configuration.GetSection("SmsSettings");
                
                var apiKey = smsSettings["ApiKey"];
                var apiUrl = smsSettings["ApiUrl"];
                var senderName = smsSettings["SenderName"];

                // TODO: Implémenter l'appel API SMS réel (Twilio, Orange SMS, etc.)
                // Pour l'instant, simulation
                _logger.LogInformation("SMS envoyé à {PhoneNumber}: {Message}", phoneNumber, message);
                
                // Simulation d'envoi SMS
                await Task.Delay(100);
                
                _logger.LogInformation("SMS envoyé avec succès à {PhoneNumber}", phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi du SMS à {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task SendAdhesionConfirmationSmsAsync(string phoneNumber, string affilieName, string codeAdhesion)
        {
            var message = $"PROSOC: Bienvenue {affilieName}! Votre adhésion N°{codeAdhesion} est confirmée. Conservez ce code pour toutes vos démarches. Merci de votre confiance!";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPaymentReminderSmsAsync(string phoneNumber, string affilieName, decimal amount, DateTime dueDate)
        {
            var message = $"PROSOC: Cher {affilieName}, votre cotisation de {amount:CDF} est due pour le {dueDate:dd/MM/yyyy}. Merci de régulariser votre situation.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendAppointmentReminderSmsAsync(string phoneNumber, string affilieName, DateTime appointmentDate, string location)
        {
            var message = $"PROSOC: Rappel {affilieName}! Rendez-vous le {appointmentDate:dd/MM/yyyy à HH:mm} à {location}. Présentez votre code d'adhésion.";
            await SendSmsAsync(phoneNumber, message);
        }
    }
}
