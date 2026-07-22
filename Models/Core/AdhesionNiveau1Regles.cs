using ProsocAPI.Helpers;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Models.Core
{
    /// <summary>Règles métier — adhésion niveau 1 (enregistrement par Agent de Terrain).</summary>
    public static class AdhesionNiveau1Regles
    {
        public static List<string> ValiderChampsObligatoires(AdhesionWithAffilieCreateDto input)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(input.Nom))
                errors.Add("Le nom est obligatoire (nom complet).");
            if (string.IsNullOrWhiteSpace(input.Prenom))
                errors.Add("Le prénom est obligatoire (nom complet).");

            if (!string.IsNullOrWhiteSpace(input.PhotoBase64) && string.IsNullOrWhiteSpace(input.PhotoContentType))
                errors.Add("Le type de la photo est obligatoire (photoContentType) lorsque photoBase64 est fourni.");

            if (!string.IsNullOrWhiteSpace(input.CarteIdentiteBase64) && string.IsNullOrWhiteSpace(input.CarteIdentiteContentType))
                errors.Add("Le type de la carte d'identité est obligatoire (carteIdentiteContentType) lorsque carteIdentiteBase64 est fourni.");

            if (string.IsNullOrWhiteSpace(input.ProvinceResidence))
                errors.Add("La province de résidence est obligatoire.");

            if (input.Collectes == null || !input.Collectes.Any())
            {
                errors.Add("Au moins une collecte est requise.");
                return errors;
            }

            // FRAIS seuls autorisés ; Souscription / Cotisation optionnelles (validées si présentes).

            var souscriptionSansPrestation = input.Collectes.Any(c =>
                c != null
                && c.TypeCollecte == TypeCollecte.Souscription
                && (c.Souscription == null || c.Souscription.PrestationId <= 0));

            if (souscriptionSansPrestation)
                errors.Add("Chaque collecte Souscription doit inclure souscription.prestationId.");

            var flexPayEnAttente = input.Collectes.All(c =>
                c != null && MethodePaiementHelper.IsFlexPay(c.ModePaiement));

            var aConfirmationPaiement = input.Collectes.Any(c =>
                c != null && EstPaiementConfirme(c.StatutPaiement));

            if (!flexPayEnAttente && !aConfirmationPaiement)
                errors.Add("Au moins une collecte doit avoir un statutPaiement confirmé (VALIDE).");

            if (AdhesionNiveau2Regles.EstRenseigne(input.PersonneContact))
                errors.AddRange(AdhesionNiveau2Regles.ValiderPersonne(input.PersonneContact!, "personne de contact"));

            return errors;
        }

        public static bool EstPaiementConfirme(string? statutPaiement) =>
            CollecteStatutPaiementRegles.EstValide(statutPaiement);
    }
}
