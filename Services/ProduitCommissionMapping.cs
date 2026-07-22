using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public static class ProduitCommissionMapping
    {
        public static void ApplyRates(ProduitBase produit, ProduitCommissionRatesDto dto)
        {
            produit.TauxCommissionAT = dto.TauxCommissionAT;
            produit.TauxCommissionAA = dto.TauxCommissionAA;
            produit.TauxCommissionAAMash = dto.TauxCommissionAAMash;
            produit.TauxCommissionAAStructure = dto.TauxCommissionAAStructure;
        }

        public static void CopyRatesToDto(ProduitCommissionRatesDto dto, ProduitBase produit)
        {
            dto.TauxCommissionAT = produit.TauxCommissionAT;
            dto.TauxCommissionAA = produit.TauxCommissionAA;
            dto.TauxCommissionAAMash = produit.TauxCommissionAAMash;
            dto.TauxCommissionAAStructure = produit.TauxCommissionAAStructure;
        }
    }
}
