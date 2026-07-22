using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public sealed class BonEnvoiQrClaims
    {
        public int IdBonEnvoi { get; init; }
        public string NumeroBon { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
    }

    public interface IBonEnvoiQrCodeService
    {
        string BuildSignedPayload(BonEnvoi bon, DateTime expiresAtUtc);
        BonEnvoiQrClaims? TryValidatePayload(string? payload);
        string GenerateImageBase64(string payload);
        Task ApplyQrToBonAsync(BonEnvoi bon, CancellationToken ct = default);
    }
}
