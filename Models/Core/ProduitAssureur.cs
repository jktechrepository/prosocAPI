using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class ProduitAssureur : ProduitBase
    {
        public int AssureurId { get; set; }
        
        [ForeignKey("AssureurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Assureur Partenaire { get; set; } = null!;
        
        [Required]
        public int DeviseId { get; set; }

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;
    }
}
