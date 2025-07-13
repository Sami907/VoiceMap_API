using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class UserSecuritySettings
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public bool SendEmailIfLoggedIn { get; set; }
        public bool TwoFactorAuth { get; set; }
    }
}
