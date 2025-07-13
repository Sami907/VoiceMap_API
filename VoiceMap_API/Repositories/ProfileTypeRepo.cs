using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class ProfileTypeRepo : IProfileType
    {
        private readonly AppDbContext.AppDbContext _context;

        public ProfileTypeRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProfileType>> GetTypes()
        {
            return await _context.ProfileType.ToListAsync();
        }
    }
}
