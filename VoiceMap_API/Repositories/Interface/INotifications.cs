using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using static VoiceMap_API.Models.Notifications;

namespace VoiceMap_API.Repositories.Interface
{
    public interface INotifications
    {
        Task AddNotificationAsync(Notifications notification);
        Task<List<dynamic>> GetUserNotificationsAsync(int userId);
        Task<List<dynamic>> DeleteNotificationAsync(int notificationId);
        Task<List<dynamic>> IsReadNotificationAsync(int notificationId);
    }
}
