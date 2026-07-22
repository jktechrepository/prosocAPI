using System.Text.RegularExpressions;

namespace Prosoc.Utilities;

public sealed record AffilieFichierBinaire(byte[] Data, string ContentType);

public static class AffilieFichierHelper
{
    public const int TailleMaxOctets = 1 * 1024 * 1024;

    private static readonly HashSet<string> TypesMimeImages =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp"
        };

    private static readonly HashSet<string> TypesMimeCarteIdentite =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp",
            "application/pdf"
        };

    public static AffilieFichierBinaire? DepuisBase64Optionnel(
        string? base64,
        string? contentType,
        string nomChamp,
        bool autoriserPdf = false)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return null;

        return DepuisBase64(base64, contentType, nomChamp, autoriserPdf);
    }

    public static AffilieFichierBinaire DepuisBase64(
        string? base64,
        string? contentType,
        string nomChamp,
        bool autoriserPdf = false)
    {
        if (string.IsNullOrWhiteSpace(base64))
            throw new ArgumentException($"Le fichier {nomChamp} est obligatoire (base64).");

        var donnees = DecoderBase64(base64, nomChamp);
        var mime = NormaliserContentType(contentType, nomChamp, autoriserPdf);
        ValiderTaille(donnees, nomChamp);
        ValiderTypeMime(mime, nomChamp, autoriserPdf);
        return new AffilieFichierBinaire(donnees, mime);
    }

    public static async Task<AffilieFichierBinaire?> DepuisFormFileOptionnelAsync(
        IFormFile? fichier,
        string nomChamp,
        bool autoriserPdf = false,
        CancellationToken ct = default)
    {
        if (fichier == null || fichier.Length == 0)
            return null;

        return await DepuisFormFileAsync(fichier, nomChamp, autoriserPdf, ct);
    }

    public static async Task<AffilieFichierBinaire> DepuisFormFileAsync(
        IFormFile? fichier,
        string nomChamp,
        bool autoriserPdf = false,
        CancellationToken ct = default)
    {
        if (fichier == null || fichier.Length == 0)
            throw new ArgumentException($"Le fichier {nomChamp} est obligatoire.");

        if (fichier.Length > TailleMaxOctets)
            throw new ArgumentException(
                $"Le fichier {nomChamp} dépasse la taille maximale de {TailleMaxOctets / (1024 * 1024)} Mo.");

        await using var ms = new MemoryStream();
        await fichier.CopyToAsync(ms, ct);
        var donnees = ms.ToArray();

        var mime = NormaliserContentType(
            string.IsNullOrWhiteSpace(fichier.ContentType)
                ? DevinerMimeDepuisExtension(fichier.FileName)
                : fichier.ContentType,
            nomChamp,
            autoriserPdf);

        ValiderTypeMime(mime, nomChamp, autoriserPdf);
        return new AffilieFichierBinaire(donnees, mime);
    }

    public static bool ADesDonnees(byte[]? data) => data != null && data.Length > 0;

    public static string? VersBase64(byte[]? data) =>
        ADesDonnees(data) ? Convert.ToBase64String(data!) : null;

    private static byte[] DecoderBase64(string base64, string nomChamp)
    {
        var payload = base64.Trim();
        var match = Regex.Match(payload, @"^data:([^;]+);base64,(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
            payload = match.Groups[2].Value;

        // Espaces / retours ligne (copier-coller Swagger, Postman, etc.)
        payload = Regex.Replace(payload, @"\s+", string.Empty);

        // Base64 URL-safe (certains clients front)
        payload = payload.Replace('-', '+').Replace('_', '/');
        var mod = payload.Length % 4;
        if (mod > 0)
            payload += new string('=', 4 - mod);

        try
        {
            return Convert.FromBase64String(payload);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                $"Contenu base64 invalide pour {nomChamp}. " +
                "Envoyez une chaîne base64 pure ou data:image/jpeg;base64,... (photo/carte d'identité obligatoires).");
        }
    }

    private static string NormaliserContentType(
        string? contentType,
        string nomChamp,
        bool autoriserPdf)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException(
                $"Le type MIME ({nomChamp}ContentType) est obligatoire (ex. image/jpeg).");

        var mime = contentType.Split(';')[0].Trim().ToLowerInvariant();
        ValiderTypeMime(mime, nomChamp, autoriserPdf);
        return mime;
    }

    private static void ValiderTaille(byte[] donnees, string nomChamp)
    {
        if (donnees.Length > TailleMaxOctets)
            throw new ArgumentException(
                $"Le fichier {nomChamp} dépasse la taille maximale de {TailleMaxOctets / (1024 * 1024)} Mo.");
    }

    private static void ValiderTypeMime(string mime, string nomChamp, bool autoriserPdf)
    {
        var permis = autoriserPdf ? TypesMimeCarteIdentite : TypesMimeImages;
        if (!permis.Contains(mime))
            throw new ArgumentException(
                $"Type MIME non autorisé pour {nomChamp}: {mime}. Types acceptés: {string.Join(", ", permis)}.");
    }

    private static string DevinerMimeDepuisExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
