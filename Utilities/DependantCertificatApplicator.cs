using ProsocAPI.Models.Core;

namespace Prosoc.Utilities;

public static class DependantCertificatApplicator
{
    public static void Appliquer(
        Dependant dependant,
        string? certificatBase64,
        string? certificatContentType)
    {
        if (string.IsNullOrWhiteSpace(certificatBase64))
            return;

        var fichier = AffilieFichierHelper.DepuisBase64(
            certificatBase64,
            certificatContentType,
            "certificatScolarite",
            autoriserPdf: true);

        dependant.CertificatScolariteData = fichier.Data;
        dependant.CertificatScolariteContentType = fichier.ContentType;
    }
}
