using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class UserSecuritySettingDTO
    {
        public long UserId { get; set; }
        public bool SendEmailIfLoggedIn { get; set; }
        public bool TwoFactorAuth { get; set; }
    }
}
