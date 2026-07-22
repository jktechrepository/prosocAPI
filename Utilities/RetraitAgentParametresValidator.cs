using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Utilities
{
    public static class RetraitAgentParametresValidator
    {
        public static string? Validate(RetraitAgentParametresUpdateDto dto)
        {
            if (dto.Fenetre1Debut < 1 || dto.Fenetre1Debut > 28)
                return "Fenetre1Debut doit être entre 1 et 28.";

            if (dto.Fenetre1Fin < dto.Fenetre1Debut || dto.Fenetre1Fin > 31)
                return "Fenetre1Fin doit être entre Fenetre1Debut et 31.";

            if (dto.Fenetre2DerniersJours < 1 || dto.Fenetre2DerniersJours > 15)
                return "Fenetre2DerniersJours doit être entre 1 et 15.";

            if (dto.MontantMinimumPartiel <= 0)
                return "MontantMinimumPartiel doit être strictement positif.";

            if (FenetresSeChevauchent(dto.Fenetre1Debut, dto.Fenetre1Fin, dto.Fenetre2DerniersJours))
                return "Les fenêtres 1 et 2 ne doivent pas se chevaucher (vérifier notamment les mois courts).";

            return null;
        }

        public static string? Validate(RetraitAgentOptions options) =>
            Validate(new RetraitAgentParametresUpdateDto
            {
                Fenetre1Debut = options.Fenetre1Debut,
                Fenetre1Fin = options.Fenetre1Fin,
                Fenetre2DerniersJours = options.Fenetre2DerniersJours,
                MontantMinimumPartiel = options.MontantMinimumPartiel
            });

        private static bool FenetresSeChevauchent(int fenetre1Debut, int fenetre1Fin, int fenetre2DerniersJours)
        {
            foreach (var mois in new[] { 2, 4, 6, 8, 10, 12 })
            {
                var annee = 2026;
                var dernierJour = DateTime.DaysInMonth(annee, mois);
                var fenetre2Debut = Math.Max(1, dernierJour - fenetre2DerniersJours + 1);

                if (fenetre1Fin >= fenetre2Debut)
                    return true;
            }

            return false;
        }
    }
}
