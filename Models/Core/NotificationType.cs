using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Types de notifications disponibles dans le système
    /// </summary>
    public class NotificationType
    {
        [Key]
        public int IdNotificationType { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(50)]
        public string Code { get; set; } = string.Empty; // COMMISSION, ADHESION, PAIEMENT, etc.
        
        [Required, StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required, StringLength(20)]
        public string Categorie { get; set; } = string.Empty; // BUSINESS, SYSTEME, MARKETING, SECURITE
        
        public string Couleur { get; set; } = "#007bff"; // Couleur pour l'UI
        
        public string Icône { get; set; } = "bell"; // Icône Font Awesome
        
        public bool EstActif { get; set; } = true;
        
        public int Priorite { get; set; } = 1; // 0=Basse, 1=Normale, 2=Haute, 3=Critique
        
        public bool EmailParDefaut { get; set; } = true;
        
        public bool SmsParDefaut { get; set; } = false;
        
        public bool PushParDefaut { get; set; } = true;
        
        public bool InAppParDefaut { get; set; } = true;
        
        [StringLength(1000)]
        public string? TemplateMessage { get; set; } // Template par défaut
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        // Navigation properties
        [InverseProperty("TypeNotification")]
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    /// <summary>
    /// Constantes pour les types de notifications
    /// </summary>
    public static class NotificationTypes
    {
        // Business
        public const string COMMISSION = "COMMISSION";
        public const string ADHESION = "ADHESION";
        public const string PAIEMENT = "PAIEMENT";
        public const string RETRAIT = "RETRAIT";
        public const string BONUS = "BONUS";
        
        // Système
        public const string COMPTE_CRÉÉ = "COMPTE_CRÉÉ";
        public const string MOT_DE_PASSE = "MOT_DE_PASSE";
        public const string CONNEXION = "CONNEXION";
        public const string SÉCURITÉ = "SÉCURITÉ";
        
        // Marketing
        public const string PROMOTION = "PROMOTION";
        public const string INFOLETTRE = "INFOLETTRE";
        public const string RAPPEL = "RAPPEL";
        
        // Support
        public const string TICKET = "TICKET";
        public const string MESSAGE = "MESSAGE";
        public const string ALERTE = "ALERTE";
        
        // Rendez-vous
        public const string RENDEZ_VOUS = "RENDEZ_VOUS";
        public const string RAPPEL_RDV = "RAPPEL_RDV";
        public const string ANNULATION_RDV = "ANNULATION_RDV";
        
        // Performance
        public const string OBJECTIF_ATTEINT = "OBJECTIF_ATTEINT";
        public const string MILESTONE = "MILESTONE";
        public const string RECOMPENSE = "RECOMPENSE";
    }

    /// <summary>
    /// Catégories de notifications
    /// </summary>
    public static class NotificationCategories
    {
        public const string BUSINESS = "BUSINESS";
        public const string SYSTÈME = "SYSTEME";
        public const string MARKETING = "MARKETING";
        public const string SÉCURITÉ = "SECURITE";
        public const string SUPPORT = "SUPPORT";
        public const string PERFORMANCE = "PERFORMANCE";
    }

    /// <summary>
    /// Priorités de notifications
    /// </summary>
    public static class NotificationPriorities
    {
        public const int BASSE = 0;
        public const int NORMALE = 1;
        public const int HAUTE = 2;
        public const int CRITIQUE = 3;
    }
}
