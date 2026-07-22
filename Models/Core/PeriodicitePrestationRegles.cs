namespace ProsocAPI.Models.Core
{
    public static class PeriodicitePrestationRegles
    {
        public static readonly string[] ValeursValides = { "Mensuel", "Trimestriel", "Semestriel", "Annuel" };

        public static string Normaliser(string? periodicite, string fallback = "Mensuel")
        {
            var value = string.IsNullOrWhiteSpace(periodicite) ? fallback : periodicite.Trim();

            if (value.Equals("mensuel", StringComparison.OrdinalIgnoreCase))
                return "Mensuel";
            if (value.Equals("trimestriel", StringComparison.OrdinalIgnoreCase))
                return "Trimestriel";
            if (value.Equals("semestriel", StringComparison.OrdinalIgnoreCase))
                return "Semestriel";
            if (value.Equals("annuel", StringComparison.OrdinalIgnoreCase))
                return "Annuel";

            throw new ArgumentException(
                $"Périodicité invalide : '{periodicite}'. Valeurs acceptées : {string.Join(", ", ValeursValides)}.");
        }
    }
}
