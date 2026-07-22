using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    public class CollecteEnAttente
    {
        [Key]
        public Guid IdCollecteEnAttente { get; set; } = Guid.NewGuid();

        public CollecteEnAttenteSourceFlux SourceFlux { get; set; }

        public CollecteEnAttenteStatut StatutEnAttente { get; set; } = CollecteEnAttenteStatut.EnAttente;

        public int? AffilieId { get; set; }

        public int? AgentId { get; set; }

        public int? IdUtilisateur { get; set; }

        public TypeCollecte TypeCollecte { get; set; }

        public int? FraisId { get; set; }

        public int? CotisationAffilieId { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        public int Mois { get; set; }

        public int Annee { get; set; }

        [Required, MaxLength(30)]
        public string MethodePaiement { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTarif { get; set; }

        public int DeviseTarifId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantFlexPay { get; set; }

        [Required, MaxLength(10)]
        public string CodeDevisePaiement { get; set; } = "CDF";

        [Column(TypeName = "decimal(18,6)")]
        public decimal? TauxVersDevisePaiement { get; set; }

        [MaxLength(100)]
        public string? OrderNumberFlexPay { get; set; }

        [MaxLength(100)]
        public string? ReferenceFlexPay { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? TelephoneAffilie { get; set; }

        public string PayloadMetierJson { get; set; } = string.Empty;

        public DateTime DateExpiration { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }

        public int? IdCollecteFinalisee { get; set; }

        public int? IdAdhesionFinalisee { get; set; }
    }
}
