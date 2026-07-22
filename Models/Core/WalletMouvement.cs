using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class WalletMouvement
    {
        [Key]
        public int IdWalletMouvement { get; set; }
        
        public int WalletId { get; set; }

        public int DeviseId { get; set; }
        
        [ForeignKey("WalletId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletAgent Wallet { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }
        
        [Required, StringLength(10)]
        public string TypeOperation { get; set; } = string.Empty; // CREDIT ou DEBIT
        
        [Required, StringLength(20)]
        public string Source { get; set; } = string.Empty; // COLLECTE, BONUS, COMMISSION, RETRAIT
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime DateOperation { get; set; } = DateTime.Now;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;
    }
}
