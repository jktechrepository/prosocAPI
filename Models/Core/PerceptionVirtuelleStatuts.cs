namespace ProsocAPI.Models.Core
{
    public static class PerceptionVirtuelleStatuts
    {
        public const string Confirmee = "CONFIRMEE";
        public const string Annulee = "ANNULEE";

        public static bool EstAnnulee(string? statutMetier) =>
            string.Equals(statutMetier, Annulee, StringComparison.OrdinalIgnoreCase);

        public static bool EstConfirmee(string? statutMetier) =>
            string.IsNullOrWhiteSpace(statutMetier)
            || string.Equals(statutMetier, Confirmee, StringComparison.OrdinalIgnoreCase);
    }
}
