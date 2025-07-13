using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class UserProfilesRepo : IUserProfiles
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;

        public UserProfilesRepo(AppDbContext.AppDbContext context,  IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<dynamic> SaveProfileAsync(UserProfiles up)
        {
            _context.UserProfiles.Add(up);
            await _context.SaveChangesAsync();
            return _context.UserProfiles.Where(w=> w.UserId == up.UserId).FirstOrDefaultAsync();
        }

        public async Task<dynamic> GetUserProfileById(long userId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var result = await _context.UserProfiles.Where(s => s.UserId == userId).FirstOrDefaultAsync();
            
            result.ProfilePictureUrl = result.ProfilePictureUrl != null ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(result.ProfilePictureUrl)}" : null;

            result.CoverImageUrl = result.CoverImageUrl != null
                            ? $"{scheme}://{host}/User/CoverPictures/{Path.GetFileName(result.CoverImageUrl)}"
                            : null;

            return result;
        }
    }
}
