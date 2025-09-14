using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class GroupDTO
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int PrivacySetting { get; set; }
        public int GroupCategory { get; set; }
        public IFormFile CoverPhoto { get; set; }
        public IFormFile GroupPic { get; set; }
    }
}
