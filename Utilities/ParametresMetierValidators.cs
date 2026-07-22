using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Utilities
{
    public static class AgentMaashParametresValidator
    {
        public static string? Validate(AgentMaashParametresUpdateDto dto)
        {
            if (dto.MontantRetenueUsd <= 0)
                return "MontantRetenueUsd doit être strictement positif.";

            if (dto.DeviseId <= 0)
                return "DeviseId invalide.";

            if (dto.CodesCategoriesEligibles == null || dto.CodesCategoriesEligibles.Length == 0)
                return "Au moins une catégorie agent éligible est requise.";

            if (dto.CodesCategoriesEligibles.Any(string.IsNullOrWhiteSpace))
                return "Les codes catégories ne peuvent pas être vides.";

            if (string.IsNullOrWhiteSpace(dto.NomProduitMaash))
                return "NomProduitMaash est requis.";

            if (dto.JourExecution < 1 || dto.JourExecution > 28)
                return "JourExecution doit être entre 1 et 28.";

            if (dto.HeureExecution < 0 || dto.HeureExecution > 23)
                return "HeureExecution doit être entre 0 et 23.";

            if (dto.IntervalleControleMinutes < 1)
                return "IntervalleControleMinutes doit être au moins 1.";

            return null;
        }

        public static string? ValidateDeviseExists(bool exists) =>
            exists ? null : "DeviseId introuvable ou inactive.";

        public static string? ValidateCategoriesExist(bool allExist) =>
            allExist ? null : "Un ou plusieurs codes catégories agent sont invalides.";
    }

    public static class ArrieresParametresValidator
    {
        public static string? Validate(ArrieresParametresUpdateDto dto)
        {
            if (dto.HeureExecution < 0 || dto.HeureExecution > 23)
                return "HeureExecution doit être entre 0 et 23.";

            if (dto.MinuteExecution < 0 || dto.MinuteExecution > 59)
                return "MinuteExecution doit être entre 0 et 59.";

            if (dto.IntervalleControleMinutes < 1)
                return "IntervalleControleMinutes doit être au moins 1.";

            if (dto.JourEcheanceMensuelle < 1 || dto.JourEcheanceMensuelle > 28)
                return "JourEcheanceMensuelle doit être entre 1 et 28.";

            return null;
        }
    }

    public static class PenaliteParametresValidator
    {
        public static string? Validate(PenaliteParametresUpdateDto dto)
        {
            if (dto.DelaiGraceJours < 0)
                return "DelaiGraceJours ne peut pas être négatif.";

            if (string.IsNullOrWhiteSpace(dto.FraisPenaliteCode))
                return "FraisPenaliteCode est requis.";

            return null;
        }

        public static string? ValidateFraisExists(bool exists) =>
            exists ? null : "FraisPenaliteCode introuvable dans le catalogue Frais.";
    }
}
