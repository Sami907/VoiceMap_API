using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class UserLoginLogs
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public DateTime LoginTime { get; set; }

        public string IpAddress { get; set; }

        public string DeviceInfo { get; set; }

        public bool IsSuccessful { get; set; }
    }
}
