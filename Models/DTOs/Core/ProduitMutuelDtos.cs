using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class ProduitMutuelReadDto : ProduitCommissionRatesDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public string Periodicite { get; set; } = string.Empty;
        public int AgeMin { get; set; }
        public int AgeMax { get; set; }
        public bool EstGratuit { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }

        public bool PrestationCree { get; set; }
        public int? PrestationId { get; set; }

        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseNom { get; set; }
    }

    public class ProduitMutuelCreateDto : ProduitCommissionRatesDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Range(0, 999999.99)]
        public decimal Montant { get; set; }

        public bool EstGratuit { get; set; }

        [Required]
        [StringLength(20)]
        public string Periodicite { get; set; } = "Mensuel";

        [Range(0, 120)]
        public int AgeMin { get; set; }

        [Range(0, 120)]
        public int AgeMax { get; set; }

        [Required]
        public int DeviseId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class ProduitMutuelUpdateDto : ProduitCommissionRatesDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Range(0, 999999.99)]
        public decimal Montant { get; set; }

        public bool EstGratuit { get; set; }

        [Required]
        [StringLength(20)]
        public string Periodicite { get; set; } = "Mensuel";

        [Range(0, 120)]
        public int AgeMin { get; set; }

        [Range(0, 120)]
        public int AgeMax { get; set; }

        [Required]
        public int DeviseId { get; set; }

        public bool Statut { get; set; } = true;
    }
}
