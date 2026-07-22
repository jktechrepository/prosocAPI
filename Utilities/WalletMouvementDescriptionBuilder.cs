using System.Text.RegularExpressions;
using ProsocAPI.Models.Core;

namespace Prosoc.Utilities
{
    public static class WalletMouvementDescriptionBuilder
    {
        private static readonly Regex CollecteIdRegex = new(
            @"collecte\s*#(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NumeroCollecteRegex = new(
            @"n°\s*(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ConversionSuffixRegex = new(
            @"\(converti en ([^)]+)\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static int? TryExtractCollecteId(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var match = CollecteIdRegex.Match(description);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var legacyId))
                return legacyId;

            match = NumeroCollecteRegex.Match(description);
            return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static string BuildStoredCommissionDescription(
            int collecteId,
            string affilieNom,
            string? deviseConversionCode = null)
        {
            var nom = string.IsNullOrWhiteSpace(affilieNom) ? $"affilié inconnu" : affilieNom.Trim();
            if (string.IsNullOrWhiteSpace(deviseConversionCode))
                return $"Commission collecte — {nom} (n° {collecteId})";

            return $"Commission collecte — {nom} (n° {collecteId}, converti en {deviseConversionCode.Trim()})";
        }

        public static string BuildDisplayDescription(WalletMouvement mouvement, Collecte? collecte)
        {
            if (!WalletMouvementSources.IsCommissionCollecteSource(mouvement.Source))
                return mouvement.Description ?? string.Empty;

            if (collecte == null)
                return mouvement.Description ?? string.Empty;

            var affilieNom = collecte.Affilie?.NomComplet;
            if (string.IsNullOrWhiteSpace(affilieNom))
                affilieNom = $"affilié {collecte.AffilieId}";

            var conversionCode = TryExtractConversionCode(mouvement.Description);
            return BuildStoredCommissionDescription(collecte.IdCollecte, affilieNom, conversionCode);
        }

        private static string? TryExtractConversionCode(string? storedDescription)
        {
            if (string.IsNullOrWhiteSpace(storedDescription))
                return null;

            var match = ConversionSuffixRegex.Match(storedDescription);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
    }
}
