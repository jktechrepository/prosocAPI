using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.Mobile
{
    /// <summary>
    /// Configuration de l'application mobile
    /// </summary>
    public class MobileAppConfig
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(50)]
        public string AppName { get; set; } = "ProsocMobile";
        
        [Required, StringLength(20)]
        public string Platform { get; set; } = "Android"; // Android, iOS
        
        [Required, StringLength(20)]
        public string Version { get; set; } = "1.0.0";
        
        [Required, StringLength(20)]
        public string BuildNumber { get; set; } = "1";
        
        [StringLength(500)]
        public string? AppStoreUrl { get; set; }
        
        [StringLength(500)]
        public string? PlayStoreUrl { get; set; }
        
        [StringLength(1000)]
        public string? UpdateMessage { get; set; }
        
        public bool IsForceUpdateRequired { get; set; } = false;
        
        public bool IsMaintenanceMode { get; set; } = false;
        
        public DateTime? MaintenanceStart { get; set; }
        
        public DateTime? MaintenanceEnd { get; set; }
        
        [StringLength(1000)]
        public string? MaintenanceMessage { get; set; }
        
        public int MinSupportedVersion { get; set; } = 1;
        
        public bool IsFeatureEnabled(string feature)
        {
            return feature switch
            {
                "notifications" => true,
                "dashboard" => true,
                "commissions" => true,
                "profile" => true,
                "settings" => true,
                "offline_mode" => true,
                "biometric_auth" => false,
                "dark_mode" => true,
                "push_notifications" => true,
                "real_time_sync" => true,
                _ => false
            };
        }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;
    }

    /// <summary>
    /// Constantes pour les fonctionnalités mobiles
    /// </summary>
    public static class MobileFeatures
    {
        public const string NOTIFICATIONS = "notifications";
        public const string DASHBOARD = "dashboard";
        public const string COMMISSIONS = "commissions";
        public const string PROFILE = "profile";
        public const string SETTINGS = "settings";
        public const string OFFLINE_MODE = "offline_mode";
        public const string BIOMETRIC_AUTH = "biometric_auth";
        public const string DARK_MODE = "dark_mode";
        public const string PUSH_NOTIFICATIONS = "push_notifications";
        public const string REAL_TIME_SYNC = "real_time_sync";
        public const string LOCATION_TRACKING = "location_tracking";
        public const string VOICE_COMMANDS = "voice_commands";
        public const string CHAT_SUPPORT = "chat_support";
        public const string ANALYTICS = "analytics";
    }

    /// <summary>
    /// Plateformes supportées
    /// </summary>
    public static class MobilePlatforms
    {
        public const string ANDROID = "Android";
        public const string IOS = "iOS";
        public const string WINDOWS = "Windows";
        public const string WEB = "Web";
    }
}
