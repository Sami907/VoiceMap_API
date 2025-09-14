using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class Groups
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        [MaxLength]
        public string GroupName { get; set; }
        [MaxLength]
        public string Description { get; set; }
        public int PrivacySetting { get; set; }
        public int GroupCategory { get; set; }
        [MaxLength]
        public string CoverPhoto { get; set; }
        [MaxLength]
        public string GroupPic { get; set; }
        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string GroupUrl { get; set; }
    }
}
