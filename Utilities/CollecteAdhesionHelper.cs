using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class CollecteAdhesionHelper
    {
        public static DateTime ResolveDateCollecte(CollecteCreateDto dto)
        {
            if (dto.Mois is >= 1 and <= 12 && dto.Annee is >= 2020 and <= 2100)
                return new DateTime(dto.Annee, dto.Mois, 1);

            return DateTime.UtcNow;
        }

        public static DateTime ResolveDateCollecte(SouscriptionPrestationCollecteCreateDto dto)
        {
            if (dto.Mois is >= 1 and <= 12 && dto.Annee is >= 2020 and <= 2100)
                return new DateTime(dto.Annee, dto.Mois, 1);

            return DateTime.UtcNow;
        }

        /// <summary>
        /// Date de référence pour les conversions de devises lors d'un paiement immédiat (wallet virtuel).
        /// La période comptable (mois/année) peut être antérieure aux taux en vigueur aujourd'hui.
        /// </summary>
        public static DateTime ResolveDateConversionPaiement(string? modePaiement, DateTime dateCollecte)
        {
            if (IsVirtualAccountPayment(modePaiement))
                return DateTime.UtcNow;

            return dateCollecte;
        }

        public static Collecte ToTempCollecte(CollecteCreateDto dto) => new()
        {
            TypeCollecte = dto.TypeCollecte,
            FraisId = dto.FraisId,
            CotisationAffilieId = dto.CotisationAffilieId,
            SouscriptionPrestationId = dto.SouscriptionPrestationId,
            AffilieId = dto.AffilieId,
            AgentId = dto.AgentId,
            Montant = dto.Montant,
            Mois = dto.Mois,
            Annee = dto.Annee,
            DeviseId = dto.DeviseId,
            ModePaiement = dto.ModePaiement,
            DateCollecte = ResolveDateCollecte(dto)
        };

        public static bool IsVirtualAccountPayment(string? modePaiement) =>
            string.Equals(modePaiement, "VIRTUAL_ACCOUNT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(modePaiement, "COMPTE VIRTUEL", StringComparison.OrdinalIgnoreCase)
            || string.Equals(modePaiement, "COMPTE_VIRTUEL", StringComparison.OrdinalIgnoreCase);
    }
}
