using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.DTOs.Core
{
    public class NotificationReadDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Info, Warning, Error, Success
        
        public int? EnvoyeurId { get; set; }
        
        public DateTime DateCreation { get; set; }
        
        public DateTime? DateLecture { get; set; }
        
        public bool EstLu { get; set; }
    }

    public class SendNotificationDto
    {
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success
        
        [Required]
        public int RecepteurId { get; set; }
    }

    public class NotificationStatsDto
    {
        public int Total { get; set; }
        public int Unread { get; set; }
        public int LastWeek { get; set; }
        
        [JsonExtensionData]
        public Dictionary<string, int> ByType { get; set; } = new();
    }

    public class NotificationPreferencesDto
    {
        public bool EmailNotification { get; set; } = true;
        public bool SmsNotification { get; set; } = true;
        public bool PushNotification { get; set; } = true;
        public bool InAppNotification { get; set; } = true;
        
        [StringLength(50)]
        public string Language { get; set; } = "fr";
        
        [StringLength(20)]
        public string Timezone { get; set; } = "Africa/Kinshasa";
        
        public bool QuietHoursEnabled { get; set; } = false;
        
        public int QuietHoursStart { get; set; } = 22; // 22:00
        public int QuietHoursEnd { get; set; } = 7; // 07:00
    }

    public class NotificationTemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;
        
        [Required]
        public string SujetEmail { get; set; } = string.Empty;
        
        [Required]
        public string ContenuEmail { get; set; } = string.Empty;
        
        [Required]
        [StringLength(160)]
        public string ContenuSms { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string TitrePush { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string MessagePush { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success
        
        public bool Actif { get; set; } = true;
        
        [StringLength(100)]
        public string? Description { get; set; }
    }

    public class BulkNotificationDto
    {
        [Required]
        public List<int> UserIds { get; set; } = new();
        
        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Type { get; set; } = "Info";
        
        public bool SendEmail { get; set; } = true;
        public bool SendSms { get; set; } = true;
        public bool SendPush { get; set; } = true;
        
        public DateTime? ScheduledDate { get; set; }
    }

    public class NotificationHistoryDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty; // Email, SMS, Push, InApp
        public DateTime DateEnvoi { get; set; }
        public DateTime? DateLecture { get; set; }
        public bool EstLu { get; set; }
        public bool EstEnvoye { get; set; }
        public string? Erreur { get; set; }
    }
}
