using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class CreateProfileDTO
    {
        public ProfileDTO? Profile { get; set; }
        public UserSecuritySettingDTO? UserSecuritySetting { get; set; }
    }
}
