namespace ProsocAPI.Models.Configuration
{
    public class WalletVirtuelOptions
    {
        public const string SectionName = "WalletVirtuel";

        /// <summary>Plafond de solde du wallet virtuel agent (devise du wallet).</summary>
        public decimal PlafondSolde { get; set; } = 100m;
    }
}
