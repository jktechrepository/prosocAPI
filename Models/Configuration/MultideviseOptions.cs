namespace ProsocAPI.Models.Configuration
{
    public class MultideviseOptions
    {
        public const string SectionName = "Multidevise";

        /// <summary>Code ISO de la devise des cotisations (tarif catalogue).</summary>
        public string DeviseTarifCotisationCode { get; set; } = "CDF";

        public decimal ToleranceConversion { get; set; } = 0.01m;
    }
}
