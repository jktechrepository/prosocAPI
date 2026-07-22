using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class PersonneContact
    {
        [Key]
        public int IdPersonneContact { get; set; }

        public int AffilieId { get; set; }

        [Required, StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Adresse { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [ForeignKey(nameof(AffilieId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;
    }
}
