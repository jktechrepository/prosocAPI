using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Services.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task<List<RefreshToken>> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken ct = default);
        Task<bool> RevokeAsync(int id, CancellationToken ct = default);
        Task<bool> RevokeAllForUserAsync(int userId, CancellationToken ct = default);
        Task<bool> DeleteExpiredAsync(CancellationToken ct = default);
        Task<bool> IsValidAsync(string token, CancellationToken ct = default);
    }
}
