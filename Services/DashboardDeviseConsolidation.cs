using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    internal static class DashboardDeviseConsolidation
    {
        public static decimal SommerCollectesEnDevisePrincipale(IEnumerable<Collecte> collectes) =>
            collectes.Sum(CollecteStatutPaiementRegles.MontantEnDevisePrincipale);

        public static async Task<decimal> MontantMouvementEnDevisePrincipaleAsync(
            IDeviseConversionService deviseConversion,
            decimal montant,
            int deviseId,
            int? devisePrincipaleId,
            DateTime dateOperation,
            CancellationToken ct)
        {
            if (!devisePrincipaleId.HasValue || deviseId == devisePrincipaleId.Value)
                return montant;

            var (montantConverti, _) = await deviseConversion.ConvertirAsync(
                montant,
                deviseId,
                devisePrincipaleId.Value,
                dateOperation,
                ct);
            return montantConverti;
        }

        public static async Task<decimal> SommerMouvementsEnDevisePrincipaleAsync(
            IDeviseConversionService deviseConversion,
            IEnumerable<(decimal Montant, int DeviseId, DateTime DateOperation)> mouvements,
            int devisePrincipaleId,
            CancellationToken ct)
        {
            decimal total = 0;
            foreach (var mouvement in mouvements)
            {
                if (mouvement.DeviseId == devisePrincipaleId)
                {
                    total += mouvement.Montant;
                    continue;
                }

                var (montantConverti, _) = await deviseConversion.ConvertirAsync(
                    mouvement.Montant,
                    mouvement.DeviseId,
                    devisePrincipaleId,
                    mouvement.DateOperation,
                    ct);
                total += montantConverti;
            }

            return total;
        }
    }
}
