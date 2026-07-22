using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class PerceptionVirtuelleLigne
    {
        [Key]
        public int IdLigne { get; set; }

        [Required]
        public int PerceptionVirtuelleId { get; set; }

        [Required]
        public int CollecteId { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        public int? WalletVirtuelMouvementId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public bool Statut { get; set; } = true;

        [ForeignKey(nameof(PerceptionVirtuelleId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual PerceptionVirtuelle PerceptionVirtuelle { get; set; } = null!;

        [ForeignKey(nameof(CollecteId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Collecte Collecte { get; set; } = null!;

        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;

        [ForeignKey(nameof(WalletVirtuelMouvementId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletVirtuelMouvement? WalletVirtuelMouvement { get; set; }
    }
}
