using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class TarifCotisationReadDto
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public string Periodicite { get; set; } = string.Empty;
        public int TypeAdhesionId { get; set; }
        public string? TypeAdhesionLibelle { get; set; }
        public int DeviseId { get; set; }
        public string? LibelleTarifCotisation { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseSymbole { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
    }

    public class TarifCotisationMontantCalculDto
    {
        public int CotisationAffilieId { get; set; }
        public int TypeAdhesionId { get; set; }
        public string TypeAdhesionLibelle { get; set; } = string.Empty;
        public string Periodicite { get; set; } = string.Empty;
        public decimal MontantUnitaire { get; set; }
        public int NombreDependants { get; set; }
        public int NombrePersonnes { get; set; }
        public decimal MontantTotal { get; set; }
        public int DeviseId { get; set; }
        public string DeviseCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO catalogue pour /api/TarifCotisation (sans résolution via affilié).
    /// </summary>
    public class TarifCotisationCreateDto
    {
        [Range(0.01, 999999999.99, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        [Required]
        [StringLength(20)]
        public string Periodicite { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TypeAdhesionId { get; set; }

        [Range(1, int.MaxValue)]
        public int DeviseId { get; set; }

        [StringLength(255)]
        public string? LibelleTarifCotisation { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class TarifCotisationUpdateDto
    {
        [Range(0.01, 999999999.99, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        [Required]
        [StringLength(20)]
        public string Periodicite { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TypeAdhesionId { get; set; }

        [Range(1, int.MaxValue)]
        public int DeviseId { get; set; }

        [StringLength(255)]
        public string? LibelleTarifCotisation { get; set; }

        public bool Statut { get; set; } = true;
    }

    [Obsolete("Use TarifCotisationReadDto instead.")]
    public class CotisationAffilieReadDto : TarifCotisationReadDto
    {
    }

    [Obsolete("Use TarifCotisationMontantCalculDto instead.")]
    public class CotisationMontantCalculDto : TarifCotisationMontantCalculDto
    {
    }
}
