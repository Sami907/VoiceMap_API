using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class ExpertiseTypeRepo : IExpertiseType
    {
        private readonly AppDbContext.AppDbContext _context;

        public ExpertiseTypeRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExpertiseType>> GetTypes()
        {
            return await _context.ExpertiseType.ToListAsync();
        }
    }
}
