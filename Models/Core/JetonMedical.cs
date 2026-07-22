using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class JetonMedical
    {
        [Key]
        public int IdJeton { get; set; }
        
        [Required]
        public int AffilieId { get; set; }
        
        [Required, StringLength(20)]
        public string CodeJeton { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateEmission { get; set; } = DateTime.Now;
        
        public DateTime? DateUtilisation { get; set; }
        
        public DateTime? DateExpiration { get; set; }
        
        [Required]
        public bool EstValide { get; set; } = true;
        
        [Required]
        public bool EstUtilise { get; set; } = false;
        
        public int? HopitalPartenaireId { get; set; }
        
        [StringLength(500)]
        public string? Observation { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
        
        [ForeignKey("AffilieId")]
        [InverseProperty("JetonsMedicaux")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;
        
        [ForeignKey("HopitalPartenaireId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual HopitalPartenaire? HopitalPartenaire { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual BonEnvoi? BonEnvoiLie { get; set; }
    }
}
