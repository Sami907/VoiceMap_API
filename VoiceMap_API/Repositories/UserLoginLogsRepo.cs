using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class UserLoginLogsRepo : IUserLoginLogs
    {
        private readonly AppDbContext.AppDbContext _context;

        public UserLoginLogsRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<long> SaveLoginLogs(long UserId, bool IsSuccessful,string IpAddress, string deviceInfo)
        {
            var logs = new UserLoginLogs
            {
                UserId = UserId,
                IsSuccessful = IsSuccessful,
                IpAddress = IpAddress,
                LoginTime = DateTime.Now,
                DeviceInfo = deviceInfo
            };

            _context.UserLoginLogs.Add(logs);
            await _context.SaveChangesAsync();
            return logs.Id;
        }
    }
}
