using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class BonEnvoiDtoMapper
    {
        public static BonEnvoiReadDto ToReadDto(BonEnvoi bon) => new()
        {
            IdBonEnvoi = bon.IdBonEnvoi,
            NumeroBon = bon.NumeroBon,
            AffilieId = bon.AffilieId,
            AffilieNom = bon.Affilie != null
                ? $"{bon.Affilie.Nom} {bon.Affilie.Prenom}".Trim()
                : null,
            PrestationId = bon.PrestationId,
            PrestationNom = bon.Prestation?.NomPrestation,
            JetonMedicalId = bon.JetonMedicalId,
            JetonMedicalCode = bon.JetonMedical?.CodeJeton,
            DateEmission = bon.DateEmission,
            DateUtilisation = bon.DateUtilisation,
            EstUtilise = bon.EstUtilise,
            Statut = bon.Statut,
            DateCreation = bon.DateCreation,
            DateModification = bon.DateModification,
            QrCodePayload = bon.QrCodePayload,
            QrCodeImageBase64 = bon.QrCodeImageBase64
        };
    }
}
