using System.Globalization;
using System.Text;

namespace ProsocAPI.Models.Core
{
    public static class LienParenteRegles
    {
        public static readonly string[] ValeursValides =
        {
            "PERE", "MERE", "GRAND_PERE", "GRAND_MERE", "ONCLE", "TANTE",
            "FRERE", "SOEUR", "COUSIN", "COUSINE", "AUTRE",
            "CONJOINT", "EPOUSE", "MARI", "FEMME", "MARIE", "EPOUX",
            "ENFANT", "FILLE", "FILS", "AMI", "VOISIN", "COLLEGUE"
        };

        private static readonly HashSet<string> CodesValides = new(ValeursValides, StringComparer.Ordinal);

        private static readonly Dictionary<string, string> AliasVersCode = ConstruireAlias();

        private static readonly Dictionary<string, string> LibellesParCode = ConstruireLibelles();

        private static readonly Dictionary<string, string> CategoriesParCode = ConstruireCategories();

        public static readonly string[] LiensEnfant = { "ENFANT", "FILS", "FILLE" };

        public static readonly string[] LiensConjoint =
        {
            "CONJOINT", "EPOUSE", "EPOUX", "MARI", "FEMME", "MARIE"
        };

        public static IReadOnlyList<(string Code, string Libelle, string Categorie)> GetReferentiel()
        {
            return ValeursValides
                .Select(code => (
                    code,
                    LibellesParCode.TryGetValue(code, out var libelle) ? libelle : code,
                    CategoriesParCode.TryGetValue(code, out var categorie) ? categorie : "AUTRE"))
                .ToList();
        }

        public static string? GetLibelle(string code) =>
            LibellesParCode.TryGetValue(code, out var libelle) ? libelle : null;

        public static bool EstLienEnfant(string? lienParente)
        {
            if (string.IsNullOrWhiteSpace(lienParente))
                return false;
            return LiensEnfant.Contains(Normaliser(lienParente), StringComparer.Ordinal);
        }

        public static bool EstLienConjoint(string? lienParente)
        {
            if (string.IsNullOrWhiteSpace(lienParente))
                return false;
            return LiensConjoint.Contains(Normaliser(lienParente), StringComparer.Ordinal);
        }

        public static bool EstValide(string? lien)
        {
            if (string.IsNullOrWhiteSpace(lien))
                return false;

            return CodesValides.Contains(Normaliser(lien));
        }

        /// <summary>
        /// Convertit un libellé français ou un code technique en code normalisé (ex. « Conjoint(e) » → CONJOINT).
        /// </summary>
        public static string Normaliser(string lien)
        {
            var trimmed = lien.Trim();
            if (trimmed.Length == 0)
                return string.Empty;

            var cle = NormaliserCle(trimmed);
            if (AliasVersCode.TryGetValue(cle, out var code))
                return code;

            var upper = trimmed.ToUpperInvariant();
            return CodesValides.Contains(upper) ? upper : upper;
        }

        private static Dictionary<string, string> ConstruireAlias()
        {
            var aliases = new Dictionary<string, string>(StringComparer.Ordinal);

            void Map(string alias, string code)
            {
                aliases[NormaliserCle(alias)] = code;
            }

            foreach (var code in ValeursValides)
            {
                Map(code, code);
                Map(code.Replace('_', ' '), code);
                Map(code.Replace('_', '-'), code);
            }

            Map("Père", "PERE");
            Map("Pere", "PERE");
            Map("Mère", "MERE");
            Map("Mere", "MERE");
            Map("Grand-père", "GRAND_PERE");
            Map("Grand père", "GRAND_PERE");
            Map("Grandpère", "GRAND_PERE");
            Map("Grand-pere", "GRAND_PERE");
            Map("Grand-mère", "GRAND_MERE");
            Map("Grand mère", "GRAND_MERE");
            Map("Grandmere", "GRAND_MERE");
            Map("Grand-mere", "GRAND_MERE");
            Map("Oncle", "ONCLE");
            Map("Tante", "TANTE");
            Map("Frère", "FRERE");
            Map("Frere", "FRERE");
            Map("Sœur", "SOEUR");
            Map("Soeur", "SOEUR");
            Map("Cousin", "COUSIN");
            Map("Cousine", "COUSINE");
            Map("Cousin(e)", "COUSIN");
            Map("Cousin(e", "COUSIN");
            Map("Conjoint", "CONJOINT");
            Map("Conjoint(e)", "CONJOINT");
            Map("Conjointe", "CONJOINT");
            Map("Épouse", "EPOUSE");
            Map("Epouse", "EPOUSE");
            Map("Époux", "EPOUX");
            Map("Epoux", "EPOUX");
            Map("Mari", "MARI");
            Map("Femme", "FEMME");
            Map("Marié", "MARIE");
            Map("Mariée", "MARIE");
            Map("Enfant", "ENFANT");
            Map("Fille", "FILLE");
            Map("Fils", "FILS");
            Map("Ami", "AMI");
            Map("Amie", "AMI");
            Map("Voisin", "VOISIN");
            Map("Voisine", "VOISIN");
            Map("Collègue", "COLLEGUE");
            Map("Collegue", "COLLEGUE");
            Map("Autre", "AUTRE");

            return aliases;
        }

        private static string NormaliserCle(string value)
        {
            var upper = value.Trim().ToUpperInvariant();
            var formD = upper.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);

            foreach (var c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static Dictionary<string, string> ConstruireLibelles() =>
            new(StringComparer.Ordinal)
            {
                ["PERE"] = "Père",
                ["MERE"] = "Mère",
                ["GRAND_PERE"] = "Grand-père",
                ["GRAND_MERE"] = "Grand-mère",
                ["ONCLE"] = "Oncle",
                ["TANTE"] = "Tante",
                ["FRERE"] = "Frère",
                ["SOEUR"] = "Sœur",
                ["COUSIN"] = "Cousin",
                ["COUSINE"] = "Cousine",
                ["CONJOINT"] = "Conjoint(e)",
                ["EPOUSE"] = "Épouse",
                ["EPOUX"] = "Époux",
                ["MARI"] = "Mari",
                ["FEMME"] = "Femme",
                ["MARIE"] = "Marié(e)",
                ["ENFANT"] = "Enfant",
                ["FILS"] = "Fils",
                ["FILLE"] = "Fille",
                ["AMI"] = "Ami(e)",
                ["VOISIN"] = "Voisin(e)",
                ["COLLEGUE"] = "Collègue",
                ["AUTRE"] = "Autre"
            };

        private static Dictionary<string, string> ConstruireCategories() =>
            new(StringComparer.Ordinal)
            {
                ["PERE"] = "ASCENDANT",
                ["MERE"] = "ASCENDANT",
                ["GRAND_PERE"] = "ASCENDANT",
                ["GRAND_MERE"] = "ASCENDANT",
                ["ONCLE"] = "FAMILLE_ELARGIE",
                ["TANTE"] = "FAMILLE_ELARGIE",
                ["COUSIN"] = "FAMILLE_ELARGIE",
                ["COUSINE"] = "FAMILLE_ELARGIE",
                ["FRERE"] = "FRATRIE",
                ["SOEUR"] = "FRATRIE",
                ["CONJOINT"] = "CONJOINT",
                ["EPOUSE"] = "CONJOINT",
                ["EPOUX"] = "CONJOINT",
                ["MARI"] = "CONJOINT",
                ["FEMME"] = "CONJOINT",
                ["MARIE"] = "CONJOINT",
                ["ENFANT"] = "ENFANT",
                ["FILS"] = "ENFANT",
                ["FILLE"] = "ENFANT",
                ["AMI"] = "AUTRE_CONTACT",
                ["VOISIN"] = "AUTRE_CONTACT",
                ["COLLEGUE"] = "AUTRE_CONTACT",
                ["AUTRE"] = "AUTRE"
            };
    }
}
