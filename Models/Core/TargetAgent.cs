using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class TargetAgent
    {
        [Key]
        public int IdTargetAgent { get; set; }

        public int RoleId { get; set; }

        [Required, StringLength(200)]
        public string LibelleTarget { get; set; } = string.Empty;

        public PeriodiciteTarget Periodicite { get; set; }

        /// <summary>Nombre d'adhésions cible (F3-6), aligné sur la périodicité.</summary>
        public int Nombre { get; set; }

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        [ForeignKey("RoleId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Role Role { get; set; } = null!;
    }
}
