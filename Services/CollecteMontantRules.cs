using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    /// <summary>
    /// Montants consolidés en devise principale (USD) pour dashboards et reporting.
    /// </summary>
    public static class CollecteMontantRules
    {
        /// <summary>
        /// Montant consolidé. Dans EF : <c>c.MontantDevisePrincipale ?? c.Montant</c>.
        /// </summary>
        public static decimal MontantConsolide(Collecte collecte) =>
            collecte.MontantDevisePrincipale ?? collecte.Montant;
    }
}
