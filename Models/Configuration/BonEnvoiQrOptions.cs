namespace ProsocAPI.Models.Configuration
{
    public class BonEnvoiQrOptions
    {
        public const string SectionName = "BonEnvoiQr";

        /// <summary>Clé HMAC pour signer les QR (min. 32 caractères recommandé).</summary>
        public string SigningKey { get; set; } = string.Empty;

        /// <summary>Validité du QR en jours (alignée jeton médical).</summary>
        public int ValidityDays { get; set; } = 30;
    }
}
