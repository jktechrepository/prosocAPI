namespace ProsocAPI.Models.Core
{
    public static class CollecteStatutPerception
    {
        public const string NonPerçu = "NON_PERCU";
        public const string Perçu = "PERCU";

        public static bool EstNonPerçu(string? statut) =>
            string.IsNullOrWhiteSpace(statut)
            || string.Equals(statut.Trim(), NonPerçu, StringComparison.OrdinalIgnoreCase);

        public static bool EstPerçu(string? statut) =>
            !string.IsNullOrWhiteSpace(statut)
            && string.Equals(statut.Trim(), Perçu, StringComparison.OrdinalIgnoreCase);
    }
}
