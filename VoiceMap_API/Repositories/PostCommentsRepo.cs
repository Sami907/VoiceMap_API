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
    public class PostCommentsRepo : IPostComments
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;
        public PostCommentsRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<dynamic>> SaveNGetComments(long postId, long userId, string commentText)
        {
            var newComment = new PostComments
            {
                PostId = postId,
                UserId = userId,
                comment = commentText,
                createdAt = DateTime.UtcNow
            };

            _context.PostComments.Add(newComment);
            await _context.SaveChangesAsync();

            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;

            var comments = await _context.PostComments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.createdAt)
                .Select(c => new
                {
                    Author = _context.UserProfiles
                        .Where(up => up.UserId == c.UserId)
                        .Select(up => up.FullName)
                        .FirstOrDefault() ?? "Unknown",
                    Text = c.comment,
                    Avatar = _context.UserProfiles
                        .Where(up => up.UserId == c.UserId && up.ProfilePictureUrl != null)
                        .Select(up => $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(up.ProfilePictureUrl)}")
                        .FirstOrDefault()
                })
                .ToListAsync();

            return comments;
        }
    }
}
