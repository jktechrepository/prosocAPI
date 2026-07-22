using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Registre unifié des obligations de paiement d'un affilié (cotisation, souscription, frais).
    /// </summary>
    public class ArrieresAffilie
    {
        [Key]
        public int IdArrieresAffilie { get; set; }

        public int AffilieId { get; set; }

        [Required]
        public TypeCollecte TypeObligation { get; set; }

        public int? FraisId { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        [Column("TarifCotisationId")]
        public int? CotisationAffilieId { get; set; }

        [Range(1, 12)]
        public int Mois { get; set; }

        [Range(2020, 2100)]
        public int Annee { get; set; }

        public DateTime DateEcheance { get; set; }

        [Required, StringLength(20)]
        public string Periodicite { get; set; } = "Mensuel";

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantAttendu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantPaye { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RestAPayer { get; set; }

        public int DeviseId { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        public string StatutPaiement { get; set; } = ArrieresAffilieStatuts.EnAttente;

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }

        public DateTime? DateDernierPaiement { get; set; }

        [ForeignKey("AffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Affilie Affilie { get; set; } = null!;

        [ForeignKey("FraisId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Frais? Frais { get; set; }

        [ForeignKey("SouscriptionPrestationId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual SouscriptionPrestation? SouscriptionPrestation { get; set; }

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

        [ForeignKey("DeviseId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Devise Devise { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();

        [NotMapped]
        public string Periode => $"{Mois:D2}-{Annee}";

        [NotMapped]
        public decimal TauxPaiement => MontantAttendu > 0 ? (MontantPaye / MontantAttendu) * 100 : 0;

        [NotMapped]
        public bool EstCompletementPaye => RestAPayer <= 0;

        public bool IsValid()
        {
            if (TypeObligation == 0)
                return false;

            return TypeObligation switch
            {
                TypeCollecte.Frais => FraisId.HasValue && !CotisationAffilieId.HasValue && !SouscriptionPrestationId.HasValue,
                TypeCollecte.Souscription => SouscriptionPrestationId.HasValue && !FraisId.HasValue && !CotisationAffilieId.HasValue,
                TypeCollecte.Cotisation => CotisationAffilieId.HasValue && !FraisId.HasValue && !SouscriptionPrestationId.HasValue,
                _ => false
            } && Mois >= 1 && Mois <= 12 && Annee >= 2020 && Annee <= 2100;
        }
    }

    public static class ArrieresAffilieStatuts
    {
        public const string EnAttente = "EN_ATTENTE";
        public const string EnRetard = "EN_RETARD";
        public const string PartiellementPaye = "PARTIELLEMENT_PAYE";
        public const string Paye = "PAYE";
    }
}
