using ProsocAPI.Models.Mobile;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Services.Mobile
{
    public interface IMobileAppServiceSimple
    {
        Task<MobileAppConfig> GetAppConfigAsync(string platform, string version);
        Task<MobileUserSession> CreateSessionAsync(int utilisateurId, string deviceId, string platform, string appVersion, string ipAddress, string userAgent);
        Task<MobileUserSession> ValidateSessionAsync(string sessionToken, string deviceId);
        Task<bool> TerminateSessionAsync(string sessionToken);
        Task<MobileSyncData> QueueSyncDataAsync(int utilisateurId, string entityType, int entityId, string operation, string data);
        Task<List<MobileSyncData>> GetPendingSyncDataAsync(int utilisateurId);
        Task<bool> MarkSyncDataAsSyncedAsync(int syncDataId);
        Task<bool> SendPushNotificationAsync(int utilisateurId, string titre, string message, object? data = null);
        Task<int> SendBulkPushNotificationAsync(List<int> utilisateurIds, string titre, string message, object? data = null);
        Task<int> CleanupExpiredSessionsAsync();
        Task<MobileUsageStatsDto> GetUsageStatsAsync(int utilisateurId, DateTime debut, DateTime fin);
        Task<MobileSyncResultDto> SyncUserDataAsync(int utilisateurId, string lastSyncDate);
    }
}
