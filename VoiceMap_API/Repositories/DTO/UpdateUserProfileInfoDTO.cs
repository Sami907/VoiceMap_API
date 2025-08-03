using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class UpdateUserProfileInfoDTO
    {
        public long UserId { get; set; }
        public string Bio { get; set; }
        public int ProfileTypeId { get; set; }
        public int ExpertiseId { get; set; }
        public int Gender { get; set; }
        public string country { get; set; }
        public string city { get; set; }
    }
}
