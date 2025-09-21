using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class GroupMembersRepo : IGroupMembers
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;

        public GroupMembersRepo(AppDbContext.AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<int> ToggleGroupMembership(int userId, int groupId)
        {
            var existingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

            if (existingMembership != null)
            {
                _context.GroupMembers.Remove(existingMembership);
                await _context.SaveChangesAsync();
                return 2; // Left
            }
            else
            {
                var newMembership = new GroupMembers
                {
                    UserId = userId,
                    GroupId = groupId,
                    JoinedAt = DateTime.Now
                };

                _context.GroupMembers.Add(newMembership);
                await _context.SaveChangesAsync();
                return 1; 
            }
        }

    }
}
