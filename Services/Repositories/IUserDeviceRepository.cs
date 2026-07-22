using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Services.Repositories
{
    public interface IUserDeviceRepository
    {
        Task<UserDevice?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<UserDevice>> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<UserDevice?> GetByFcmTokenAsync(string fcmToken, CancellationToken ct = default);
        Task<UserDevice> CreateAsync(UserDevice device, CancellationToken ct = default);
        Task<UserDevice> UpdateAsync(UserDevice device, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> UpdateLastUsageAsync(int id, CancellationToken ct = default);
        Task<UserDevice> RegisterOrUpdateDeviceAsync(int userId, string fcmToken, 
            string? deviceType = null, string? deviceModel = null, string? osVersion = null, 
            CancellationToken ct = default);
    }
}
