using System.Text.RegularExpressions;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Utilities
{
    public static class CategorieAgentLibelleHelper
    {
        private static readonly Regex CodeSuffixRegex = new(@"\(([A-Za-z]{2,10})\)\s*$", RegexOptions.Compiled);

        public static string BuildLibelle(string description, string code)
        {
            var trimmedCode = code.Trim().ToUpperInvariant();
            var trimmedDescription = string.IsNullOrWhiteSpace(description)
                ? trimmedCode
                : description.Trim();

            return $"{trimmedDescription} ({trimmedCode})";
        }

        public static string? ExtractCodeFromLibelle(string? libelle)
        {
            if (string.IsNullOrWhiteSpace(libelle))
                return null;

            var match = CodeSuffixRegex.Match(libelle.Trim());
            if (match.Success)
                return match.Groups[1].Value.ToUpperInvariant();

            var compact = libelle.Trim();
            if (!compact.Contains(' ') && compact.Length <= 10)
                return compact.ToUpperInvariant();

            return null;
        }

        public static string ResolveCode(CategorieAgent? categorie)
        {
            if (categorie == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(categorie.Code))
                return categorie.Code.Trim().ToUpperInvariant();

            return ExtractCodeFromLibelle(categorie.LibelleCategorie) ?? string.Empty;
        }

        public static (string Code, string Description, string Libelle) Normalize(
            string? code,
            string? description,
            string? libelleCategorie)
        {
            var resolvedCode = !string.IsNullOrWhiteSpace(code)
                ? code.Trim().ToUpperInvariant()
                : ExtractCodeFromLibelle(libelleCategorie);

            if (string.IsNullOrWhiteSpace(resolvedCode))
                throw new ArgumentException("Le code de catégorie agent est requis (ex. AT, FI).");

            var resolvedDescription = string.IsNullOrWhiteSpace(description)
                ? resolvedCode
                : description.Trim();

            var resolvedLibelle = !string.IsNullOrWhiteSpace(libelleCategorie)
                                  && libelleCategorie.Contains('(', StringComparison.Ordinal)
                ? libelleCategorie.Trim()
                : BuildLibelle(resolvedDescription, resolvedCode);

            return (resolvedCode, resolvedDescription, resolvedLibelle);
        }
    }
}
