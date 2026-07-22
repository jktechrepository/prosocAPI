namespace ProsocAPI.Services.Queue
{
    /// <summary>
    /// Message de notification pour la queue
    /// </summary>
    public class NotificationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public NotificationMessageType Type { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = "INFO";
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Message spécialisé pour les notifications de commission
    /// </summary>
    public class CommissionNotificationMessage : NotificationMessage
    {
        public int AgentId { get; set; }
        public decimal CommissionAmount { get; set; }
        public int CollecteId { get; set; }
        public decimal AncienSolde { get; set; }
        public decimal NouveauSolde { get; set; }
        public string AffilieNom { get; set; } = string.Empty;
        public decimal CollecteMontant { get; set; }
        public string Devise { get; set; } = "USD";
    }

    public enum NotificationMessageType
    {
        General,
        Commission,
        Adhesion,
        Payment,
        System
    }

    public enum NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
}
