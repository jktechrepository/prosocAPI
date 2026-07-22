using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    /// <summary>Bénéficiaire MAASH rattaché à l'agent (famille).</summary>
    public class AgentBeneficiaireMaash
    {
        [Key]
        public int IdAgentBeneficiaireMaash { get; set; }

        public int AgentId { get; set; }

        [Required, StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Adresse { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;
    }
}
