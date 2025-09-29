using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class UpdateGroupDTO
    {
        public int groupId { get; set; }
        public int UserId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int PrivacySetting { get; set; }
        public int GroupCategory { get; set; }
    }
}
