using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    /// <summary>Retenue mensuelle à la source pour couverture MAASH (agent + famille).</summary>
    public class RetenueMaashAgent
    {
        [Key]
        public int IdRetenueMaashAgent { get; set; }

        public int AgentId { get; set; }

        public int Annee { get; set; }

        public int Mois { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        public int DeviseId { get; set; }

        public int? WalletMouvementId { get; set; }

        public DateTime DatePaiement { get; set; } = DateTime.Now;

        public bool Statut { get; set; } = true;

        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;

        [ForeignKey(nameof(WalletMouvementId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletMouvement? WalletMouvement { get; set; }
    }
}
