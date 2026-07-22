using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Assureur
    {
        [Key]
        public int IdAssureur { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool Statut { get; set; } = true;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<ProduitAssureur> Produits { get; set; } = new List<ProduitAssureur>();
    }
}
