using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class UserSecuritySettingsRepo : IUserSecuritySettings
    {
        private readonly AppDbContext.AppDbContext _context;

        public UserSecuritySettingsRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveSecuritySettings(UserSecuritySettings uss)
        {
            try
            {
                _context.UserSecuritySettings.Add(uss);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while save settings.", ex);
            }
        }
    }
}
