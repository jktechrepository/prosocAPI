using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public static class AffilieConformiteStatuts
    {
        public const string EnOrdre = "EN_ORDRE";
        public const string HorsOrdre = "HORS_ORDRE";
    }

    public static class AffilieConformiteRules
    {
        public static bool EstLigneEnSouffrance(ArrieresAffilie arriere, DateTime today)
        {
            if (!arriere.Statut || arriere.RestAPayer <= 0)
                return false;

            return arriere.StatutPaiement == ArrieresAffilieStatuts.EnRetard
                || arriere.StatutPaiement == ArrieresAffilieStatuts.PartiellementPaye
                || (arriere.StatutPaiement == ArrieresAffilieStatuts.EnAttente
                    && arriere.DateEcheance.Date < today.Date);
        }

        public static bool EstEnOrdre(IEnumerable<ArrieresAffilie> lignes, DateTime today) =>
            !lignes.Any(l => EstLigneEnSouffrance(l, today));

        public static bool EstEnOrdrePourType(
            IEnumerable<ArrieresAffilie> lignes,
            TypeCollecte type,
            DateTime today)
        {
            var filtered = lignes.Where(l => l.TypeObligation == type).ToList();
            if (filtered.Count == 0)
                return true;

            return EstEnOrdre(filtered, today);
        }

        public static string ToStatut(bool enOrdre) =>
            enOrdre ? AffilieConformiteStatuts.EnOrdre : AffilieConformiteStatuts.HorsOrdre;
    }
}
