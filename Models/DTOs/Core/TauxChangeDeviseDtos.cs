using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class TauxChangeDeviseReadDto
    {
        public int IdTauxChangeDevise { get; set; }
        public int DeviseSourceId { get; set; }
        public string CodeDeviseSource { get; set; } = string.Empty;
        public int DeviseCibleId { get; set; }
        public string CodeDeviseCible { get; set; } = string.Empty;
        public decimal Taux { get; set; }
        public DateTime DateEffet { get; set; }
        public bool Statut { get; set; }
    }

    public class TauxChangeDeviseCreateDto
    {
        [Required]
        public string CodeDeviseSource { get; set; } = string.Empty;

        [Required]
        public string CodeDeviseCible { get; set; } = string.Empty;

        [Range(0.000001, 999999999.999999)]
        public decimal Taux { get; set; }

        public DateTime? DateEffet { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class PreviewConversionDto
    {
        public string CodeDeviseSource { get; set; } = string.Empty;
        public string CodeDeviseCible { get; set; } = string.Empty;
        public string? CodeDevisePrincipale { get; set; }
        public DateTime DatePaiement { get; set; }
        public decimal Taux { get; set; }
        public decimal MontantSource { get; set; }
        public decimal MontantConverti { get; set; }
    }
}
