using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class NotificationRepo : INotifications
    {
        private readonly AppDbContext.AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddNotificationAsync(Notifications notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<dynamic>> GetUserNotificationsAsync(int userId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;

            var notifications = await _context.Notifications
                .Where(n => n.recipient_user_id == userId)
                .OrderByDescending(n => n.created_at)
                .Take(50)
                .ToListAsync();

            var reactorUserIds = notifications.Select(n => n.reactor_user_id).Distinct().ToList();
            var postIds = notifications
                .Where(n => n.post_id.HasValue && n.post_id.Value != 0)
                .Select(n => n.post_id.Value)
                .Distinct()
                .ToList();

            var groupIds = notifications
                .Where(n => n.typId == 4 && n.groupId.HasValue && n.groupId.Value != 0)
                .Select(n => n.groupId.Value)
                .Distinct()
                .ToList();

            var users = await (from u in _context.UserProfiles
                               join usr in _context.Users
                               on u.UserId equals usr.Id
                               where reactorUserIds.Contains(Convert.ToInt32(u.UserId)) &&
                                     usr.IsActivated == true &&
                                     usr.IsDeleted == false
                               select u)
                          .ToDictionaryAsync(u => u.UserId);

            var posts = await _context.Posts
                .Where(p => postIds.Contains(Convert.ToInt32(p.Id)))
                .ToDictionaryAsync(p => p.Id);

            var groups = await _context.Groups
                .Where(g => groupIds.Contains(Convert.ToInt32(g.Id)))
                .ToDictionaryAsync(g => g.Id);

            var result = notifications.Select(n => new
            {
                NotificationId = n.Id,
                Message = n.message,
                IsRead = n.is_read,
                CreatedAt = n.created_at,
                PostId = n.post_id,
                PostUrl = (n.post_id.HasValue && n.post_id.Value != 0 && posts.ContainsKey(n.post_id.Value))
                  ? posts[n.post_id.Value].PostUrl
                  : null,
                typeId = n.typId,
                commentId = n.comment_id,
                groupId = n.groupId,
                groupUrl = (n.groupId.HasValue && n.groupId.Value != 0 && groups.ContainsKey(n.groupId.Value))
                  ? groups[n.groupId.Value].GroupUrl
                  : null,
                ReactorUser = n.typId == 4 && n.groupId.HasValue && groups.ContainsKey(n.groupId.Value)
                    ? new
                    {
                        Id = (int)groups[n.groupId.Value].Id,
                        Name = groups[n.groupId.Value].GroupName,
                        Avatar = !string.IsNullOrEmpty(groups[n.groupId.Value].GroupPic)
                            ? $"{scheme}://{host}/User/GroupProfilePhotos/{Path.GetFileName(groups[n.groupId.Value].GroupPic)}"
                            : null
                    }
                    : (users.ContainsKey(n.reactor_user_id) ? new
                    {
                        Id = n.reactor_user_id,
                        Name = users[n.reactor_user_id].FullName,
                        Avatar = users[n.reactor_user_id].ProfilePictureUrl != null
                            ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(users[n.reactor_user_id].ProfilePictureUrl)}"
                            : null
                    } : null)
            }).ToList<object>();

            return result;
        }

        public async Task<List<dynamic>> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return await GetUserNotificationsAsync(notification.recipient_user_id);
            }

            return new List<dynamic>();
        }

        public async Task<List<dynamic>> IsReadNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.is_read = true; 
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();

                return await GetUserNotificationsAsync(notification.recipient_user_id);
            }

            return new List<dynamic>();
        }
    }
}
