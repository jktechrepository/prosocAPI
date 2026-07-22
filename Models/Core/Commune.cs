using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Commune
    {
        [Key]
        public int IdCommune { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;
        
        public int ProvinceId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        /// <summary>Titulaire Superviseur de la commune (unique par commune et par agent).</summary>
        public int? SuperviseurAgentId { get; set; }

        [ForeignKey("ProvinceId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Province Province { get; set; } = null!;

        [ForeignKey(nameof(SuperviseurAgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent? Superviseur { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<ZoneSociale> Zones { get; set; } = new List<ZoneSociale>();
    }
}
