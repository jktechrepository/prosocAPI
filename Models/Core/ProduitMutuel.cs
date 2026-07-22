using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class ProduitMutuel : ProduitBase
    {
        [Required]
        public int DeviseId { get; set; }

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;
        
        // Propriétés spécifiques aux produits mutuels si nécessaire
    }
}
