using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Province
    {
        [Key]
        public int IdProvince { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;
        
        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Commune> Communes { get; set; } = new List<Commune>();
    }
}
