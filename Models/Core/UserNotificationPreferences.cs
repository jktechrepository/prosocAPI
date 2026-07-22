using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class UserNotificationPreferences
    {
        [Key]
        public int IdUserNotificationPreference { get; set; }  // ✅ Standardisé
        
        [Required]
        public int UserId { get; set; }
        
        public bool EmailNotification { get; set; } = true;
        
        public bool SmsNotification { get; set; } = true;
        
        public bool PushNotification { get; set; } = true;
        
        public bool InAppNotification { get; set; } = true;
        
        // Préférences spécifiques aux commissions (pour les agents)
        public bool CommissionEmail { get; set; } = true;
        
        public bool CommissionSms { get; set; } = false;
        
        public bool CommissionPush { get; set; } = true;
        
        public bool CommissionInApp { get; set; } = true;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinCommissionAmount { get; set; } = 1.0m; // Seuil minimum de notification
        
        [StringLength(20)]
        public string CommissionCurrency { get; set; } = "USD";
        
        [StringLength(500)]
        public string? CommissionMessageTemplate { get; set; } // Template personnalisé
        
        [StringLength(50)]
        public string Language { get; set; } = "fr";
        
        [StringLength(50)]
        public string Timezone { get; set; } = "Africa/Kinshasa";
        
        public bool QuietHoursEnabled { get; set; } = false;
        
        public int QuietHoursStart { get; set; } = 22; // 22:00
        
        public int QuietHoursEnd { get; set; } = 7; // 07:00
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;

        [ForeignKey("UserId")]
        [NotMapped]
        public virtual Utilisateur User { get; set; } = null!;
    }
}
