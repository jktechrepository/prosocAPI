using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class DemandeRetraitAgent
    {
        [Key]
        public int IdDemande { get; set; }
        
        [Required]
        public int AgentId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantDemande { get; set; }
        
        [Required]
        [StringLength(20)]
        public string TypeRetrait { get; set; } = string.Empty; // "PARTIEL", "TOTAL"
        
        [Required]
        [StringLength(20)]
        public string StatutDemande { get; set; } = "EN_ATTENTE"; // "EN_ATTENTE", "VALIDEE", "REJETEE", "TRAITEE"
        
        [StringLength(500)]
        public string? MotifRetrait { get; set; }
        
        [StringLength(500)]
        public string? MotifRejet { get; set; }
        
        public DateTime DateDemande { get; set; } = DateTime.Now;
        
        public DateTime? DateValidation { get; set; }
        
        public DateTime? DateTraitement { get; set; }
        
        public int? AgentValidationId { get; set; } // Agent superviseur qui valide
        
        public int? JetonRetraitId { get; set; }

        public int? OperateurPaiementUtilisateurId { get; set; }

        public int? WalletMouvementId { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        // Navigation Properties
        [ForeignKey("AgentId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;
        
        [ForeignKey("AgentValidationId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent? AgentValidation { get; set; }
        
        [ForeignKey("JetonRetraitId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual JetonRetrait? JetonRetrait { get; set; }

        [ForeignKey("OperateurPaiementUtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? OperateurPaiement { get; set; }

        [ForeignKey("WalletMouvementId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletMouvement? WalletMouvement { get; set; }
    }
}
