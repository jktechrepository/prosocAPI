using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    /// <summary>Paramètres métier éditables (une ligne par module, valeur JSON typée côté API).</summary>
    public class ParametreMetier
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string ValeurJson { get; set; } = "{}";

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public int? ModifieParUtilisateurId { get; set; }

        [ForeignKey(nameof(ModifieParUtilisateurId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? ModifiePar { get; set; }
    }
}
