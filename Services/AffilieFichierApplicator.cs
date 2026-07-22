using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services;

public static class AffilieFichierApplicator
{
    public static void AppliquerCreation(Affilie affilie, AdhesionWithAffilieCreateDto input)
    {
        AppliquerPiecesIdentiteOptionnelles(
            affilie,
            input.PhotoBase64,
            input.PhotoContentType,
            input.CarteIdentiteBase64,
            input.CarteIdentiteContentType);
    }

    public static void AppliquerPiecesIdentiteOptionnelles(
        Affilie affilie,
        string? photoBase64,
        string? photoContentType,
        string? carteIdentiteBase64,
        string? carteIdentiteContentType)
    {
        var photo = AffilieFichierHelper.DepuisBase64Optionnel(
            photoBase64, photoContentType, "photo");
        if (photo != null)
        {
            affilie.PhotoData = photo.Data;
            affilie.PhotoContentType = photo.ContentType;
        }

        var carte = AffilieFichierHelper.DepuisBase64Optionnel(
            carteIdentiteBase64, carteIdentiteContentType, "carteIdentite", autoriserPdf: true);
        if (carte != null)
        {
            affilie.CarteIdentiteData = carte.Data;
            affilie.CarteIdentiteContentType = carte.ContentType;
        }
    }

    public static void AppliquerMiseAJourOptionnelle(Affilie affilie, AffilieUpdateDto dto)
    {
        AppliquerPiecesIdentiteOptionnelles(
            affilie,
            dto.PhotoBase64,
            dto.PhotoContentType,
            dto.CarteIdentiteBase64,
            dto.CarteIdentiteContentType);
    }

    public static void AppliquerCreationAffilie(Affilie affilie, AffilieCreateDto dto)
    {
        AppliquerPiecesIdentiteOptionnelles(
            affilie,
            dto.PhotoBase64,
            dto.PhotoContentType,
            dto.CarteIdentiteBase64,
            dto.CarteIdentiteContentType);
    }
}
