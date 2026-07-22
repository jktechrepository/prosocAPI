using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Mobile
{
    /// <summary>
    /// Données de synchronisation mobile pour le mode hors ligne
    /// </summary>
    public class MobileSyncData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UtilisateurId { get; set; }
        
        [Required, StringLength(50)]
        public string EntityType { get; set; } = string.Empty; // Notification, Commission, Profile, etc.
        
        [Required]
        public int EntityId { get; set; } // ID de l'entité concernée
        
        [Required, StringLength(20)]
        public string Operation { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
        
        [Column(TypeName = "json")]
        public string Data { get; set; } = string.Empty; // Données JSON de l'entité
        
        [StringLength(50)]
        public string? SyncStatus { get; set; } = "PENDING"; // PENDING, SYNCED, FAILED
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateSynchronisation { get; set; }
        
        public DateTime? DateDerniereTentative { get; set; }
        
        public int NombreTentatives { get; set; } = 0;
        
        [StringLength(1000)]
        public string? ErreurMessage { get; set; }
        
        public bool EstSynchronise { get; set; } = false;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("UtilisateurId")]
        public virtual Authentication.Utilisateur Utilisateur { get; set; } = null!;
    }

    /// <summary>
    /// Opérations de synchronisation
    /// </summary>
    public static class MobileSyncOperations
    {
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
        public const string READ = "READ";
        public const string SYNC = "SYNC";
        public const string UPLOAD = "UPLOAD";
        public const string DOWNLOAD = "DOWNLOAD";
    }

    /// <summary>
    /// Statuts de synchronisation
    /// </summary>
    public static class MobileSyncStatus
    {
        public const string PENDING = "PENDING";
        public const string SYNCING = "SYNCING";
        public const string SYNCED = "SYNCED";
        public const string FAILED = "FAILED";
        public const string CONFLICT = "CONFLICT";
        public const string SKIPPED = "SKIPPED";
    }

    /// <summary>
    /// Types d'entités pour synchronisation
    /// </summary>
    public static class MobileSyncEntities
    {
        public const string NOTIFICATION = "Notification";
        public const string COMMISSION = "Commission";
        public const string PROFILE = "Profile";
        public const string PREFERENCE = "Preference";
        public const string WALLET = "Wallet";
        public const string ADHESION = "Adhesion";
        public const string COLLECTE = "Collecte";
        public const string RETRAIT = "Retrait";
        public const string MESSAGE = "Message";
        public const string DOCUMENT = "Document";
    }
}
