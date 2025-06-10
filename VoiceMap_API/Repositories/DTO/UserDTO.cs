using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class UserDTO
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
