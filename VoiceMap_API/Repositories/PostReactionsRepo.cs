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
    public class PostReactionsRepo : IPostReactions
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;
        private readonly INotifications _Inotification;
        public PostReactionsRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor, INotifications notification)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _Inotification = notification;
        }

        public async Task<IEnumerable<dynamic>> SaveNGetReaction(long postId, long userId, int reactionId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;

            var existingReaction = await _context.PostReactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

            if (existingReaction != null)
            {
                existingReaction.ReactionTypeId = reactionId;
                existingReaction.reactedAt = DateTime.Now;
                _context.PostReactions.Update(existingReaction);
            }
            else
            {
                var newReaction = new PostReactions
                {
                    PostId = postId,
                    UserId = userId,
                    ReactionTypeId = reactionId,
                    reactedAt = DateTime.UtcNow
                };
                await _context.PostReactions.AddAsync(newReaction);
            }

            await _context.SaveChangesAsync();

            var reactions = await _context.PostReactions
                .Where(r => r.PostId == postId)
                .OrderByDescending(r => r.reactedAt)
                .Select(r => new
                {
                    r.PostId,
                    r.UserId,
                    r.ReactionTypeId,
                    r.reactedAt
                }).ToListAsync();

            var userIds = reactions.Select(r => r.UserId).Distinct().ToList();

            var users = await _context.UserProfiles
                .Where(up => userIds.Contains(up.UserId))
                .ToDictionaryAsync(up => up.UserId);

            var reactionTypeIds = reactions.Select(r => r.ReactionTypeId).Distinct().ToList();

            var reactionTypes = await _context.ReactionTypes
                .Where(rt => reactionTypeIds.Contains(rt.Id))
                .ToDictionaryAsync(rt => rt.Id);

            var finalReactions = reactions.Select(r => new
                {
                    UserId = r.UserId,
                    User = users.ContainsKey(r.UserId) ? users[r.UserId].FullName : "Unknown",
                    Avatar = users.ContainsKey(r.UserId) && users[r.UserId].ProfilePictureUrl != null
                    ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(users[r.UserId].ProfilePictureUrl)}"
                    : null,
                    type = reactionTypes.ContainsKey(r.ReactionTypeId) ? reactionTypes[r.ReactionTypeId].name : "Unknown"
                }).ToList();

                var postOwnerId = await _context.Posts
                .Where(p => p.Id == postId)
                .Select(p => p.userId)
                .FirstOrDefaultAsync();

                if (postOwnerId != 0 && postOwnerId != userId) 
                {
                    var reactorNames = users
                        .Where(u => u.Key != postOwnerId)
                        .Select(u => u.Value.FullName)
                        .Distinct()
                        .ToList();

                    string message;

                    if (reactorNames.Count == 1)
                        message = $"{reactorNames[0]} reacted on your post";
                    else if (reactorNames.Count == 2)
                        message = $"{reactorNames[0]}, {reactorNames[1]} reacted on your post";
                    else if (reactorNames.Count >= 3)
                        message = $"{reactorNames.Count} persons reacted on your post";
                    else
                        message = "Someone reacted on your post";

                var existingNotif = await _context.Notifications.FirstOrDefaultAsync(n =>
                    n.recipient_user_id == postOwnerId &&
                    n.post_id == postId &&
                    n.typId == 1
                );

                if (existingNotif != null)
                {
                    existingNotif.message = message;
                    existingNotif.is_read = false;
                    existingNotif.created_at = DateTime.UtcNow;
                    _context.Notifications.Update(existingNotif);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var notification = new Notifications
                    {
                        recipient_user_id = postOwnerId,
                        reactor_user_id = Convert.ToInt32(userId),
                        post_id = Convert.ToInt32(postId),
                        comment_id = null,
                        typId = 1,
                        message = message,
                        is_read = false,
                        created_at = DateTime.UtcNow
                    };

                    await _Inotification.AddNotificationAsync(notification);
                }
            }
            return finalReactions;
        }
    }
}
