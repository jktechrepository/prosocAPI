using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Models.Core
{
    /// <summary>Remarque 4 — personnes à charge et âge d'adhésion.</summary>
    public static class PersonneEnChargeRegles
    {
        public const int AgeMinAdherent = 18;
        /// <summary>À partir de 55 ans, l'affilié ne peut plus adhérer en titulaire.</summary>
        public const int AgeMaxAdherent = 54;

        public const int AgeMaxEnfantSansJustificatif = 18;
        public const int AgeMaxEnfantEtudiant = 25;

        private static readonly HashSet<string> LiensEnfant = new(
            LienParenteRegles.LiensEnfant,
            StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> LiensConjoint = new(
            LienParenteRegles.LiensConjoint,
            StringComparer.OrdinalIgnoreCase);

        public static int CalculerAge(DateTime dateNaissance)
        {
            var today = DateTime.Today;
            var age = today.Year - dateNaissance.Year;
            if (dateNaissance.Date > today.AddYears(-age))
                age--;
            return age;
        }

        public static bool EstLienEnfant(string? lienParente)
        {
            if (string.IsNullOrWhiteSpace(lienParente))
                return false;
            return LiensEnfant.Contains(lienParente.Trim().ToUpperInvariant());
        }

        public static List<string> ValiderAgeAdherent(DateTime dateNaissance)
        {
            var errors = new List<string>();

            if (dateNaissance == default)
            {
                errors.Add("La date de naissance est obligatoire.");
                return errors;
            }

            if (dateNaissance.Date > DateTime.Today)
            {
                errors.Add("La date de naissance ne peut pas être dans le futur.");
                return errors;
            }

            var age = CalculerAge(dateNaissance);

            if (age < AgeMinAdherent)
                errors.Add($"L'affilié doit avoir au moins {AgeMinAdherent} ans pour adhérer. Âge actuel : {age} ans.");

            if (age > AgeMaxAdherent)
            {
                errors.Add(
                    $"Une personne de {age} ans ne peut pas prendre d'adhésion en titulaire (limite : {AgeMaxAdherent} ans). " +
                    "Elle doit être déclarée comme personne à charge d'un autre affilié.");
            }

            return errors;
        }

        public static List<string> ValiderDependant(
            DependantValidationInput dependant,
            DateTime dateNaissanceAffilie,
            int numero = 1)
        {
            var errors = new List<string>();
            var libelle = string.IsNullOrWhiteSpace(dependant.Nom)
                ? $"dépendant #{numero}"
                : $"'{dependant.Nom}'";

            if (string.IsNullOrWhiteSpace(dependant.Nom))
                errors.Add($"Le nom du dépendant #{numero} est obligatoire.");

            if (string.IsNullOrWhiteSpace(dependant.LienParente))
            {
                errors.Add($"Le lien de parenté est obligatoire pour le dépendant {libelle}.");
                return errors;
            }

            var lien = LienParenteRegles.Normaliser(dependant.LienParente);
            if (!LienParenteRegles.EstValide(lien))
            {
                errors.Add(
                    $"Lien de parenté invalide pour {libelle}. Valeurs acceptées : {string.Join(", ", LienParenteRegles.ValeursValides)}.");
                return errors;
            }

            if (EstLienEnfant(lien))
                errors.AddRange(ValiderEnfant(dependant, libelle, dateNaissanceAffilie));
            else
                errors.AddRange(ValiderAutreDependant(dependant, libelle, lien, dateNaissanceAffilie));

            return errors;
        }

        public static List<string> ValiderDependants(
            IEnumerable<DependantValidationInput>? dependants,
            DateTime dateNaissanceAffilie)
        {
            if (dependants == null)
                return new List<string>();

            var errors = new List<string>();
            var i = 0;
            foreach (var d in dependants)
            {
                i++;
                if (d == null)
                {
                    errors.Add($"Le dépendant à l'index {i - 1} est invalide.");
                    continue;
                }

                errors.AddRange(ValiderDependant(d, dateNaissanceAffilie, i));
            }

            return errors;
        }

        private static IEnumerable<string> ValiderEnfant(
            DependantValidationInput dependant,
            string libelle,
            DateTime dateNaissanceAffilie)
        {
            var errors = new List<string>();

            if (!dependant.DateNaissance.HasValue)
            {
                errors.Add($"La date de naissance est obligatoire pour l'enfant {libelle}.");
                return errors;
            }

            var date = dependant.DateNaissance.Value;
            if (date.Date > DateTime.Today)
            {
                errors.Add($"La date de naissance de l'enfant {libelle} ne peut pas être dans le futur.");
                return errors;
            }

            var age = CalculerAge(date);
            var ageAffilie = CalculerAge(dateNaissanceAffilie);

            if (age < 0)
                errors.Add($"L'enfant {libelle} ne peut pas avoir un âge négatif.");

            if (age > AgeMaxEnfantEtudiant)
            {
                errors.Add(
                    $"L'enfant {libelle} a {age} ans : l'âge maximum pour une personne à charge enfant est {AgeMaxEnfantEtudiant} ans.");
            }

            if (age > ageAffilie)
            {
                errors.Add(
                    $"L'enfant {libelle} ({age} ans) ne peut pas être plus âgé que l'affilié titulaire ({ageAffilie} ans).");
            }

            if (age >= AgeMaxEnfantSansJustificatif && age <= AgeMaxEnfantEtudiant)
            {
                if (string.IsNullOrWhiteSpace(dependant.CertificatScolariteBase64))
                {
                    errors.Add(
                        $"Pour l'enfant {libelle} ({age} ans), un certificat de scolarité est obligatoire " +
                        $"(tranche {AgeMaxEnfantSansJustificatif}–{AgeMaxEnfantEtudiant} ans : justificatif d'études).");
                }
                else if (string.IsNullOrWhiteSpace(dependant.CertificatScolariteContentType))
                {
                    errors.Add(
                        $"Le type MIME du certificat de scolarité est obligatoire pour l'enfant {libelle} (certificatScolariteContentType).");
                }
            }

            return errors;
        }

        private static IEnumerable<string> ValiderAutreDependant(
            DependantValidationInput dependant,
            string libelle,
            string lien,
            DateTime dateNaissanceAffilie)
        {
            var errors = new List<string>();

            if (!dependant.DateNaissance.HasValue)
                return errors;

            var date = dependant.DateNaissance.Value;
            if (date.Date > DateTime.Today)
            {
                errors.Add($"La date de naissance du dépendant {libelle} ne peut pas être dans le futur.");
                return errors;
            }

            var age = CalculerAge(date);
            var ageAffilie = CalculerAge(dateNaissanceAffilie);

            if (LiensConjoint.Contains(lien) && age < 15)
                errors.Add($"Le conjoint {libelle} doit avoir au moins 15 ans. Âge actuel : {age} ans.");

            if (age > ageAffilie)
            {
                errors.Add(
                    $"Le dépendant {libelle} ({age} ans) ne peut pas être plus âgé que l'affilié titulaire ({ageAffilie} ans).");
            }

            if (age > 120)
                errors.Add($"L'âge du dépendant {libelle} ({age} ans) n'est pas raisonnable.");

            return errors;
        }
    }

    public class DependantValidationInput
    {
        public string Nom { get; set; } = string.Empty;
        public string? LienParente { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string? CertificatScolariteBase64 { get; set; }
        public string? CertificatScolariteContentType { get; set; }

        public static DependantValidationInput FromCreate(DependantCreateDto d) => new()
        {
            Nom = d.Nom,
            LienParente = d.LienParente,
            DateNaissance = d.DateNaissance,
            CertificatScolariteBase64 = d.CertificatScolariteBase64,
            CertificatScolariteContentType = d.CertificatScolariteContentType
        };

        public static DependantValidationInput FromNiveau2(DependantNiveau2Dto d) => new()
        {
            Nom = d.NomComplet,
            LienParente = d.LienParente,
            DateNaissance = d.DateNaissance,
            CertificatScolariteBase64 = d.CertificatScolariteBase64,
            CertificatScolariteContentType = d.CertificatScolariteContentType
        };
    }
}
