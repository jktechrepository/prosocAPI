using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class ZoneSociale
    {
        [Key]
        public int IdZoneSociale { get; set; }  // ✅ Standardisé
        
        [Required, StringLength(200)]
        public string Nom { get; set; } = string.Empty;
        
        public int CommuneId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        /// <summary>Titulaire Chef d'équipe de la zone (unique par zone et par agent).</summary>
        public int? ChefEquipeAgentId { get; set; }

        [ForeignKey("CommuneId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Commune Commune { get; set; } = null!;

        [ForeignKey(nameof(ChefEquipeAgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent? ChefEquipe { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();
    }
}
