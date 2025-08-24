using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.AppClasses
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task SendNotification(string userId, object notification)
        {
            try
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                throw;
            }
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("User connected: {UserId}", Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User disconnected: {UserId}, Reason: {Reason}", Context.UserIdentifier, exception?.Message);
            return base.OnDisconnectedAsync(exception);
        }
    }

}
