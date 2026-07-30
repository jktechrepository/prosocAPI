using System.Globalization;
using System.Text;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Canon <see cref="Adhesion.StatutDossier"/> : uniquement
    /// <see cref="EnAttente"/> et <see cref="Valide"/>.
    /// </summary>
    public static class AdhesionStatutDossierRegles
    {
        public const string EnAttente = "EN ATTENTE";
        public const string Valide = "VALIDÉ";

        /// <summary>
        /// Mappe une valeur libre / legacy vers un canon.
        /// <c>COMPLET</c>, <c>VALIDE</c>, <c>VALIDÉ</c> → <see cref="Valide"/> ;
        /// <c>A</c>, <c>B</c>, variantes d'attente, vide ou inconnu → <see cref="EnAttente"/>.
        /// </summary>
        public static string Normaliser(string? statutDossier)
        {
            var key = CleComparaison(statutDossier);
            return key switch
            {
                "VALIDE" or "COMPLET" => Valide,
                _ => EnAttente
            };
        }

        public static bool EstEnAttente(string? statutDossier) =>
            Normaliser(statutDossier) == EnAttente;

        public static bool EstValide(string? statutDossier) =>
            Normaliser(statutDossier) == Valide;

        /// <summary>
        /// Parse strict pour filtre API : valeurs reconnues → canon ;
        /// vide → <c>null</c> (pas de filtre) ; inconnu → <c>null</c> (à traiter en 400).
        /// Ne mappe pas l'inconnu vers <see cref="EnAttente"/> (contrairement à <see cref="Normaliser"/>).
        /// </summary>
        public static string? EssayerParserFiltre(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            return CleComparaison(raw) switch
            {
                "ENATTENTE" or "A" or "B" => EnAttente,
                "VALIDE" or "COMPLET" => Valide,
                _ => null
            };
        }

        /// <summary>Clé upper, sans accents, espaces retirés.</summary>
        public static string CleComparaison(string? statutDossier)
        {
            if (string.IsNullOrWhiteSpace(statutDossier))
                return string.Empty;

            var trimmed = statutDossier.Trim();
            var upper = trimmed.ToUpperInvariant();
            var withoutDiacritics = SupprimerDiacritiques(upper);
            return string.Concat(withoutDiacritics.Where(c => !char.IsWhiteSpace(c)));
        }

        private static string SupprimerDiacritiques(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
