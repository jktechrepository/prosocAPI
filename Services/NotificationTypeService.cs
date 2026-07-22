using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using System.Text.Json;

namespace ProsocAPI.Services
{
    public interface INotificationTypeService
    {
        Task<List<NotificationType>> GetAllAsync();
        Task<NotificationType?> GetByCodeAsync(string code);
        Task<NotificationType> CreateAsync(NotificationType type);
        Task<NotificationType> UpdateAsync(NotificationType type);
        Task<bool> DeleteAsync(int id);
        Task<List<NotificationType>> GetByCategoryAsync(string category);
        Task<Notification> CreateNotificationAsync(string typeCode, int recepteurId, string titre, string message, object? metadata = null, int? envoyeurId = null);
        Task<List<Notification>> GetNotificationsByTypeAsync(string typeCode, int userId);
        Task SeedDefaultTypesAsync();
    }

    public class NotificationTypeService : INotificationTypeService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<NotificationTypeService> _logger;

        public NotificationTypeService(ProsocDbContext db, ILogger<NotificationTypeService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<NotificationType>> GetAllAsync()
        {
            return await _db.NotificationTypes
                .Where(t => t.EstActif && t.Statut)
                .OrderBy(t => t.Categorie)
                .ThenBy(t => t.Priorite)
                .ThenBy(t => t.Nom)
                .ToListAsync();
        }

        public async Task<NotificationType?> GetByCodeAsync(string code)
        {
            return await _db.NotificationTypes
                .FirstOrDefaultAsync(t => t.Code == code && t.EstActif && t.Statut);
        }

        public async Task<NotificationType> CreateAsync(NotificationType type)
        {
            type.DateCreation = DateTime.Now;
            type.Statut = true;
            type.EstActif = true;

            _db.NotificationTypes.Add(type);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Type de notification créé: {Code}", type.Code);
            return type;
        }

        public async Task<NotificationType> UpdateAsync(NotificationType type)
        {
            type.DateModification = DateTime.Now;

            _db.NotificationTypes.Update(type);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Type de notification mis à jour: {Code}", type.Code);
            return type;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var type = await _db.NotificationTypes.FindAsync(id);
            if (type == null)
                return false;

            type.EstActif = false;
            type.DateModification = DateTime.Now;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Type de notification désactivé: {Code}", type.Code);
            return true;
        }

        public async Task<List<NotificationType>> GetByCategoryAsync(string category)
        {
            return await _db.NotificationTypes
                .Where(t => t.Categorie == category && t.EstActif && t.Statut)
                .OrderBy(t => t.Priorite)
                .ThenBy(t => t.Nom)
                .ToListAsync();
        }

        public async Task<Notification> CreateNotificationAsync(
            string typeCode, 
            int recepteurId, 
            string titre, 
            string message, 
            object? metadata = null, 
            int? envoyeurId = null)
        {
            // Récupérer le type de notification
            var type = await GetByCodeAsync(typeCode);
            if (type == null)
            {
                _logger.LogWarning("Type de notification non trouvé: {Code}", typeCode);
                throw new ArgumentException($"Type de notification '{typeCode}' non trouvé");
            }

            // Créer la notification
            var notification = new Notification
            {
                Titre = titre,
                Message = message,
                Type = typeCode,
                TypeNotificationId = type.IdNotificationType,
                RecepteurId = recepteurId,
                EnvoyeurId = envoyeurId,
                Priorite = GetPrioritéText(type.Priorite),
                Categorie = type.Categorie,
                Couleur = type.Couleur,
                Icône = type.Icône,
                Métadonnées = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                DateCreation = DateTime.Now,
                EstLu = false
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Notification créée: {Type} pour l'utilisateur {UserId}", typeCode, recepteurId);
            return notification;
        }

        public async Task<List<Notification>> GetNotificationsByTypeAsync(string typeCode, int userId)
        {
            return await _db.Notifications
                .Where(n => n.Type == typeCode && n.RecepteurId == userId)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public async Task SeedDefaultTypesAsync()
        {
            var existingTypes = await _db.NotificationTypes.ToListAsync();
            var existingCodes = existingTypes.Select(t => t.Code).ToHashSet();

            var defaultTypes = GetDefaultNotificationTypes();

            foreach (var type in defaultTypes)
            {
                if (!existingCodes.Contains(type.Code))
                {
                    await CreateAsync(type);
                }
            }
        }

        private List<NotificationType> GetDefaultNotificationTypes()
        {
            return new List<NotificationType>
            {
                // Business
                new NotificationType
                {
                    Code = NotificationTypes.COMMISSION,
                    Nom = "Commission Reçue",
                    Description = "Notification lorsqu'un agent reçoit une commission",
                    Categorie = NotificationCategories.BUSINESS,
                    Couleur = "#28a745",
                    Icône = "dollar-sign",
                    Priorite = NotificationPriorities.NORMALE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = true,
                    InAppParDefaut = true,
                    TemplateMessage = "Félicitations {AgentName} ! Vous avez reçu une commission de {CommissionAmount} {Currency} pour la collecte de {CollecteAmount} {Currency} effectuée par {AffilieName}."
                },
                new NotificationType
                {
                    Code = NotificationTypes.ADHESION,
                    Nom = "Nouvelle Adhésion",
                    Description = "Notification lors d'une nouvelle adhésion",
                    Categorie = NotificationCategories.BUSINESS,
                    Couleur = "#007bff",
                    Icône = "user-plus",
                    Priorite = NotificationPriorities.NORMALE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = true,
                    InAppParDefaut = true
                },
                new NotificationType
                {
                    Code = NotificationTypes.PAIEMENT,
                    Nom = "Paiement Reçu",
                    Description = "Notification lors d'un paiement reçu",
                    Categorie = NotificationCategories.BUSINESS,
                    Couleur = "#28a745",
                    Icône = "credit-card",
                    Priorite = NotificationPriorities.NORMALE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = true,
                    InAppParDefaut = true
                },
                new NotificationType
                {
                    Code = NotificationTypes.RETRAIT,
                    Nom = "Demande de Retrait",
                    Description = "Notification lors d'une demande de retrait",
                    Categorie = NotificationCategories.BUSINESS,
                    Couleur = "#ffc107",
                    Icône = "money-bill-wave",
                    Priorite = NotificationPriorities.HAUTE,
                    EmailParDefaut = true,
                    SmsParDefaut = true,
                    PushParDefaut = true,
                    InAppParDefaut = true
                },

                // Système
                new NotificationType
                {
                    Code = NotificationTypes.COMPTE_CRÉÉ,
                    Nom = "Compte Créé",
                    Description = "Notification de création de compte",
                    Categorie = NotificationCategories.SYSTÈME,
                    Couleur = "#007bff",
                    Icône = "user-plus",
                    Priorite = NotificationPriorities.NORMALE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = false,
                    InAppParDefaut = true
                },
                new NotificationType
                {
                    Code = NotificationTypes.MOT_DE_PASSE,
                    Nom = "Mot de Passe",
                    Description = "Notification liée au mot de passe",
                    Categorie = NotificationCategories.SÉCURITÉ,
                    Couleur = "#dc3545",
                    Icône = "key",
                    Priorite = NotificationPriorities.HAUTE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = false,
                    InAppParDefaut = true
                },
                new NotificationType
                {
                    Code = NotificationTypes.CONNEXION,
                    Nom = "Connexion",
                    Description = "Notification de connexion",
                    Categorie = NotificationCategories.SÉCURITÉ,
                    Couleur = "#6c757d",
                    Icône = "sign-in-alt",
                    Priorite = NotificationPriorities.BASSE,
                    EmailParDefaut = false,
                    SmsParDefaut = false,
                    PushParDefaut = false,
                    InAppParDefaut = true
                },

                // Marketing
                new NotificationType
                {
                    Code = NotificationTypes.PROMOTION,
                    Nom = "Promotion",
                    Description = "Notification promotionnelle",
                    Categorie = NotificationCategories.MARKETING,
                    Couleur = "#e83e8c",
                    Icône = "tag",
                    Priorite = NotificationPriorities.BASSE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = true,
                    InAppParDefaut = true
                },
                new NotificationType
                {
                    Code = NotificationTypes.RAPPEL,
                    Nom = "Rappel",
                    Description = "Notification de rappel",
                    Categorie = NotificationCategories.MARKETING,
                    Couleur = "#fd7e14",
                    Icône = "bell",
                    Priorite = NotificationPriorities.NORMALE,
                    EmailParDefaut = true,
                    SmsParDefaut = true,
                    PushParDefaut = true,
                    InAppParDefaut = true
                },

                // Performance
                new NotificationType
                {
                    Code = NotificationTypes.OBJECTIF_ATTEINT,
                    Nom = "Objectif Atteint",
                    Description = "Notification lorsqu'un objectif est atteint",
                    Categorie = NotificationCategories.PERFORMANCE,
                    Couleur = "#28a745",
                    Icône = "trophy",
                    Priorite = NotificationPriorities.HAUTE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = true,
                    InAppParDefaut = true
                },
                new NotificationType
                {
                    Code = NotificationTypes.MILESTONE,
                    Nom = "Milestone",
                    Description = "Notification d'un milestone important",
                    Categorie = NotificationCategories.PERFORMANCE,
                    Couleur = "#17a2b8",
                    Icône = "flag",
                    Priorite = NotificationPriorities.NORMALE,
                    EmailParDefaut = true,
                    SmsParDefaut = false,
                    PushParDefaut = true,
                    InAppParDefaut = true
                }
            };
        }

        private string GetPrioritéText(int priorité)
        {
            return priorité switch
            {
                NotificationPriorities.BASSE => "Basse",
                NotificationPriorities.NORMALE => "Normale",
                NotificationPriorities.HAUTE => "Haute",
                NotificationPriorities.CRITIQUE => "Critique",
                _ => "Normale"
            };
        }
    }
}
