using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Mobile
{
    /// <summary>
    /// Session utilisateur mobile pour le suivi et l'authentification
    /// </summary>
    public class MobileUserSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UtilisateurId { get; set; }
        
        [Required, StringLength(500)]
        public string SessionToken { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string DeviceId { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        public string Platform { get; set; } = string.Empty; // Android, iOS
        
        [StringLength(100)]
        public string? AppVersion { get; set; }
        
        [StringLength(100)]
        public string? OsVersion { get; set; }
        
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        [StringLength(100)]
        public string? UserAgent { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime DateDerniereActivite { get; set; } = DateTime.Now;
        
        public DateTime DateExpiration { get; set; }
        
        public bool EstActive { get; set; } = true;
        
        public bool EstBiometricAuth { get; set; } = false;
        
        public int NombreRequetes { get; set; } = 0;
        
        [StringLength(1000)]
        public string? Metadata { get; set; } // JSON avec infos additionnelles
        
        public DateTime? DateDerniereSynchronisation { get; set; }
        
        public bool EstModeHorsLigne { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("UtilisateurId")]
        public virtual Authentication.Utilisateur Utilisateur { get; set; } = null!;
    }

    /// <summary>
    /// Statuts de session mobile
    /// </summary>
    public static class MobileSessionStatus
    {
        public const string ACTIVE = "ACTIVE";
        public const string EXPIRED = "EXPIRED";
        public const string TERMINATED = "TERMINATED";
        public const string SUSPENDED = "SUSPENDED";
        public const string OFFLINE = "OFFLINE";
    }

    /// <summary>
    /// Types d'authentification mobile
    /// </summary>
    public static class MobileAuthTypes
    {
        public const string PASSWORD = "PASSWORD";
        public const string BIOMETRIC = "BIOMETRIC";
        public const string FACE_ID = "FACE_ID";
        public const string TOUCH_ID = "TOUCH_ID";
        public const string FINGERPRINT = "FINGERPRINT";
        public const string TWO_FACTOR = "TWO_FACTOR";
        public const string SSO = "SSO";
    }
}
