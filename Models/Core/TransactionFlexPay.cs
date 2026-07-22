using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    public class TransactionFlexPay
    {
        [Key]
        public Guid IdTransaction { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Reference { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProviderReference { get; set; }

        [Required, MaxLength(10)]
        public string TypePaiement { get; set; } = "1";

        [MaxLength(50)]
        public string? Channel { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountCustomer { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "CDF";

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(10)]
        public string? CodeFlexPay { get; set; }

        [MaxLength(500)]
        public string? MessageFlexPay { get; set; }

        [MaxLength(100)]
        public string? Merchant { get; set; }

        [MaxLength(500)]
        public string? CallbackUrl { get; set; }

        [MaxLength(500)]
        public string? PaymentUrl { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateCallback { get; set; }

        public DateTime? DateDerniereVerification { get; set; }

        public Guid? IdCollecteEnAttente { get; set; }

        public int? IdCollecte { get; set; }

        public CollecteEnAttenteSourceFlux SourceFlux { get; set; }

        [MaxLength(1000)]
        public string? MessageErreur { get; set; }

        public string? ReponseBruteFlexPay { get; set; }

        public int NombreCallbacks { get; set; }

        public int NombreVerifications { get; set; }
    }
}
