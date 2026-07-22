using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class WalletAgent
    {
        [Key]
        public int IdWalletAgent { get; set; }
        
        public int AgentId { get; set; }

        public int DeviseId { get; set; }
        
        [ForeignKey("AgentId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;
        
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeCourant { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeDisponible { get; set; } = 0;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<WalletMouvement> Mouvements { get; set; } = new List<WalletMouvement>();
    }
}
