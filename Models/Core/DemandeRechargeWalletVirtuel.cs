using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class DemandeRechargeWalletVirtuel
    {
        [Key]
        public int IdDemande { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantCalcule { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeAuMomentDemande { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PlafondAuMomentDemande { get; set; }

        [Required]
        [StringLength(20)]
        public string StatutDemande { get; set; } = DemandeRechargeWalletVirtuelStatuts.EnAttente;

        [StringLength(500)]
        public string? Motif { get; set; }

        [StringLength(500)]
        public string? MotifRejet { get; set; }

        public DateTime DateDemande { get; set; } = DateTime.Now;

        public DateTime? DateConfirmation { get; set; }

        public DateTime? DateRejet { get; set; }

        public int DemandeParUtilisateurId { get; set; }

        public int? ConfirmeParUtilisateurId { get; set; }

        public int? RejeteParUtilisateurId { get; set; }

        public int? WalletVirtuelMouvementId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontantCredite { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldeAvantCredit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldeApresCredit { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent Agent { get; set; } = null!;

        [ForeignKey(nameof(DemandeParUtilisateurId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur DemandePar { get; set; } = null!;

        [ForeignKey(nameof(ConfirmeParUtilisateurId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? ConfirmePar { get; set; }

        [ForeignKey(nameof(RejeteParUtilisateurId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? RejetePar { get; set; }

        [ForeignKey(nameof(WalletVirtuelMouvementId))]
        [JsonIgnore]
        [ValidateNever]
        public virtual WalletVirtuelMouvement? WalletVirtuelMouvement { get; set; }
    }

    public static class DemandeRechargeWalletVirtuelStatuts
    {
        public const string EnAttente = "EN_ATTENTE";
        public const string Confirmee = "CONFIRMEE";
        public const string Rejetee = "REJETEE";
        public const string Annulee = "ANNULEE";
    }
}
