using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Models.Core
{
    /// <summary>Règles métier — adhésion niveau 2 (encodeur / Agent Administratif).</summary>
    public static class AdhesionNiveau2Regles
    {
        public const string StatutEnAttente = AdhesionStatutDossierRegles.EnAttente;
        public const string StatutValide = AdhesionStatutDossierRegles.Valide;

        public static List<string> Valider(
            AdhesionNiveau2EncodeurDto input,
            DateTime dateNaissanceAffilie,
            bool contactExistantEnBase = false)
        {
            var errors = new List<string>();

            var contactFourni = EstRenseigne(input.PersonneContact);
            if (!contactFourni && !contactExistantEnBase)
            {
                errors.Add("La personne de contact est obligatoire.");
            }
            else if (contactFourni)
            {
                errors.AddRange(ValiderPersonne(input.PersonneContact, "personne de contact"));
            }

            if (input.Dependants != null)
            {
                for (var i = 0; i < input.Dependants.Count; i++)
                {
                    var d = input.Dependants[i];
                    if (d == null)
                    {
                        errors.Add($"Le dépendant à l'index {i} est invalide.");
                        continue;
                    }

                    errors.AddRange(ValiderDependantChampsEncodeur(d, i + 1));
                }

                if (input.Dependants.Any(d => d != null))
                {
                    errors.AddRange(
                        PersonneEnChargeRegles.ValiderDependants(
                            input.Dependants.Where(d => d != null).Select(DependantValidationInput.FromNiveau2)!,
                            dateNaissanceAffilie));
                }
            }

            return errors;
        }

        /// <summary>
        /// Applique sur l'affilié les champs identité / adresse d'activité fournis dans le DTO
        /// (uniquement les valeurs non vides). Met à jour <see cref="Affilie.NomComplet"/> si nom/prénom/postnom changent.
        /// </summary>
        public static void AppliquerIdentiteActivite(Affilie affilie, AdhesionNiveau2EncodeurDto input)
        {
            if (!string.IsNullOrWhiteSpace(input.Nom))
                affilie.Nom = input.Nom.Trim();
            if (!string.IsNullOrWhiteSpace(input.Prenom))
                affilie.Prenom = input.Prenom.Trim();
            if (input.Postnom != null)
                affilie.Postnom = string.IsNullOrWhiteSpace(input.Postnom) ? null : input.Postnom.Trim();
            if (input.Telephone != null)
                affilie.Telephone = string.IsNullOrWhiteSpace(input.Telephone) ? null : input.Telephone.Trim();
            if (input.DateNaissance.HasValue)
                affilie.DateNaissance = input.DateNaissance.Value.Date;

            if (!string.IsNullOrWhiteSpace(input.CommuneActivite))
                affilie.CommuneActivite = input.CommuneActivite.Trim();
            if (!string.IsNullOrWhiteSpace(input.QuartierActivite))
                affilie.QuartierActivite = input.QuartierActivite.Trim();
            if (input.AvenueActivite != null)
                affilie.AvenueActivite = string.IsNullOrWhiteSpace(input.AvenueActivite) ? null : input.AvenueActivite.Trim();
            if (input.NumeroActivite != null)
                affilie.NumeroActivite = string.IsNullOrWhiteSpace(input.NumeroActivite) ? null : input.NumeroActivite.Trim();

            affilie.NomComplet = ConstruireNomComplet(affilie.Nom, affilie.Postnom, affilie.Prenom);
        }

        public static string ConstruireNomComplet(string? nom, string? postnom, string? prenom)
        {
            var parts = new[] { nom, postnom, prenom }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p!.Trim());
            return string.Join(" ", parts);
        }

        /// <summary>
        /// Prérequis des 4 blocs « dossier complet » pour <c>valider: true</c>.
        /// Attendu : <paramref name="affilie"/> déjà fusionné avec le DTO (<see cref="AppliquerIdentiteActivite"/>).
        /// </summary>
        public static List<string> ValiderDossierCompletPourValidation(
            Affilie affilie,
            AdhesionNiveau2EncodeurDto input,
            bool contactExistantEnBase = false)
        {
            var errors = new List<string>();

            if (!input.Valider)
                return errors;

            // 1. Identité
            if (string.IsNullOrWhiteSpace(affilie.Nom))
                errors.Add("L'identité de l'affilié est incomplète : le nom est obligatoire pour valider le dossier.");
            if (string.IsNullOrWhiteSpace(affilie.Prenom))
                errors.Add("L'identité de l'affilié est incomplète : le prénom est obligatoire pour valider le dossier.");
            if (affilie.DateNaissance == default)
                errors.Add("L'identité de l'affilié est incomplète : la date de naissance est obligatoire pour valider le dossier.");

            // 2. Adresse activité
            if (string.IsNullOrWhiteSpace(affilie.CommuneActivite))
                errors.Add("L'adresse d'activité est incomplète : la commune d'activité est obligatoire pour valider le dossier.");
            if (string.IsNullOrWhiteSpace(affilie.QuartierActivite))
                errors.Add("L'adresse d'activité est incomplète : le quartier d'activité est obligatoire pour valider le dossier.");

            // 3. Photo + pièce d'identité
            errors.AddRange(ValiderPiecesIdentitePourValidation(affilie, input));

            // 4. Personne à contacter — déjà contrôlée par Valider() à chaque PUT ;
            //    rappel explicite si absente au moment de valider.
            var contactFourni = EstRenseigne(input.PersonneContact);
            if (!contactFourni && !contactExistantEnBase)
                errors.Add("La personne de contact est obligatoire pour valider le dossier.");

            return errors;
        }

        public static bool EstDossierComplet(
            Affilie affilie,
            AdhesionNiveau2EncodeurDto input,
            bool contactExistantEnBase = false)
        {
            var probe = new AdhesionNiveau2EncodeurDto
            {
                Valider = true,
                PersonneContact = input.PersonneContact,
                PhotoBase64 = input.PhotoBase64,
                PhotoContentType = input.PhotoContentType,
                CarteIdentiteBase64 = input.CarteIdentiteBase64,
                CarteIdentiteContentType = input.CarteIdentiteContentType
            };
            return !ValiderDossierCompletPourValidation(affilie, probe, contactExistantEnBase).Any();
        }

        public static List<string> ValiderPiecesIdentitePourValidation(Affilie affilie, AdhesionNiveau2EncodeurDto input)
        {
            var errors = new List<string>();

            if (!input.Valider)
                return errors;

            errors.AddRange(ValiderContentTypeSiBase64Fourni(
                input.PhotoBase64, input.PhotoContentType, "photo"));
            errors.AddRange(ValiderContentTypeSiBase64Fourni(
                input.CarteIdentiteBase64, input.CarteIdentiteContentType, "carteIdentite"));

            var aPhoto = AffilieFichierHelper.ADesDonnees(affilie.PhotoData)
                || !string.IsNullOrWhiteSpace(input.PhotoBase64);
            var aCarte = AffilieFichierHelper.ADesDonnees(affilie.CarteIdentiteData)
                || !string.IsNullOrWhiteSpace(input.CarteIdentiteBase64);

            if (!aPhoto)
                errors.Add("La photo de l'affilié est obligatoire pour valider le dossier.");

            if (!aCarte)
                errors.Add("La carte d'identité est obligatoire pour valider le dossier.");

            return errors;
        }

        private static IEnumerable<string> ValiderContentTypeSiBase64Fourni(
            string? base64,
            string? contentType,
            string nomChamp)
        {
            if (!string.IsNullOrWhiteSpace(base64) && string.IsNullOrWhiteSpace(contentType))
            {
                yield return nomChamp == "photo"
                    ? "Le type de la photo est obligatoire (photoContentType) lorsque photoBase64 est fourni."
                    : "Le type de la carte d'identité est obligatoire (carteIdentiteContentType) lorsque carteIdentiteBase64 est fourni.";
            }
        }

        public static bool EstRenseigne(PersonneContactCreateDto? dto) =>
            dto != null && (
                !string.IsNullOrWhiteSpace(dto.NomComplet) ||
                !string.IsNullOrWhiteSpace(dto.LienParente) ||
                !string.IsNullOrWhiteSpace(dto.Adresse));

        public static PersonneContact MapToEntity(PersonneContactCreateDto dto) => new()
        {
            NomComplet = dto.NomComplet.Trim(),
            LienParente = LienParenteRegles.Normaliser(dto.LienParente),
            Adresse = dto.Adresse.Trim()
        };

        public static IEnumerable<string> ValiderPersonne(PersonneContactCreateDto p, string libelle)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(p.NomComplet))
                errors.Add($"Le nom complet de la {libelle} est obligatoire.");

            if (string.IsNullOrWhiteSpace(p.LienParente))
                errors.Add($"Le lien de parenté de la {libelle} est obligatoire.");
            else if (!LienParenteRegles.EstValide(p.LienParente))
                errors.Add($"Lien de parenté invalide pour la {libelle}. Valeurs acceptées : {string.Join(", ", LienParenteRegles.ValeursValides)}.");

            if (string.IsNullOrWhiteSpace(p.Adresse))
                errors.Add($"L'adresse de la {libelle} est obligatoire.");

            return errors;
        }

        private static IEnumerable<string> ValiderDependantChampsEncodeur(DependantNiveau2Dto d, int numero)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(d.NomComplet))
                errors.Add($"Le nom complet du dépendant #{numero} est obligatoire.");

            if (string.IsNullOrWhiteSpace(d.LienParente))
                errors.Add($"Le lien de parenté du dépendant #{numero} est obligatoire.");
            else if (!LienParenteRegles.EstValide(d.LienParente))
                errors.Add($"Lien de parenté invalide pour le dépendant #{numero}.");

            if (string.IsNullOrWhiteSpace(d.Adresse))
                errors.Add($"L'adresse du dépendant #{numero} est obligatoire.");

            return errors;
        }
    }
}
