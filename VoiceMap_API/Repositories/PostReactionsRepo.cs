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
        public PostReactionsRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
                existingReaction.reactedAt = DateTime.UtcNow;
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

            return finalReactions;
        }
    }
}
