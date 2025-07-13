using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IUserLoginLogs
    {
        Task<long> SaveLoginLogs(long UserId, bool IsSuccessful, string IpAddress, string deviceInfo);
    }
}
