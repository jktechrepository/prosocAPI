using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.Core
{
    public class Collecte
    {
        [Key]
        public int IdCollecte { get; set; }
        
        // NOUVEAU : Type de collecte (obligatoire)
        [Required]
        public TypeCollecte TypeCollecte { get; set; }
        
        // NOUVEAU : Relation avec Frais (optionnel)
        public int? FraisId { get; set; }

        [Column("TarifCotisationId")]
        public int? CotisationAffilieId { get; set; }
        
        public int AffilieId { get; set; }
        
        public int? AgentId { get; set; }

        /// <summary>Utilisateur guichet (caissier/percepteur) ayant saisi l'encaissement.</summary>
        public int? OperateurUtilisateurId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [StringLength(100)]
        public string? ReferencePaiement { get; set; }

        [MaxLength(100)]
        public string? OrderNumberFlexPay { get; set; }

        [MaxLength(100)]
        public string? ProviderReferenceFlexPay { get; set; }

        [StringLength(20)]
        public string? ModePaiement { get; set; }

        [StringLength(50)]
        public string? Operateur { get; set; }

        [StringLength(20)]
        public string? StatutPaiement { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        public int? ArrieresAffilieId { get; set; }

        public int? PenaliteAffilieId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontantRecu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontantAttendu { get; set; }
        
        public int DeviseId { get; set; }

        public int? DevisePrincipaleId { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? TauxVersDevisePrincipale { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontantDevisePrincipale { get; set; }

        public int? DeviseTarifId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontantTarifAttendu { get; set; }
        
        public DateTime DateCollecte { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string? Observation { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public bool Statut { get; set; } = true;

        [StringLength(20)]
        public string? StatutPerception { get; set; }

        public DateTime? DatePerception { get; set; }

        public int? PercepteurUtilisateurId { get; set; }

        public int? PerceptionVirtuelleId { get; set; }

        [ForeignKey("AffilieId")]
        [InverseProperty("Collectes")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;

        [ForeignKey("AgentId")]
        [InverseProperty("Collectes")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Agent? Agent { get; set; }

        [ForeignKey("OperateurUtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? OperateurUtilisateur { get; set; }

        [ForeignKey("PercepteurUtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Utilisateur? PercepteurUtilisateur { get; set; }

        [ForeignKey("PerceptionVirtuelleId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual PerceptionVirtuelle? PerceptionVirtuelle { get; set; }

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [ForeignKey("DevisePrincipaleId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise? DevisePrincipale { get; set; }

        [ForeignKey("DeviseTarifId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise? DeviseTarif { get; set; }

        [ForeignKey("SouscriptionPrestationId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual SouscriptionPrestation? SouscriptionPrestationRef { get; set; }

        // NOUVEAU : Navigation vers Frais
        [ForeignKey("FraisId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Frais? Frais { get; set; }

        [ForeignKey("CotisationAffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual TarifCotisation? CotisationAffilie { get; set; }

        [NotMapped]
        public TarifCotisation? TarifCotisation
        {
            get => CotisationAffilie;
            set => CotisationAffilie = value;
        }

        [ForeignKey("ArrieresAffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual ArrieresAffilie? ArrieresAffilie { get; set; }

        [ForeignKey("PenaliteAffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual PenaliteAffilie? PenaliteAffilie { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<WalletMouvement> MouvementsWallet { get; set; } = new List<WalletMouvement>();
        
        // NOUVEAU : Méthode de validation
        public bool IsValid()
        {
            // ✅ AMÉLIORATION : Validation du TypeCollecte = 0
            if (TypeCollecte == 0)
                return false;
                
            return TypeCollecte switch
            {
                TypeCollecte.Frais => FraisId.HasValue && !CotisationAffilieId.HasValue && !SouscriptionPrestationId.HasValue,
                TypeCollecte.Souscription => SouscriptionPrestationId.HasValue && !FraisId.HasValue && !CotisationAffilieId.HasValue,
                TypeCollecte.Cotisation => CotisationAffilieId.HasValue && !FraisId.HasValue && !SouscriptionPrestationId.HasValue,
                _ => false
            } && Mois >= 1 && Mois <= 12 
            && Annee >= 2020 && Annee <= 2100;
        }
    }
}
