using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class MouvementCaisse
    {
        [Key]
        public int IdMouvementCaisse { get; set; }

        [Required]
        public int SessionCaisseId { get; set; }

        [Required]
        public int UtilisateurId { get; set; }

        [Required]
        [StringLength(10)]
        public string TypeOperation { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Source { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        [Required]
        public int DeviseId { get; set; }

        public DateTime DateOperation { get; set; } = DateTime.Now;

        public int? CollecteId { get; set; }

        public int? DemandeRetraitId { get; set; }

        public int? JetonRetraitId { get; set; }

        public int? WalletMouvementId { get; set; }

        public int? PerceptionVirtuelleId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public bool Statut { get; set; } = true;

        [ForeignKey("SessionCaisseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual SessionCaisse SessionCaisse { get; set; } = null!;

        [ForeignKey("UtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur Utilisateur { get; set; } = null!;

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [ForeignKey("CollecteId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Collecte? Collecte { get; set; }

        [ForeignKey("DemandeRetraitId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual DemandeRetraitAgent? DemandeRetrait { get; set; }

        [ForeignKey("JetonRetraitId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual JetonRetrait? JetonRetrait { get; set; }

        [ForeignKey("WalletMouvementId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletMouvement? WalletMouvement { get; set; }

        [ForeignKey(nameof(PerceptionVirtuelleId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual PerceptionVirtuelle? PerceptionVirtuelle { get; set; }
    }
}
