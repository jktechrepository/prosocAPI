using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Antecedant
    {
        [Key]
        public int IdAntecedant { get; set; }
        
        [Required, StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public int AffilieId { get; set; }

        public int? DependantId { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;

        [ForeignKey("AffilieId")]
        [InverseProperty("Antecedants")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;

        [ForeignKey("DependantId")]
        [InverseProperty("Antecedants")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Dependant? Dependant { get; set; }
    }
}
