namespace ProsocAPI.Models.Mobile
{
    public class MobileUsageStatsDto
    {
        public int TotalSessions { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public int TotalNotifications { get; set; }
        public int TotalSyncOperations { get; set; }
        public Dictionary<string, int> FeatureUsage { get; set; } = new();
        public DateTime LastActivity { get; set; }
        public string MostUsedPlatform { get; set; } = string.Empty;
        public string MostUsedVersion { get; set; } = string.Empty;
    }

    public class MobileSyncResultDto
    {
        public bool Success { get; set; }
        public int SyncedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncDate { get; set; }
        public string NextSyncToken { get; set; } = string.Empty;
    }
}
