using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace ProsocAPI.Services
{
    public class RefreshTokenService : IRefreshTokenRepository
    {
        private readonly ProsocDbContext _context;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(ProsocDbContext context, ILogger<RefreshTokenService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RefreshToken?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Utilisateur)
                .FirstOrDefaultAsync(rt => rt.IdRefreshToken == id, ct);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            var tokenHash = HashToken(token);
            return await _context.RefreshTokens
                .Include(rt => rt.Utilisateur)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);
        }

        public async Task<List<RefreshToken>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UtilisateurId == userId)
                .OrderByDescending(rt => rt.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            // Ne pas hasher le token car il est déjà hashé dans GenerateRefreshToken
            refreshToken.DateCreation = DateTime.UtcNow;
            
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Refresh token créé pour l'utilisateur {UserId}", refreshToken.UtilisateurId);
            return refreshToken;
        }

        public async Task<bool> RevokeAsync(int id, CancellationToken ct = default)
        {
            var refreshToken = await _context.RefreshTokens.FindAsync(new object[] { id }, ct);
            if (refreshToken == null)
                return false;

            refreshToken.DateRevocation = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Refresh token {Id} révoqué", id);
            return true;
        }

        public async Task<bool> RevokeAllForUserAsync(int userId, CancellationToken ct = default)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UtilisateurId == userId && rt.EstActif)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.DateRevocation = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Tous les refresh tokens révoqués pour l'utilisateur {UserId}", userId);
            return true;
        }

        public async Task<bool> DeleteExpiredAsync(CancellationToken ct = default)
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.EstExpire)
                .ToListAsync(ct);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Suppression de {Count} refresh tokens expirés", expiredTokens.Count);
            return true;
        }

        public async Task<bool> IsValidAsync(string token, CancellationToken ct = default)
        {
            var tokenHash = HashToken(token);
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

            return refreshToken?.EstActif ?? false;
        }

        // Générer un nouveau refresh token
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Hasher le token pour le stockage sécurisé
        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }

        // Créer un refresh token avec expiration par défaut (7 jours)
        public RefreshToken CreateRefreshToken(int userId, string? deviceInfo = null, string? ipAddress = null)
        {
            return new RefreshToken
            {
                UtilisateurId = userId,
                TokenHash = GenerateRefreshToken(),
                DateExpiration = DateTime.UtcNow.AddDays(7),
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress
            };
        }
    }
}
