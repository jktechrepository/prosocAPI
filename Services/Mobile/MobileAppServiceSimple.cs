using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Mobile;
using System.Text.Json;

namespace ProsocAPI.Services.Mobile
{
    public class MobileAppServiceSimple : IMobileAppServiceSimple
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<MobileAppServiceSimple> _logger;

        public MobileAppServiceSimple(ProsocDbContext db, ILogger<MobileAppServiceSimple> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<MobileAppConfig> GetAppConfigAsync(string platform, string version)
        {
            try
            {
                var config = await _db.MobileAppConfigs
                    .FirstOrDefaultAsync(c => c.Platform == platform && c.Statut);

                if (config == null)
                {
                    config = new MobileAppConfig
                    {
                        Platform = platform,
                        Version = version,
                        IsMaintenanceMode = false,
                        IsForceUpdateRequired = false
                    };
                }

                _logger.LogInformation("Configuration mobile récupérée pour {Platform} v{Version}", platform, version);
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la configuration mobile");
                throw;
            }
        }

        public async Task<MobileUserSession> CreateSessionAsync(
            int utilisateurId, 
            string deviceId, 
            string platform, 
            string appVersion, 
            string ipAddress, 
            string userAgent)
        {
            try
            {
                var sessionToken = Guid.NewGuid().ToString("N");
                var session = new MobileUserSession
                {
                    UtilisateurId = utilisateurId,
                    SessionToken = sessionToken,
                    DeviceId = deviceId,
                    Platform = platform,
                    AppVersion = appVersion,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DateCreation = DateTime.Now,
                    DateDerniereActivite = DateTime.Now,
                    DateExpiration = DateTime.Now.AddHours(24),
                    EstActive = true,
                    NombreRequetes = 0,
                    EstModeHorsLigne = false
                };

                _db.MobileUserSessions.Add(session);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Nouvelle session mobile créée pour l'utilisateur {UserId}", utilisateurId);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la session mobile");
                throw;
            }
        }

        public async Task<MobileUserSession> ValidateSessionAsync(string sessionToken, string deviceId)
        {
            try
            {
                var session = await _db.MobileUserSessions
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.DeviceId == deviceId && s.EstActive);

                if (session == null || DateTime.Now > session.DateExpiration)
                {
                    return null;
                }

                session.DateDerniereActivite = DateTime.Now;
                session.NombreRequetes++;
                await _db.SaveChangesAsync();

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la session mobile");
                throw;
            }
        }

        public async Task<bool> TerminateSessionAsync(string sessionToken)
        {
            try
            {
                var session = await _db.MobileUserSessions
                    .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

                if (session == null)
                    return false;

                session.EstActive = false;
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la terminaison de la session mobile");
                return false;
            }
        }

        public async Task<MobileSyncData> QueueSyncDataAsync(
            int utilisateurId, 
            string entityType, 
            int entityId, 
            string operation, 
            string data)
        {
            try
            {
                var syncData = new MobileSyncData
                {
                    UtilisateurId = utilisateurId,
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation,
                    Data = data,
                    SyncStatus = "PENDING",
                    DateCreation = DateTime.Now,
                    NombreTentatives = 0,
                    EstSynchronise = false
                };

                _db.MobileSyncData.Add(syncData);
                await _db.SaveChangesAsync();

                return syncData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise en queue des données de synchronisation");
                throw;
            }
        }

        public async Task<List<MobileSyncData>> GetPendingSyncDataAsync(int utilisateurId)
        {
            try
            {
                return await _db.MobileSyncData
                    .Where(s => s.UtilisateurId == utilisateurId && 
                               s.SyncStatus == "PENDING" && 
                               s.Statut)
                    .OrderBy(s => s.DateCreation)
                    .Take(100)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des données de synchronisation en attente");
                throw;
            }
        }

        public async Task<bool> MarkSyncDataAsSyncedAsync(int syncDataId)
        {
            try
            {
                var syncData = await _db.MobileSyncData.FindAsync(syncDataId);
                if (syncData == null)
                    return false;

                syncData.SyncStatus = "SYNCED";
                syncData.DateSynchronisation = DateTime.Now;
                syncData.EstSynchronise = true;

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage des données comme synchronisées");
                return false;
            }
        }

        public async Task<bool> SendPushNotificationAsync(int utilisateurId, string titre, string message, object? data = null)
        {
            try
            {
                _logger.LogInformation("Notification push simulée pour l'utilisateur {UserId}", utilisateurId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification push");
                return false;
            }
        }

        public async Task<int> SendBulkPushNotificationAsync(
            List<int> utilisateurIds, 
            string titre, 
            string message, 
            object? data = null)
        {
            try
            {
                _logger.LogInformation("Notification push bulk simulée pour {Count} utilisateurs", utilisateurIds.Count);
                return utilisateurIds.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification push bulk");
                return 0;
            }
        }

        public async Task<int> CleanupExpiredSessionsAsync()
        {
            try
            {
                var expiredSessions = await _db.MobileUserSessions
                    .Where(s => s.EstActive && s.DateExpiration < DateTime.Now)
                    .ToListAsync();

                foreach (var session in expiredSessions)
                {
                    session.EstActive = false;
                }

                await _db.SaveChangesAsync();
                return expiredSessions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du nettoyage des sessions expirées");
                return 0;
            }
        }

        public async Task<MobileUsageStatsDto> GetUsageStatsAsync(int utilisateurId, DateTime debut, DateTime fin)
        {
            try
            {
                var sessions = await _db.MobileUserSessions
                    .Where(s => s.UtilisateurId == utilisateurId && 
                               s.DateCreation >= debut && 
                               s.DateCreation <= fin)
                    .ToListAsync();

                return new MobileUsageStatsDto
                {
                    TotalSessions = sessions.Count,
                    AverageSessionDuration = TimeSpan.Zero,
                    TotalNotifications = 0,
                    TotalSyncOperations = 0,
                    FeatureUsage = new Dictionary<string, int>(),
                    LastActivity = sessions.OrderByDescending(s => s.DateDerniereActivite).FirstOrDefault()?.DateDerniereActivite ?? DateTime.MinValue,
                    MostUsedPlatform = sessions.GroupBy(s => s.Platform).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "Unknown",
                    MostUsedVersion = sessions.GroupBy(s => s.AppVersion).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul des statistiques d'utilisation mobile");
                throw;
            }
        }

        public async Task<MobileSyncResultDto> SyncUserDataAsync(int utilisateurId, string lastSyncDate)
        {
            try
            {
                var pendingData = await GetPendingSyncDataAsync(utilisateurId);
                var syncedCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var data in pendingData)
                {
                    try
                    {
                        await MarkSyncDataAsSyncedAsync(data.Id);
                        syncedCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        errors.Add($"Erreur pour {data.EntityType} {data.EntityId}: {ex.Message}");
                    }
                }

                return new MobileSyncResultDto
                {
                    Success = failedCount == 0,
                    SyncedCount = syncedCount,
                    FailedCount = failedCount,
                    Errors = errors,
                    SyncDate = DateTime.Now,
                    NextSyncToken = Guid.NewGuid().ToString("N")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation des données utilisateur");
                throw;
            }
        }
    }
}
