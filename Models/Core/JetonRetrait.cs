using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class JetonRetrait
    {
        [Key]
        public int IdJeton { get; set; }
        
        [Required]
        public int AgentId { get; set; }
        
        [Required]
        public int DemandeRetraitId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string CodeJeton { get; set; } = string.Empty; // Format: JRT + 8 caractères
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantRetrait { get; set; }
        
        public DateTime DateEmission { get; set; } = DateTime.Now;
        
        public DateTime? DateUtilisation { get; set; }
        
        [Required]
        public DateTime DateExpiration { get; set; } = DateTime.Now.AddDays(7); // Valide 7 jours
        
        public bool EstValide { get; set; } = true;
        
        public bool EstUtilise { get; set; } = false;
        
        [StringLength(500)]
        public string? ObservationUtilisation { get; set; }

        public int? OperateurUtilisateurId { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        // Navigation Properties
        [ForeignKey("AgentId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;
        
        [ForeignKey("DemandeRetraitId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual DemandeRetraitAgent DemandeRetrait { get; set; } = null!;

        [ForeignKey("OperateurUtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? OperateurUtilisateur { get; set; }
    }
}
