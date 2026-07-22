using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public class CommissionNotificationService : ICommissionNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly ProsocDbContext _db;
        private readonly ILogger<CommissionNotificationService> _logger;
        private const decimal MIN_COMMISSION_THRESHOLD = 1.0m; // Seuil minimum de 1$

        public CommissionNotificationService(
            INotificationService notificationService,
            ProsocDbContext db,
            ILogger<CommissionNotificationService> logger)
        {
            _notificationService = notificationService;
            _db = db;
            _logger = logger;
        }

        public async Task NotifyCommissionEarnedAsync(
            int agentId, 
            decimal commissionAmount, 
            Collecte collecte,
            decimal ancienSolde,
            decimal nouveauSolde,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("=== DÉBUT NOTIFICATION COMMISSION ===");
                _logger.LogInformation("AgentId: {AgentId}, Commission: {Commission}", agentId, commissionAmount);

                // 1. Récupérer les informations de l'agent
                var agent = await _db.Agents
                    .FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);

                if (agent == null)
                {
                    _logger.LogWarning("Agent {AgentId} non trouvé", agentId);
                    return;
                }

                // 2. Récupérer l'utilisateur associé à l'agent
                var utilisateur = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.AgentId == agentId, ct);

                if (utilisateur == null)
                {
                    _logger.LogWarning("Aucun utilisateur trouvé pour l'agent {AgentId}", agentId);
                    return;
                }

                // 3. Récupérer les préférences de notification de l'utilisateur
                var preferences = await _db.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == utilisateur.IdUtilisateur, ct);

                // 4. Appliquer les préférences spécifiques aux commissions
                if (preferences != null)
                {
                    // Vérifier le seuil minimum personnalisé
                    if (commissionAmount < preferences.MinCommissionAmount)
                    {
                        _logger.LogInformation("Commission {Commission} inférieure au seuil personnalisé {Threshold}, notification ignorée", 
                            commissionAmount, preferences.MinCommissionAmount);
                        return;
                    }

                    // Vérifier les heures de silence
                    if (preferences.QuietHoursEnabled && IsInQuietHours(preferences))
                    {
                        _logger.LogInformation("Heures de silence activées, notification reportée");
                        return;
                    }

                    // Envoyer selon les préférences spécifiques aux commissions
                    await SendCommissionNotificationWithPreferences(
                        utilisateur.IdUtilisateur, 
                        agent, 
                        collecte, 
                        commissionAmount, 
                        ancienSolde, 
                        nouveauSolde, 
                        preferences, 
                        ct);
                }
                else
                {
                    // Utiliser les préférences par défaut si aucune préférence n'existe
                    await SendCommissionNotificationDefault(
                        utilisateur.IdUtilisateur, 
                        agent, 
                        collecte, 
                        commissionAmount, 
                        ancienSolde, 
                        nouveauSolde, 
                        ct);
                }

                _logger.LogInformation("Notification commission envoyée avec succès à l'agent {AgentId}", agentId);
                _logger.LogInformation("=== FIN NOTIFICATION COMMISSION ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification commission pour l'agent {AgentId}", agentId);
                // Ne pas lever d'exception pour ne pas bloquer le processus de commission
            }
        }

        private string CreateCommissionMessage(
            Agent agent, 
            Affilie? affilie, 
            Collecte collecte, 
            decimal commissionAmount,
            decimal ancienSolde,
            decimal nouveauSolde,
            UserNotificationPreferences? preferences = null)
        {
            var affilieNom = affilie?.NomComplet ?? $"Affilié #{collecte.AffilieId}";
            var devise = collecte.Devise?.Code ?? "USD";
            
            // Utiliser le template personnalisé si disponible
            if (preferences?.CommissionMessageTemplate != null)
            {
                return ApplyTemplate(preferences.CommissionMessageTemplate, agent, affilieNom, collecte, commissionAmount, ancienSolde, nouveauSolde, devise);
            }
            
            return $"Félicitations {agent.NomComplet} !\n\n" +
                   $"Vous avez reçu une commission de {commissionAmount:N2} {devise}\n" +
                   $"pour la collecte de {collecte.Montant:N2} {devise} effectuée par {affilieNom}.\n\n" +
                   $"💰 Solde du wallet : {ancienSolde:N2} {devise} → {nouveauSolde:N2} {devise}\n\n" +
                   $"Continuez votre excellent travail ! 🚀";
        }

        private string ApplyTemplate(string template, Agent agent, string affilieNom, Collecte collecte, decimal commissionAmount, decimal ancienSolde, decimal nouveauSolde, string devise)
        {
            return template
                .Replace("{AgentName}", agent.NomComplet)
                .Replace("{AffilieName}", affilieNom)
                .Replace("{CommissionAmount}", commissionAmount.ToString("N2"))
                .Replace("{CollecteAmount}", collecte.Montant.ToString("N2"))
                .Replace("{OldBalance}", ancienSolde.ToString("N2"))
                .Replace("{NewBalance}", nouveauSolde.ToString("N2"))
                .Replace("{Currency}", devise)
                .Replace("{CollecteId}", collecte.IdCollecte.ToString());
        }

        private async Task SendCommissionNotificationWithPreferences(
            int userId,
            Agent agent,
            Collecte collecte,
            decimal commissionAmount,
            decimal ancienSolde,
            decimal nouveauSolde,
            UserNotificationPreferences preferences,
            CancellationToken ct)
        {
            var affilie = await _db.Affilies
                .FirstOrDefaultAsync(a => a.IdAffilie == collecte.AffilieId, ct);

            var titre = "🎉 Commission Reçue !";
            var message = CreateCommissionMessage(agent, affilie, collecte, commissionAmount, ancienSolde, nouveauSolde);

            // Envoyer selon les préférences spécifiques aux commissions
            var channels = new List<string>();
            if (preferences.CommissionEmail) channels.Add("email");
            if (preferences.CommissionSms) channels.Add("sms");
            if (preferences.CommissionPush) channels.Add("push");
            if (preferences.CommissionInApp) channels.Add("inapp");

            if (channels.Any())
            {
                await _notificationService.SendCustomNotificationAsync(userId, titre, message, "COMMISSION");
            }

            if (preferences.CommissionInApp)
            {
                await CreateNotificationInDatabaseAsync(userId, titre, message, "COMMISSION", ct);
            }

            _logger.LogInformation("Notification commission envoyée selon les préférences de l'utilisateur {UserId}", userId);
        }

        private async Task SendCommissionNotificationDefault(
            int userId,
            Agent agent,
            Collecte collecte,
            decimal commissionAmount,
            decimal ancienSolde,
            decimal nouveauSolde,
            CancellationToken ct)
        {
            var affilie = await _db.Affilies
                .FirstOrDefaultAsync(a => a.IdAffilie == collecte.AffilieId, ct);

            var titre = "🎉 Commission Reçue !";
            var message = CreateCommissionMessage(agent, affilie, collecte, commissionAmount, ancienSolde, nouveauSolde);

            // Utiliser le service unifié avec les préférences générales
            await _notificationService.SendToUserPreferredChannelsAsync(userId, titre, message, "COMMISSION");

            // Créer la notification en base pour l'historique
            await CreateNotificationInDatabaseAsync(userId, titre, message, "COMMISSION", ct);

            _logger.LogInformation("Notification commission envoyée avec les préférences par défaut à l'utilisateur {UserId}", userId);
        }

        private bool IsInQuietHours(UserNotificationPreferences preferences)
        {
            var now = DateTime.Now;
            var currentHour = now.Hour;

            // Vérifier si l'heure actuelle est dans la plage de silence
            if (preferences.QuietHoursStart <= preferences.QuietHoursEnd)
            {
                // Cas normal : 22:00 à 07:00
                return currentHour >= preferences.QuietHoursStart && currentHour <= preferences.QuietHoursEnd;
            }
            else
            {
                // Cas minuit : 22:00 à 23:59 ET 00:00 à 07:00
                return currentHour >= preferences.QuietHoursStart || currentHour <= preferences.QuietHoursEnd;
            }
        }

        private async Task CreateNotificationInDatabaseAsync(
            int userId,
            string titre,
            string message,
            string type,
            CancellationToken ct)
        {
            try
            {
                var notification = new Notification
                {
                    Titre = titre,
                    Message = message,
                    Type = type,
                    RecepteurId = userId,
                    DateCreation = DateTime.Now,
                    EstLu = false
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Notification créée en base : Id {NotificationId}", notification.IdNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la notification en base pour l'utilisateur {UserId}", userId);
            }
        }
    }
}
