using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class RetraitAgent
    {
        [Key]
        public int IdRetraitAgent { get; set; }
        
        public int AgentId { get; set; }
        
        [ForeignKey("AgentId")]
        [InverseProperty("Retraits")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }
        
        [Required, StringLength(20)]
        public string CodeRetraitPin { get; set; } = string.Empty;
        
        public DateTime DateDemande { get; set; } = DateTime.Now;
        
        public bool EstValide { get; set; } = false;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public virtual Devise? Devise { get; set; } = null!;
    }
}
