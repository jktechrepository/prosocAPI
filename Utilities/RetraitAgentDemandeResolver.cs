using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Utilities
{
    public static class RetraitAgentDemandeResolver
    {
        public static RetraitDemandeResolutionResult Resoudre(
            DemandeRetraitAgentCreateDto dto,
            DateTime date,
            decimal? soldeDisponible,
            RetraitAgentOptions options)
        {
            var typeAutorise = RetraitAgentPeriodeHelper.GetTypeRetraitAutorise(date, options);
            if (typeAutorise == null)
            {
                return RetraitDemandeResolutionResult.Echec(
                    RetraitAgentPeriodeHelper.BuildMessage(date, estAutorise: false, options));
            }

            // Le typeRetrait client est ignoré : l'API impose toujours le type de la fenêtre courante.
            if (typeAutorise == RetraitAgentPeriodeHelper.TypePartiel)
            {
                if (!dto.MontantDemande.HasValue || dto.MontantDemande.Value <= 0)
                {
                    return RetraitDemandeResolutionResult.Echec(
                        "Le montant est obligatoire pour un retrait PARTIEL.");
                }

                if (dto.MontantDemande.Value < options.MontantMinimumPartiel)
                {
                    return RetraitDemandeResolutionResult.Echec(
                        $"Le montant minimum de retrait PARTIEL est de {options.MontantMinimumPartiel:N0} (devise principale).");
                }

                return RetraitDemandeResolutionResult.Ok(typeAutorise, dto.MontantDemande.Value);
            }

            if (soldeDisponible == null)
            {
                return RetraitDemandeResolutionResult.Echec(
                    "Aucun wallet en devise principale pour cet agent.");
            }

            if (soldeDisponible.Value < options.MontantMinimumPartiel)
            {
                return RetraitDemandeResolutionResult.Echec(
                    $"Solde insuffisant pour un retrait TOTAL (minimum {options.MontantMinimumPartiel:N0}).");
            }

            return RetraitDemandeResolutionResult.Ok(RetraitAgentPeriodeHelper.TypeTotal, soldeDisponible.Value);
        }

        public static RetraitDemandeResolutionResult ResoudreModeTest(
            DemandeRetraitAgentCreateDto dto,
            RetraitAgentOptions options)
        {
            var typeRetrait = NormaliserTypeRetraitClient(dto.TypeRetrait);

            if (!dto.MontantDemande.HasValue || dto.MontantDemande.Value <= 0)
            {
                return RetraitDemandeResolutionResult.Echec(
                    "Le montant est obligatoire pour un retrait PARTIEL.");
            }

            if (dto.MontantDemande.Value < options.MontantMinimumPartiel)
            {
                return RetraitDemandeResolutionResult.Echec(
                    $"Le montant minimum de retrait est de {options.MontantMinimumPartiel:N0} (devise principale).");
            }

            return RetraitDemandeResolutionResult.Ok(typeRetrait, dto.MontantDemande.Value);
        }

        private static string NormaliserTypeRetraitClient(string? typeClient)
        {
            if (typeClient == RetraitAgentPeriodeHelper.TypePartiel
                || typeClient == RetraitAgentPeriodeHelper.TypeTotal)
            {
                return typeClient;
            }

            return RetraitAgentPeriodeHelper.TypePartiel;
        }
    }

    public class RetraitDemandeResolutionResult
    {
        public bool Succes { get; init; }
        public string Message { get; init; } = string.Empty;
        public string TypeRetrait { get; init; } = string.Empty;
        public decimal MontantEffectif { get; init; }

        public static RetraitDemandeResolutionResult Ok(string typeRetrait, decimal montantEffectif) =>
            new()
            {
                Succes = true,
                TypeRetrait = typeRetrait,
                MontantEffectif = montantEffectif
            };

        public static RetraitDemandeResolutionResult Echec(string message) =>
            new()
            {
                Succes = false,
                Message = message
            };
    }
}
