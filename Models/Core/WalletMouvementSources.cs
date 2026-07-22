namespace ProsocAPI.Models.Core
{
    public static class WalletMouvementSources
    {
        public const string CommissionCollecte = "COMM_COLLECTE";
        public const string RetraitJeton = "RETRAIT_JETON";
        public const string RetenueMaash = "RETENUE_MAASH";
        public const string MigrationRetraitDevise = "MIG_RETRAIT_DEVISE";

        public static bool IsCommissionCollecteSource(string? source) =>
            !string.IsNullOrWhiteSpace(source)
            && (source == CommissionCollecte
                || source.Contains("COMMISSION", StringComparison.OrdinalIgnoreCase));
    }
}
