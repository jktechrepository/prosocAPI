using ProsocAPI.Models.Configuration;

namespace ProsocAPI.Utilities
{
    public static class RetraitAgentPeriodeHelper
    {
        public const string Fenetre1 = "Fenetre1";
        public const string Fenetre2 = "Fenetre2";
        public const string TypePartiel = "PARTIEL";
        public const string TypeTotal = "TOTAL";

        public static int GetFenetre2Fin(int annee, int mois) =>
            DateTime.DaysInMonth(annee, mois);

        public static int GetFenetre2Debut(int annee, int mois, RetraitAgentOptions options)
        {
            var dernierJour = GetFenetre2Fin(annee, mois);
            var n = Math.Max(1, options.Fenetre2DerniersJours);
            return Math.Max(1, dernierJour - n + 1);
        }

        public static bool EstJourAutorise(DateTime date, RetraitAgentOptions options)
        {
            var jour = date.Day;
            if (jour >= options.Fenetre1Debut && jour <= options.Fenetre1Fin)
                return true;

            var fenetre2Debut = GetFenetre2Debut(date.Year, date.Month, options);
            return jour >= fenetre2Debut;
        }

        public static string? GetFenetreActive(DateTime date, RetraitAgentOptions options)
        {
            if (!EstJourAutorise(date, options))
                return null;

            var jour = date.Day;
            if (jour >= options.Fenetre1Debut && jour <= options.Fenetre1Fin)
                return Fenetre1;

            return Fenetre2;
        }

        public static string? GetTypeRetraitAutorise(DateTime date, RetraitAgentOptions options) =>
            GetFenetreActive(date, options) switch
            {
                Fenetre1 => TypePartiel,
                Fenetre2 => TypeTotal,
                _ => null
            };

        public static string GetPeriodeInfo(DateTime date, RetraitAgentOptions options)
        {
            if (!EstJourAutorise(date, options))
                return "Hors période";

            var jour = date.Day;
            if (jour >= options.Fenetre1Debut && jour <= options.Fenetre1Fin)
                return $"{options.Fenetre1Debut}-{options.Fenetre1Fin}";

            var fenetre2Debut = GetFenetre2Debut(date.Year, date.Month, options);
            var fenetre2Fin = GetFenetre2Fin(date.Year, date.Month);
            return $"{fenetre2Debut}-{fenetre2Fin}";
        }

        public static string BuildMessage(DateTime date, bool estAutorise, RetraitAgentOptions options)
        {
            if (estAutorise)
                return "Période de retrait autorisée";

            var fenetre2Debut = GetFenetre2Debut(date.Year, date.Month, options);
            var fenetre2Fin = GetFenetre2Fin(date.Year, date.Month);
            return $"Les retraits ne sont autorisés que du {options.Fenetre1Debut} au {options.Fenetre1Fin} " +
                   $"et du {fenetre2Debut} au {fenetre2Fin} du mois. Jour actuel: {date.Day}";
        }
    }
}
