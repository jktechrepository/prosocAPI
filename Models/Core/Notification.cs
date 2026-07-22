using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Core
{
    public class Notification
    {
        [Key]
        public int IdNotification { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(200)]
        public string Titre { get; set; } = string.Empty;
        
        [Required, StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Code du type (COMMISSION, ADHESION, etc.)
        
        public int? TypeNotificationId { get; set; }
        
        [ForeignKey("TypeNotificationId")]
        public virtual NotificationType? TypeNotification { get; set; }
        
        public int? EnvoyeurId { get; set; }
        
        public int? RecepteurId { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateLecture { get; set; }
        
        public bool EstLu { get; set; } = false;
        
        [StringLength(20)]
        public string Priorite { get; set; } = "Normale"; // Basse, Normale, Haute, Critique
        
        [StringLength(50)]
        public string Categorie { get; set; } = "BUSINESS"; // BUSINESS, SYSTEME, MARKETING, etc.
        
        public string Couleur { get; set; } = "#007bff";
        
        public string Icône { get; set; } = "bell";
        
        [StringLength(1000)]
        public string? Métadonnées { get; set; } // JSON string pour données additionnelles
        
        public DateTime? DateEnvoiEmail { get; set; }
        
        public DateTime? DateEnvoiSms { get; set; }
        
        public DateTime? DateEnvoiPush { get; set; }
        
        public bool EmailEnvoyé { get; set; } = false;
        
        public bool SmsEnvoyé { get; set; } = false;
        
        public bool PushEnvoyé { get; set; } = false;
        
        public bool Statut { get; set; } = true;

        [ForeignKey("EnvoyeurId")]
        [JsonIgnore]
        [NotMapped]
        public virtual Authentication.Utilisateur? Envoyeur { get; set; }

        [ForeignKey("RecepteurId")]
        [JsonIgnore]
        [NotMapped]
        public virtual Authentication.Utilisateur? Recepteur { get; set; }
    }
}
