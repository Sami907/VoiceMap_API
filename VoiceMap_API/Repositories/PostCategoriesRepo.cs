using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;

namespace VoiceMap_API.Repositories
{
    public class PostCategoriesRepo : IPostCategories
    {
        private readonly AppDbContext.AppDbContext _context;

        public PostCategoriesRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostCategories>> GetCategories()
        {
            return await _context.PostCategories.ToListAsync();
        }
    }
}
