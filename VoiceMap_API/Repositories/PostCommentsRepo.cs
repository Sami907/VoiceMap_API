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

        private async Task<List<dynamic>> GetCommentsByPostId(long postId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host.Value;

            var comments = await _context.PostComments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.createdAt)
                .Select(c => new
                {
                    Id = c.Id,
                    AuthorId = c.UserId,
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

            return comments.Cast<dynamic>().ToList();
        }

        public async Task<IEnumerable<dynamic>> SaveNGetComments(long postId, long userId, string commentText)
        {
            var newComment = new PostComments
            {
                PostId = postId,
                UserId = userId,
                comment = commentText,
                createdAt = DateTime.Now
            };

            _context.PostComments.Add(newComment);
            await _context.SaveChangesAsync();

            return await GetCommentsByPostId(postId);
        }
        public async Task<IEnumerable<dynamic>> UpdateNGetComments(long postId, long commentId, string comment)
        {
            var existingComment = await _context.PostComments.FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

            if (existingComment == null)
            {
                throw new Exception("Comment not found.");
            }

            existingComment.comment = comment;
            existingComment.createdAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await GetCommentsByPostId(postId);
        }

        public async Task<IEnumerable<dynamic>> DeleteNGetComments(long postId, long commentId)
        {
            var existingComment = await _context.PostComments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);

            if (existingComment == null)
            {
                throw new Exception("Comment not found.");
            }

            _context.PostComments.Remove(existingComment);
            await _context.SaveChangesAsync();

            return await GetCommentsByPostId(postId);
        }
    }
}
