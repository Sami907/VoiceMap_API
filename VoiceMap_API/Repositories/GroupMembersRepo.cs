using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class GroupMembersRepo : IGroupMembers
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotifications _notifications;

        public GroupMembersRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor, IHubContext<NotificationHub> hubContext,
            INotifications notifications)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
            _notifications = notifications;
        }
        public async Task<int> ToggleGroupMembership(int userId, int groupId)
        {
            var existingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

            if (existingMembership != null)
            {
                // User left the group
                _context.GroupMembers.Remove(existingMembership);
                await _context.SaveChangesAsync();
                return 2; 
            }
            else
            {
                // User Join the group
                var newMembership = new GroupMembers
                {
                    UserId = userId,
                    GroupId = groupId,
                    JoinedAt = DateTime.Now
                };

                _context.GroupMembers.Add(newMembership);
                await _context.SaveChangesAsync();

                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group != null && group.UserId != userId)  
                {
                    var user = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

                    string message = $"{user?.FullName ?? "A user"} has joined your group '{group.GroupName}'.";

                    await AppClasses.Methods.SendGroupNotificationAsync(
                        actorUserId: userId,
                        recipientUserId: group.UserId,
                        typeId: 4,
                        message: message,
                        _context: _context,
                        _notificationService: _notifications,
                        _hubContext,
                        groupId: groupId
                    );
                }

                return 1; 
            }
        }
    }
}
