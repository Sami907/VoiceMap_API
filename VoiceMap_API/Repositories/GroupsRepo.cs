using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
    }
}
