using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class WalletVirtuelAgent
    {
        [Key]
        public int IdWalletVirtuelAgent { get; set; }
        
        public int AgentId { get; set; }

        public int DeviseId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeVirtuel { get; set; } = 0;

        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; } = true;
        
        [ForeignKey("AgentId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<WalletVirtuelMouvement> Mouvements { get; set; } = new List<WalletVirtuelMouvement>();
    }
}
