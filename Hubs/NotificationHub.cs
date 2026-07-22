using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Core;
using Prosoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ProsocAPI.Hubs
{
    /// <summary>
    /// Hub SignalR pour les notifications en temps réel
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly ProsocDbContext _db;

        public NotificationHub(ILogger<NotificationHub> logger, ProsocDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Appelé quand un client se connecte
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Ajouter l'utilisateur à son groupe personnel
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Ajouter l'utilisateur au groupe général
                await Groups.AddToGroupAsync(Context.ConnectionId, "all_users");
                
                // Ajouter l'utilisateur au groupe des affiliés si applicable
                await AddUserToAffilieGroupAsync(userId);
                
                _logger.LogInformation($"User {userName} (ID: {userId}) connected to NotificationHub. ConnectionId: {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Appelé quand un client se déconnecte
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userName} (ID: {userId}) disconnected from NotificationHub. ConnectionId: {Context.ConnectionId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Permet à un utilisateur de rejoindre un groupe spécifique
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"User {Context.User?.Identity?.Name} joined group: {groupName}");
        }

        /// <summary>
        /// Permet à un utilisateur de quitter un groupe spécifique
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"User {Context.User?.Identity?.Name} left group: {groupName}");
        }

        /// <summary>
        /// Marquer une notification comme lue
        /// </summary>
        public async Task MarkNotificationAsRead(int notificationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Mettre à jour la notification en base de données
                    var notification = await _db.Notifications
                        .FirstOrDefaultAsync(n => n.IdNotification == notificationId && n.RecepteurId == int.Parse(userId));

                    if (notification != null)
                    {
                        notification.EstLu = true;
                        notification.DateLecture = DateTime.Now;
                        await _db.SaveChangesAsync();
                    }

                    _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);
                    
                    // Notifier les autres clients du même utilisateur
                    await Clients.OthersInGroup($"user_{userId}").SendAsync("NotificationMarkedAsRead", new { notificationId, userId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error marking notification {notificationId} as read for user {userId}");
                }
            }
        }

        /// <summary>
        /// Envoyer une notification à un utilisateur spécifique
        /// </summary>
        public async Task SendNotificationToUser(int userId, string title, string message, string type = "info")
        {
            try
            {
                var notification = new Notification
                {
                    Titre = title,
                    Message = message,
                    Type = type,
                    RecepteurId = userId,
                    DateCreation = DateTime.Now
                };

                _db.Notifications.Add(notification);
                await _db.SaveChangesAsync();

                // Envoyer la notification en temps réel via SignalR
                await Clients.Group($"user_{userId}").SendAsync("NewNotification", new
                {
                    id = notification.IdNotification,
                    title = notification.Titre,
                    message = notification.Message,
                    type = notification.Type,
                    dateCreation = notification.DateCreation,
                    estLu = false
                });

                _logger.LogInformation($"Notification sent to user {userId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to user {userId}");
            }
        }

        /// <summary>
        /// Envoyer une notification à tous les affiliés
        /// </summary>
        public async Task SendBroadcastToAffilies(string title, string message, string type = "info")
        {
            try
            {
                await Clients.Group("all_affilies").SendAsync("BroadcastNotification", new
                {
                    title = title,
                    message = message,
                    type = type,
                    timestamp = DateTime.Now
                });

                _logger.LogInformation($"Broadcast sent to all affilies: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending broadcast to affilies");
            }
        }

        /// <summary>
        /// Obtenir le statut de connexion
        /// </summary>
        public async Task GetConnectionStatus()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            await Clients.Caller.SendAsync("ConnectionStatus", new
            {
                IsConnected = true,
                UserId = userId,
                UserName = userName,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Ajouter l'utilisateur à son groupe d'affilié
        /// </summary>
        private async Task AddUserToAffilieGroupAsync(string userId)
        {
            try
            {
                var affilie = await _db.Affilies
                    .Include(a => a.Adhesions)
                    .ThenInclude(ad => ad.Utilisateur)
                    .FirstOrDefaultAsync(a => a.Adhesions.Any(ad => ad.Utilisateur != null && ad.Utilisateur.IdUtilisateur == int.Parse(userId)));

                if (affilie != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"affilie_{affilie.IdAffilie}");
                    await Groups.AddToGroupAsync(Context.ConnectionId, "all_affilies");
                    
                    _logger.LogInformation("User {UserId} added to affilie group {AffilieId}", userId, affilie.IdAffilie);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to affilie group", userId);
            }
        }
    }
}
