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
    public class GroupsRepo : IGroups
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;

        public GroupsRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
    }
}
