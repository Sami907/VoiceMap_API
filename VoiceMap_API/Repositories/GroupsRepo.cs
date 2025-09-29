using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class GroupsRepo : IGroups
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;
        private readonly IPosts _pr;
        private readonly IGroupMembers _iGrpMem;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotifications _notifications;

        public GroupsRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor, IPosts pr, IGroupMembers iGrpMem, IHubContext<NotificationHub> hubContext,
            INotifications notifications)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _pr = pr;
            _iGrpMem = iGrpMem;
            _hubContext = hubContext;
            _notifications = notifications;
        }

        public async Task CreateGroup(Groups grp)
        {
            _context.Groups.Add(grp);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<dynamic>> GetCurrentUserGroup(int userId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;

            var groups = await _context.Groups
                .Where(g => g.UserId == userId)
                .Select(g => new
                {
                    Group = g, 
                    GroupProfilePhotoUrl = !string.IsNullOrEmpty(g.GroupPic)
                        ? $"{scheme}://{host}/User/GroupProfilePhotos/{Path.GetFileName(g.GroupPic)}"
                        : null,
                    GroupCoverPhotoUrl = !string.IsNullOrEmpty(g.CoverPhoto)
                        ? $"{scheme}://{host}/User/GroupCoverPhotos/{Path.GetFileName(g.CoverPhoto)}"
                        : null
                })
                .ToListAsync();

            return groups;
        }

        public async Task<dynamic> SearchGroups(string query, int userId, int skip = 0, int take = 20)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var groups = await _context.Groups
                .Where(u => u.GroupName.Contains(query))
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(take)
                .Select(g => new
                {
                    Group = g,
                    GroupProfilePhotoUrl = !string.IsNullOrEmpty(g.GroupPic)
                        ? $"{scheme}://{host}/User/GroupProfilePhotos/{Path.GetFileName(g.GroupPic)}"
                        : null,
                    GroupCoverPhotoUrl = !string.IsNullOrEmpty(g.CoverPhoto)
                        ? $"{scheme}://{host}/User/GroupCoverPhotos/{Path.GetFileName(g.CoverPhoto)}"
                        : null
                })
                .ToListAsync();

            return groups;
        }

        public async Task<dynamic> GetGroupByUrl(string grpUrl, int userId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var fullGroupUrl = $"{Methods.GetOrigin()}/main/groups/{grpUrl}";

            var groupWithCategory = await (from g in _context.Groups
                                           join gc in _context.PostCategories on g.GroupCategory equals gc.Id
                                           where g.GroupUrl == fullGroupUrl
                                           select new
                                           {
                                               g.Id,
                                               g.UserId,
                                               g.GroupName,
                                               g.Description,
                                               g.PrivacySetting,
                                               GroupCategoryId = gc.Id,
                                               GroupCategoryName = gc.Name,
                                               g.CreatedAt,
                                               g.GroupUrl,
                                               g.GroupPic,
                                               g.CoverPhoto
                                           }).FirstOrDefaultAsync();

            if (groupWithCategory != null)
            {
                var postsQuery = _context.Posts
                    .Where(p => p.groupid == groupWithCategory.Id)
                    .OrderByDescending(p => p.PostTime);

                bool hasJoined = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupId == groupWithCategory.Id && gm.UserId == userId);

                int totalMembers = await _context.GroupMembers
                    .CountAsync(gm => gm.GroupId == groupWithCategory.Id);

                var groupWithPhotos = new
                {
                    groupWithCategory.Id,
                    groupWithCategory.UserId,
                    groupWithCategory.GroupName,
                    groupWithCategory.Description,
                    groupWithCategory.PrivacySetting,
                    groupWithCategory.GroupCategoryId,
                    groupWithCategory.GroupCategoryName,  // category name here
                    groupWithCategory.CreatedAt,
                    groupWithCategory.GroupUrl,
                    GroupProfilePhotoUrl = !string.IsNullOrEmpty(groupWithCategory.GroupPic)
                        ? $"{scheme}://{host}/User/GroupProfilePhotos/{Path.GetFileName(groupWithCategory.GroupPic)}"
                        : null,
                    GroupCoverPhotoUrl = !string.IsNullOrEmpty(groupWithCategory.CoverPhoto)
                        ? $"{scheme}://{host}/User/GroupCoverPhotos/{Path.GetFileName(groupWithCategory.CoverPhoto)}"
                        : null,
                    hasJoined,
                    totalMembers,
                    posts = postsQuery != null ? await _pr.GetPostsByQuery(postsQuery, userId) : null
                };

                return groupWithPhotos;
            }

            return null;
        }

        public async Task<bool> UpdateGroupInfo(UpdateGroupDTO dto)
        {
            var existUser = await _context.Users
                .Where(w => w.Id == dto.UserId && w.IsActivated && !w.IsDeleted)
                .FirstOrDefaultAsync();

            if (existUser == null)
                return false;

            var group = await _context.Groups
                .Where(w => w.UserId == dto.UserId && w.Id == dto.groupId)
                .FirstOrDefaultAsync();

            if (group == null)
                return false;

            bool groupNameChanged = group.GroupName != dto.GroupName;
            var oldGroupName = group.GroupName;

            group.GroupName = dto.GroupName;
            group.PrivacySetting = dto.PrivacySetting;
            group.GroupCategory = dto.GroupCategory;
            group.Description = dto.Description;

            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            if (groupNameChanged)
            {
                var memberUserIds = await _context.GroupMembers
                    .Where(gm => gm.GroupId == group.Id)
                    .Select(gm => gm.UserId)
                    .ToListAsync();

                string message = $"You had joined this group when it was named '{oldGroupName}', and now its name has been changed to '{dto.GroupName}'.";

                foreach (var memberUserId in memberUserIds)
                {
                    await AppClasses.Methods.SendGroupNotificationAsync(
                        actorUserId: dto.UserId,
                        recipientUserId: memberUserId, 
                        typeId: 4,
                        message: message,
                        _context: _context,
                        _notificationService: _notifications,
                        _hubContext: _hubContext,
                        groupId: dto.groupId
                    );
                }
            }

            return true;
        }
    }
}
