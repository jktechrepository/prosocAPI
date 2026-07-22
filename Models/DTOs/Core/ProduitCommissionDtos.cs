using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.DTOs.Core
{
    public class ProduitCommissionRatesDto
    {
        [Range(0, 100)]
        public decimal TauxCommissionAT { get; set; }

        [Range(0, 100)]
        public decimal TauxCommissionAA { get; set; }

        [Range(0, 100)]
        public decimal TauxCommissionAAMash { get; set; }

        [Range(0, 100)]
        public decimal TauxCommissionAAStructure { get; set; }
    }

    public class ProduitAssureurCommissionRatesDto : ProduitCommissionRatesDto
    {
        // IMPORTANT: L'alias JSON `autrePrime` a été supprimé car il écrasait `tauxCommissionAT`
        // quand les deux étaient envoyés dans le même payload (notamment depuis Swagger).
        // Utiliser uniquement `tauxCommissionAT`.
    }
}
