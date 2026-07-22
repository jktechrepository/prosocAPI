using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class CategorieAdhesion
    {
        [Key]
        public int IdCategorieAdhesion { get; set; }

        [Required]
        [StringLength(200)]
        public string Libelle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<TypeAdhesion> TypeAdhesions { get; set; } = new List<TypeAdhesion>();
    }
}
