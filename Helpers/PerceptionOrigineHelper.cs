using ProsocAPI.Models.Core;

namespace ProsocAPI.Helpers
{
    /// <summary>Classification perception Agent (VA) vs Affilié (guichet direct).</summary>
    public static class PerceptionOrigineHelper
    {
        public const string OrigineAgent = "AGENT";
        public const string OrigineAffilie = "AFFILIE";
        public const string OrigineTous = "TOUS";

        public const string StatutEnAttente = "EN_ATTENTE";
        public const string StatutPercu = "PERCU";
        public const string StatutTous = "TOUS";

        public static bool IsOrigineAgent(Collecte collecte, bool hasDebitVirtuel) =>
            collecte.Statut
            && CollecteStatutPaiementRegles.EstValide(collecte.StatutPaiement)
            && MethodePaiementHelper.IsVirtualAccount(collecte.ModePaiement)
            && hasDebitVirtuel;

        public static bool IsOrigineAffilie(Collecte collecte) =>
            collecte.Statut
            && CollecteStatutPaiementRegles.EstValide(collecte.StatutPaiement)
            && MethodePaiementHelper.IsGuichetSync(collecte.ModePaiement)
            && !MethodePaiementHelper.IsVirtualAccount(collecte.ModePaiement);

        public static string? ResolveOrigine(Collecte collecte, bool hasDebitVirtuel)
        {
            if (IsOrigineAgent(collecte, hasDebitVirtuel))
                return OrigineAgent;

            if (IsOrigineAffilie(collecte))
                return OrigineAffilie;

            return null;
        }

        public static string ResolveStatutPerception(Collecte collecte, bool isOrigineAgent) =>
            isOrigineAgent
                ? (CollecteStatutPerception.EstPerçu(collecte.StatutPerception) ? StatutPercu : StatutEnAttente)
                : StatutPercu;

        public static string NormalizeOrigineFiltre(string? origine) =>
            string.IsNullOrWhiteSpace(origine)
                ? OrigineTous
                : origine.Trim().ToUpperInvariant() switch
                {
                    OrigineAgent => OrigineAgent,
                    OrigineAffilie => OrigineAffilie,
                    OrigineTous => OrigineTous,
                    _ => OrigineTous
                };

        public static string NormalizeStatutFiltre(string? statut) =>
            string.IsNullOrWhiteSpace(statut)
                ? StatutTous
                : statut.Trim().ToUpperInvariant() switch
                {
                    StatutEnAttente => StatutEnAttente,
                    StatutPercu => StatutPercu,
                    StatutTous => StatutTous,
                    _ => StatutTous
                };

        public static bool MatchesOrigineFiltre(string? origineCollecte, string origineFiltre) =>
            origineFiltre == OrigineTous
            || string.Equals(origineCollecte, origineFiltre, StringComparison.OrdinalIgnoreCase);

        public static bool MatchesStatutFiltre(string statutCollecte, string statutFiltre) =>
            statutFiltre == StatutTous
            || string.Equals(statutCollecte, statutFiltre, StringComparison.OrdinalIgnoreCase);
    }
}
