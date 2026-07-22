using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using QRCoder;

namespace ProsocAPI.Services
{
    public class BonEnvoiQrCodeService : IBonEnvoiQrCodeService
    {
        private readonly BonEnvoiQrOptions _options;

        public BonEnvoiQrCodeService(IOptions<BonEnvoiQrOptions> options)
        {
            _options = options.Value;
        }

        public string BuildSignedPayload(BonEnvoi bon, DateTime expiresAtUtc)
        {
            var expUnix = new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds();
            var data = $"{bon.IdBonEnvoi}|{bon.NumeroBon}|{expUnix}";
            var sig = ComputeSignature(data);
            var token = new
            {
                v = 1,
                id = bon.IdBonEnvoi,
                nb = bon.NumeroBon,
                exp = expUnix,
                sig
            };
            return JsonSerializer.Serialize(token);
        }

        public BonEnvoiQrClaims? TryValidatePayload(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(payload.Trim());
                var root = doc.RootElement;
                if (!root.TryGetProperty("v", out var v) || v.GetInt32() != 1)
                    return null;
                if (!root.TryGetProperty("id", out var idEl) || !root.TryGetProperty("nb", out var nbEl)
                    || !root.TryGetProperty("exp", out var expEl) || !root.TryGetProperty("sig", out var sigEl))
                    return null;

                var id = idEl.GetInt32();
                var nb = nbEl.GetString() ?? string.Empty;
                var expUnix = expEl.GetInt64();
                var sig = sigEl.GetString() ?? string.Empty;

                var data = $"{id}|{nb}|{expUnix}";
                if (!ConstantTimeEquals(ComputeSignature(data), sig))
                    return null;

                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                if (expiresAt < DateTime.UtcNow)
                    return null;

                return new BonEnvoiQrClaims
                {
                    IdBonEnvoi = id,
                    NumeroBon = nb,
                    ExpiresAtUtc = expiresAt
                };
            }
            catch
            {
                return null;
            }
        }

        public string GenerateImageBase64(string payload)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            var bytes = qr.GetGraphic(20);
            return Convert.ToBase64String(bytes);
        }

        public Task ApplyQrToBonAsync(BonEnvoi bon, CancellationToken ct = default)
        {
            var expiresAt = DateTime.UtcNow.AddDays(Math.Max(1, _options.ValidityDays));
            var payload = BuildSignedPayload(bon, expiresAt);
            bon.QrCodePayload = payload;
            bon.QrCodeImageBase64 = GenerateImageBase64(payload);
            bon.DateModification = DateTime.Now;
            return Task.CompletedTask;
        }

        private string ComputeSignature(string data)
        {
            var key = string.IsNullOrWhiteSpace(_options.SigningKey)
                ? "Prosoc-BonEnvoi-Qr-Fallback-Change-In-Production"
                : _options.SigningKey;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
