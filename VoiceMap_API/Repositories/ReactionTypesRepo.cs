using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class ReactionTypesRepo : IReactionTypes
    {
        private readonly AppDbContext.AppDbContext _context;

        public ReactionTypesRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReactionTypes>> GetReactions()
        {
            return await _context.ReactionTypes.ToListAsync();
        }
    }
}
