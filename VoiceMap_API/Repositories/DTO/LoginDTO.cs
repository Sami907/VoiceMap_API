using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ipAddress { get; set; }
        public string deviceInfo { get; set; }
    }
}
