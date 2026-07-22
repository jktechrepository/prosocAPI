namespace ProsocAPI.Services.Queue
{
    public interface INotificationQueueService
    {
        /// <summary>
        /// Mettre en file d'attente une notification de commission
        /// </summary>
        Task QueueCommissionNotificationAsync(int agentId, decimal commissionAmount, int collecteId, decimal ancienSolde, decimal nouveauSolde);

        /// <summary>
        /// Mettre en file d'attente une notification générale
        /// </summary>
        Task QueueNotificationAsync(int userId, string titre, string message, string type);

        /// <summary>
        /// Démarrer le traitement de la queue
        /// </summary>
        Task StartProcessingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Arrêter le traitement de la queue
        /// </summary>
        Task StopProcessingAsync();

        /// <summary>
        /// Obtenir les statistiques de la queue
        /// </summary>
        Task<NotificationQueueStatsDto> GetStatsAsync();
    }

    public class NotificationQueueStatsDto
    {
        public int QueueLength { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal ProcessingRate { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public DateTime LastProcessed { get; set; }
    }
}
