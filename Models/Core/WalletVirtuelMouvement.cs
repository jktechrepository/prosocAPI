using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class WalletVirtuelMouvement
    {
        [Key]
        public int IdWalletVirtuelMouvement { get; set; }

        public int WalletVirtuelId { get; set; }

        [ForeignKey(nameof(WalletVirtuelId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletVirtuelAgent WalletVirtuel { get; set; } = null!;

        public int? DeviseId { get; set; }

        [ForeignKey(nameof(DeviseId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise? Devise { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        [Required, StringLength(10)]
        public string TypeOperation { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Source { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ReferenceExterne { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldeAvant { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldeApres { get; set; }

        public int? OperateurUtilisateurId { get; set; }

        [ForeignKey(nameof(OperateurUtilisateurId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? OperateurUtilisateur { get; set; }

        public DateTime DateOperation { get; set; } = DateTime.Now;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;
    }
}
