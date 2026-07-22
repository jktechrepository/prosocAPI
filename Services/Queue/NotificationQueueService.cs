using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ProsocAPI.Services.Queue
{
    public class NotificationQueueService : INotificationQueueService, IHostedService
    {
        private readonly ILogger<NotificationQueueService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<NotificationMessage> _queue;
        private readonly SemaphoreSlim _processingLock;
        private readonly Timer _statsTimer;
        private volatile bool _isProcessing;
        private volatile bool _isStopping;

        // Statistiques
        private long _processedCount;
        private long _failedCount;
        private readonly ConcurrentQueue<TimeSpan> _processingTimes;

        public NotificationQueueService(
            ILogger<NotificationQueueService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _queue = new ConcurrentQueue<NotificationMessage>();
            _processingLock = new SemaphoreSlim(1, 1);
            _processingTimes = new ConcurrentQueue<TimeSpan>();

            // Timer pour les statistiques (toutes les minutes)
            _statsTimer = new Timer(UpdateStats, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public async Task QueueCommissionNotificationAsync(
            int agentId, 
            decimal commissionAmount, 
            int collecteId, 
            decimal ancienSolde, 
            decimal nouveauSolde)
        {
            try
            {
                var message = new CommissionNotificationMessage
                {
                    Type = NotificationMessageType.Commission,
                    UserId = agentId,
                    Title = "🎉 Commission Reçue !",
                    Message = $"Commission de {commissionAmount:F2} reçue pour la collecte #{collecteId}",
                    NotificationType = "COMMISSION",
                    Priority = NotificationPriority.Normal,
                    AgentId = agentId,
                    CommissionAmount = commissionAmount,
                    CollecteId = collecteId,
                    AncienSolde = ancienSolde,
                    NouveauSolde = nouveauSolde
                };

                await EnqueueMessageAsync(message);
                _logger.LogDebug("Notification de commission mise en queue pour l'agent {AgentId}", agentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise en queue de la notification commission pour l'agent {AgentId}", agentId);
            }
        }

        public async Task QueueNotificationAsync(int userId, string titre, string message, string type)
        {
            try
            {
                var notificationMessage = new NotificationMessage
                {
                    Type = NotificationMessageType.General,
                    UserId = userId,
                    Title = titre,
                    Message = message,
                    NotificationType = type,
                    Priority = NotificationPriority.Normal
                };

                await EnqueueMessageAsync(notificationMessage);
                _logger.LogDebug("Notification mise en queue pour l'utilisateur {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise en queue de la notification pour l'utilisateur {UserId}", userId);
            }
        }

        public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
        {
            if (_isProcessing)
            {
                _logger.LogWarning("Le traitement de la queue est déjà en cours");
                return;
            }

            _isProcessing = true;
            _isStopping = false;
            _logger.LogInformation("Démarrage du traitement de la queue de notifications");

            // Démarrer le traitement en arrière-plan
            _ = Task.Run(() => ProcessQueueAsync(cancellationToken));
        }

        public async Task StopProcessingAsync()
        {
            if (!_isProcessing)
            {
                return;
            }

            _isStopping = true;
            _logger.LogInformation("Arrêt du traitement de la queue de notifications");

            // Attendre que le traitement en cours se termine
            await _processingLock.WaitAsync();
            _processingLock.Release();

            _isProcessing = false;
            _logger.LogInformation("Traitement de la queue arrêté");
        }

        public async Task<NotificationQueueStatsDto> GetStatsAsync()
        {
            return await Task.FromResult(new NotificationQueueStatsDto
            {
                QueueLength = _queue.Count,
                ProcessedCount = (int)_processedCount,
                FailedCount = (int)_failedCount,
                ProcessingRate = CalculateProcessingRate(),
                AverageProcessingTime = CalculateAverageProcessingTime(),
                LastProcessed = DateTime.Now // TODO: Garder la trace du dernier traitement
            });
        }

        private async Task EnqueueMessageAsync(NotificationMessage message)
        {
            // Ajouter à la queue en fonction de la priorité
            if (message.Priority == NotificationPriority.Critical)
            {
                // Les messages critiques sont traités en priorité
                await ProcessMessageImmediatelyAsync(message);
            }
            else
            {
                _queue.Enqueue(message);
                _logger.LogDebug("Message {MessageId} ajouté à la queue (taille: {QueueLength})", 
                    message.Id, _queue.Count);
            }
        }

        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Démarrage du worker de traitement de queue");

            while (!_isStopping && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var message))
                    {
                        await ProcessMessageAsync(message);
                    }
                    else
                    {
                        // Pas de messages, attendre un peu
                        await Task.Delay(100, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur dans le worker de traitement de queue");
                    await Task.Delay(1000, cancellationToken); // Attendre avant de réessayer
                }
            }

            _logger.LogInformation("Worker de traitement de queue arrêté");
        }

        private async Task ProcessMessageAsync(NotificationMessage message)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _processingLock.WaitAsync();
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    switch (message.Type)
                    {
                        case NotificationMessageType.Commission:
                            await ProcessCommissionMessageAsync(message, notificationService);
                            break;
                        default:
                            await ProcessGeneralMessageAsync(message, notificationService);
                            break;
                    }

                    Interlocked.Increment(ref _processedCount);
                    _logger.LogDebug("Message {MessageId} traité avec succès", message.Id);
                }
                finally
                {
                    _processingLock.Release();
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCount);
                _logger.LogError(ex, "Erreur lors du traitement du message {MessageId}", message.Id);

                // Réessayer plus tard si le nombre de tentatives n'est pas dépassé
                if (message.RetryCount < 3)
                {
                    message.RetryCount++;
                    message.NextRetryAt = DateTime.Now.AddMinutes(message.RetryCount * 5); // 5, 10, 15 minutes
                    _queue.Enqueue(message);
                    _logger.LogInformation("Message {MessageId} replanifié pour retry #{RetryCount}", message.Id, message.RetryCount);
                }
                else
                {
                    _logger.LogWarning("Message {MessageId} abandonné après {RetryCount} tentatives", message.Id, message.RetryCount);
                }
            }
            finally
            {
                stopwatch.Stop();
                _processingTimes.Enqueue(stopwatch.Elapsed);
                
                // Garder seulement les 1000 derniers temps de traitement pour éviter la surcharge mémoire
                if (_processingTimes.Count > 1000)
                {
                    _processingTimes.TryDequeue(out _);
                }
            }
        }

        private async Task ProcessCommissionMessageAsync(NotificationMessage message, INotificationService notificationService)
        {
            if (message is CommissionNotificationMessage commissionMessage)
            {
                // Utiliser le service de notification de commission existant
                using var scope = _serviceProvider.CreateScope();
                var commissionNotificationService = scope.ServiceProvider.GetRequiredService<ICommissionNotificationService>();

                // Recréer l'objet Collecte simplifié
                var collecte = new Collecte
                {
                    IdCollecte = commissionMessage.CollecteId,
                    Montant = commissionMessage.CollecteMontant,
                    AffilieId = commissionMessage.AgentId // TODO: Corriger avec le vrai affilié ID
                };

                await commissionNotificationService.NotifyCommissionEarnedAsync(
                    commissionMessage.AgentId,
                    commissionMessage.CommissionAmount,
                    collecte,
                    commissionMessage.AncienSolde,
                    commissionMessage.NouveauSolde);
            }
            else
            {
                await notificationService.SendToUserPreferredChannelsAsync(
                    message.UserId,
                    message.Title,
                    message.Message,
                    message.NotificationType);
            }
        }

        private async Task ProcessGeneralMessageAsync(NotificationMessage message, INotificationService notificationService)
        {
            await notificationService.SendToUserPreferredChannelsAsync(
                message.UserId,
                message.Title,
                message.Message,
                message.NotificationType);
        }

        private async Task ProcessMessageImmediatelyAsync(NotificationMessage message)
        {
            _logger.LogInformation("Traitement immédiat du message critique {MessageId}", message.Id);
            await ProcessMessageAsync(message);
        }

        private decimal CalculateProcessingRate()
        {
            var total = _processedCount + _failedCount;
            return total > 0 ? (decimal)_processedCount / total * 100 : 0;
        }

        private TimeSpan CalculateAverageProcessingTime()
        {
            if (_processingTimes.IsEmpty) return TimeSpan.Zero;

            var times = _processingTimes.ToArray();
            var totalMs = times.Average(t => t.TotalMilliseconds);
            return TimeSpan.FromMilliseconds(totalMs);
        }

        private void UpdateStats(object? state)
        {
            try
            {
                var stats = GetStatsAsync().GetAwaiter().GetResult();
                _logger.LogDebug("Stats Queue - Longueur: {Length}, Traités: {Processed}, Échecs: {Failed}, Taux: {Rate:F1}%",
                    stats.QueueLength, stats.ProcessedCount, stats.FailedCount, stats.ProcessingRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des statistiques");
            }
        }

        // Implémentation de IHostedService
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopProcessingAsync();
        }
    }
}
