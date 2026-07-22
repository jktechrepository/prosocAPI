namespace ProsocAPI.Models.Core
{
    /// <summary>Règles de statut paiement collecte — EN_ATTENTE vs VALIDE.</summary>
    public static class CollecteStatutPaiementRegles
    {
        private static readonly string[] AliasLegacyValide =
        {
            "OK", "VALIDE", "VALIDÉ", "Validé", "Valide",
            "PAYE", "PAYÉ", "Payé", "Paye",
            "CONFIRME", "CONFIRMÉ", "Confirmé", "Confirme"
        };

        /// <summary>Valeurs reconnues comme payées dans les requêtes EF (canonique + legacy).</summary>
        public static readonly string[] ValeursSqlValideEtLegacy =
        {
            CollecteStatutPaiement.Valide,
            "Validé", "Valide", "OK",
            "PAYE", "PAYÉ", "Payé", "Paye",
            "VALIDÉ", "CONFIRME", "CONFIRMÉ", "Confirmé", "Confirme"
        };

        /// <summary>Paiement finalisé (état canonique VALIDE ou alias legacy en lecture).</summary>
        public static bool EstValide(string? statutPaiement)
        {
            if (string.IsNullOrWhiteSpace(statutPaiement))
                return false;

            var trimmed = statutPaiement.Trim();
            if (string.Equals(trimmed, CollecteStatutPaiement.Valide, StringComparison.OrdinalIgnoreCase))
                return true;

            return AliasLegacyValide.Contains(trimmed, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Paiement non finalisé (FlexPay en cours).</summary>
        public static bool EstEnAttente(string? statutPaiement) =>
            !string.IsNullOrWhiteSpace(statutPaiement)
            && string.Equals(
                statutPaiement.Trim(),
                CollecteStatutPaiement.EnAttente,
                StringComparison.OrdinalIgnoreCase);

        /// <summary>Normalise une valeur entrante vers EN_ATTENTE ou VALIDE pour persistance.</summary>
        public static string NormaliserPourEcriture(string? statutPaiement)
        {
            if (EstEnAttente(statutPaiement))
                return CollecteStatutPaiement.EnAttente;

            return CollecteStatutPaiement.Valide;
        }

        [Obsolete("Utiliser EstValide — plus de distinction validation admin.")]
        public static bool EstPaiementValideAdmin(string? statutPaiement) => EstValide(statutPaiement);

        [Obsolete("Utiliser EstEnAttente — la file admin ne couvre plus que les paiements EN_ATTENTE.")]
        public static bool EstEnAttenteValidationAdmin(string? statutPaiement, bool statutCollecte = true) =>
            statutCollecte && EstEnAttente(statutPaiement);

        public static decimal CalculerProgressionCollectesMois(
            decimal totalCollectesMoisCourant,
            decimal totalCollectesMoisPrecedent)
        {
            if (totalCollectesMoisPrecedent > 0)
            {
                return ((totalCollectesMoisCourant - totalCollectesMoisPrecedent)
                    / totalCollectesMoisPrecedent) * 100;
            }

            return totalCollectesMoisCourant > 0 ? 100 : 0;
        }

        /// <summary>Montant consolidé en devise principale (snapshot ou montant brut).</summary>
        public static decimal MontantEnDevisePrincipale(Collecte collecte) =>
            collecte.MontantDevisePrincipale ?? collecte.Montant;
    }
}
