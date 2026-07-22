using System.Text.RegularExpressions;

namespace ProsocAPI.Helpers
{
    /// <summary>
    /// Normalisation des numéros de téléphone (RDC par défaut : indicatif 243).
    /// </summary>
    public static class PhoneNumberHelper
    {
        private const string DefaultCountryCode = "243";

        private static readonly Regex CanonicalInternationalPattern =
            new(@"^\+243\d{9}$", RegexOptions.Compiled);

        private static readonly Regex CanonicalLocalPattern =
            new(@"^0\d{9}$", RegexOptions.Compiled);

        /// <summary>
        /// Forme canonique de stockage : +243XXXXXXXXX (9 chiffres nationaux).
        /// </summary>
        public static string? NormalizeForStorage(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var trimmed = phone.Trim();
            var digits = ExtractDigits(trimmed);
            if (digits.Length == 0)
                return null;

            if (digits.Length == 10 && digits.StartsWith('0'))
                return $"+{DefaultCountryCode}{digits[1..]}";

            if (digits.StartsWith(DefaultCountryCode, StringComparison.Ordinal) && digits.Length == 12)
                return "+" + digits;

            if (digits.Length == 9 && !digits.StartsWith('0'))
                return $"+{DefaultCountryCode}{digits}";

            return null;
        }

        /// <summary>
        /// Indique si le numéro est un téléphone valide (après normalisation ou format local 0XXXXXXXXX).
        /// </summary>
        public static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var normalized = NormalizeForStorage(phone);
            if (normalized != null && CanonicalInternationalPattern.IsMatch(normalized))
                return true;

            var digits = ExtractDigits(phone.Trim());
            return digits.Length == 10 && digits.StartsWith('0');
        }

        /// <summary>
        /// Variantes à tester en base pour une recherche tolérante (0…, +243…, 243…).
        /// </summary>
        public static IReadOnlyList<string> GetLookupVariants(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Array.Empty<string>();

            var variants = new HashSet<string>(StringComparer.Ordinal);

            var trimmed = phone.Trim();
            variants.Add(trimmed);

            var digits = ExtractDigits(trimmed);
            if (digits.Length > 0)
            {
                variants.Add(digits);
                if (!digits.StartsWith('+'))
                    variants.Add("+" + digits);
            }

            var normalized = NormalizeForStorage(trimmed);
            if (!string.IsNullOrEmpty(normalized))
            {
                variants.Add(normalized);
                variants.Add(normalized[1..]); // 243XXXXXXXXX

                if (normalized.Length == 13 && normalized.StartsWith("+243", StringComparison.Ordinal))
                {
                    var national = normalized[4..];
                    variants.Add("0" + national);
                }
            }
            else if (digits.Length == 10 && digits.StartsWith('0'))
            {
                variants.Add(digits);
                variants.Add($"+{DefaultCountryCode}{digits[1..]}");
                variants.Add($"{DefaultCountryCode}{digits[1..]}");
            }

            return variants.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        }

        private static string ExtractDigits(string value) =>
            new string(value.Where(char.IsDigit).ToArray());
    }
}
