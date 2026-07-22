using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class DemandeBonEnvoi
    {
        [Key]
        public int IdDemande { get; set; }
        
        [Required]
        public int AffilieId { get; set; }
        
        [Required]
        public int PrestationId { get; set; }
        
        [StringLength(500)]
        public string? MotifDemande { get; set; }
        
        /// <summary>Agent assigné à la confirmation (renseigné à la validation).</summary>
        public int? AgentId { get; set; }
        
        [StringLength(500)]
        public string? ObservationAgent { get; set; }
        
        public DateTime DateDemande { get; set; } = DateTime.Now;
        
        public DateTime? DateValidation { get; set; }
        
        [Required]
        [StringLength(20)]
        public string StatutDemande { get; set; } = "EN_ATTENTE"; // "EN_ATTENTE", "VALIDEE", "REJETEE"
        
        public int? BonEnvoiId { get; set; }
        
        public int? JetonMedicalId { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        // Navigation Properties
        [ForeignKey("AffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;
        
        [ForeignKey("PrestationId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Prestation Prestation { get; set; } = null!;
        
        [ForeignKey("AgentId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent? Agent { get; set; }
        
        [ForeignKey("BonEnvoiId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual BonEnvoi? BonEnvoi { get; set; }
        
        [ForeignKey("JetonMedicalId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual JetonMedical? JetonMedical { get; set; }
    }
}
