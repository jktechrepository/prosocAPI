using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class PerceptionVirtuelle
    {
        [Key]
        public int IdPerceptionVirtuelle { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        public int PercepteurUtilisateurId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        [Required]
        public int DeviseId { get; set; }

        [Required]
        public int NombreCollectes { get; set; }

        public DateTime DatePerception { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Observation { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;

        [ForeignKey(nameof(PercepteurUtilisateurId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur PercepteurUtilisateur { get; set; } = null!;

        [ForeignKey(nameof(DeviseId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<PerceptionVirtuelleLigne> Lignes { get; set; } = new List<PerceptionVirtuelleLigne>();
    }
}
