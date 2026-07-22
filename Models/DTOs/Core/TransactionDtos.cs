using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.DTOs.Core
{
    public class TransactionDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class CollecteDto : TransactionDto
    {
        public int AffilieId { get; set; }
        public int AdhesionId { get; set; }
    }

    public class RetraitDto : TransactionDto
    {
        [Required]
        [StringLength(200)]
        public string MotifRetrait { get; set; } = string.Empty;
    }

    public class BonusDto : TransactionDto
    {
        [Required]
        [StringLength(100)]
        public string SourceBonus { get; set; } = string.Empty;
    }

    public class CommissionDto : TransactionDto
    {
        [Required]
        [StringLength(100)]
        public string SourceCommission { get; set; } = string.Empty;
        public decimal TauxCommission { get; set; }
    }

    public class TransactionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NouveauSolde { get; set; }
        public int MouvementId { get; set; }
    }
}
