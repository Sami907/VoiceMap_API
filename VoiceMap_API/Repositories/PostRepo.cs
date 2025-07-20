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
    public class PostRepo : IPosts
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;
        public PostRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SavePost(Posts posts)
        {
            _context.Posts.Add(posts);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<dynamic>> GetFeed(int userId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;

            var friendIds = await _context.Friendships
               .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == "accepted")
               .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
               .ToListAsync();

            var posts = await _context.Posts
                .OrderByDescending(p => p.PostTime)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    PostImageUrl = p.PostImageUrl,
                    VoiceUrl = p.VoiceUrl,
                    p.PostTime,
                    p.tagLocation,
                    p.categoryId,
                    p.userId
                })
                .ToListAsync();

            var userIds = posts.Select(p => p.userId).Distinct().ToList();

            var userProfiles = await _context.UserProfiles
                .Where(up => userIds.Contains(Convert.ToInt32(up.UserId)))
                .Select(up => new
                {
                    up.UserId,
                    up.FullName,
                    up.ProfilePictureUrl,
                    up.ProfileTypeId
                })
                .ToListAsync();

            var userProfileDict = userProfiles.ToDictionary(up => up.UserId);

            var profileTypeIds = userProfiles.Select(up => up.ProfileTypeId).Distinct().ToList();
            var profileTypes = await _context.ProfileType
                .Where(pt => profileTypeIds.Contains(pt.id))
                .ToDictionaryAsync(pt => pt.id);

            var postIds = posts.Select(p => p.Id).ToList();

            var allComments = await _context.PostComments
                .Where(c => postIds.Contains(c.PostId))
                .Select(c => new
                {
                    c.Id,
                    c.PostId,
                    AuthorId = c.UserId,
                    CommentText = c.comment,
                    CreatedAt = c.createdAt
                }).OrderByDescending(c => c.CreatedAt).ToListAsync();

            var commentAuthorIds = allComments.Select(c => c.AuthorId).Distinct().ToList();
            var commentAuthors = await _context.UserProfiles
                .Where(up => commentAuthorIds.Contains(Convert.ToInt32(up.UserId)))
                .ToDictionaryAsync(up => up.UserId);

            var allReactions = await _context.PostReactions
                .Where(r => postIds.Contains(r.PostId))
                .Select(r => new
                {
                    r.PostId,
                    r.UserId,
                    r.ReactionTypeId,
                    reactedAt = r.reactedAt
                }).OrderByDescending(r => r.reactedAt).ToListAsync();

            var reactionUserIds = allReactions.Select(r => r.UserId).Distinct().ToList();
            var reactionUsers = await _context.UserProfiles
                .Where(up => reactionUserIds.Contains(Convert.ToInt32(up.UserId)))
                .ToDictionaryAsync(up => up.UserId);

            var reactionTypeIds = allReactions.Select(r => r.ReactionTypeId).Distinct().ToList();
            var reactionTypes = await _context.ReactionTypes
                .Where(rt => reactionTypeIds.Contains(rt.Id))
                .ToDictionaryAsync(rt => rt.Id);

            var categoryIds = posts.Select(p => p.categoryId).Distinct().ToList();

            var postCategories = await _context.PostCategories
                .Where(pc => categoryIds.Contains(pc.Id))
                .ToDictionaryAsync(pc => pc.Id);


            var finalPosts = posts.Select(p => new
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.PostImageUrl != null ? $"{scheme}://{host}/User/PostImages/{Path.GetFileName(p.PostImageUrl)}" : null,
                VoiceUrl = p.VoiceUrl != null ? $"{scheme}://{host}/User/Voices/{Path.GetFileName(p.VoiceUrl)}" : null,
                Time = p.PostTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff") + "Z" ,
                LocationTag = p.tagLocation,
                Category = postCategories.ContainsKey(p.categoryId)
                ? postCategories[p.categoryId].Name
                : "Unknown",
    
                CategoryIcon = postCategories.ContainsKey(p.categoryId)
                ? postCategories[p.categoryId].IconUrl
                : null,

                Author = userProfileDict.ContainsKey(p.userId) ? userProfileDict[p.userId].FullName : "Unknown",
                Avatar = userProfileDict.ContainsKey(p.userId) && userProfileDict[p.userId].ProfilePictureUrl != null
                    ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(userProfileDict[p.userId].ProfilePictureUrl)}"
                    : null,

                ProfileType = userProfileDict.ContainsKey(p.userId)
                    && profileTypes.ContainsKey(userProfileDict[p.userId].ProfileTypeId)
                    ? profileTypes[userProfileDict[p.userId].ProfileTypeId].Name
                    : null,

                Comments = allComments
                    .Where(c => c.PostId == p.Id).OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        Id = c.Id,
                        Author = commentAuthors.ContainsKey(c.AuthorId) ? commentAuthors[c.AuthorId].FullName : "Unknown",
                        Text = c.CommentText,
                        Avatar = commentAuthors.ContainsKey(c.AuthorId) && commentAuthors[c.AuthorId].ProfilePictureUrl != null
                        ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(commentAuthors[c.AuthorId].ProfilePictureUrl)}"
                        : null,
                        AuthorId = c.AuthorId
                    }).ToList(),

                Reactions = allReactions
                .Where(r => r.PostId == p.Id)
                .OrderByDescending(r => r.reactedAt)
                .Select(r => new
                {
                    UserId = r.UserId,
                    User = reactionUsers.ContainsKey(r.UserId) ? reactionUsers[r.UserId].FullName : "Unknown",
                    Avatar = reactionUsers.ContainsKey(r.UserId) && reactionUsers[r.UserId].ProfilePictureUrl != null
                        ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(reactionUsers[r.UserId].ProfilePictureUrl)}"
                        : null,
                    type = reactionTypes[r.ReactionTypeId].name,
                }).ToList(),

                Likes = allReactions.Count(r => r.PostId == p.Id),
                Liked = allReactions.Any(r => r.PostId == p.Id && r.UserId == userId),
                NewComment = "",
                ShowComments = false,
                UserId = p.userId
            }).ToList();

            return finalPosts;
        }

        public async Task<bool> DeletePostWithDependencies(long postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return false;

            var relatedComments = _context.PostComments.Where(c => c.PostId == postId);
            _context.PostComments.RemoveRange(relatedComments);

            var relatedReactions = _context.PostReactions.Where(r => r.PostId == postId);
            _context.PostReactions.RemoveRange(relatedReactions);

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
