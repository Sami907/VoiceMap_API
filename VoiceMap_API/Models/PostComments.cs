using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class PostComments
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public long UserId { get; set; }
        public string comment { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
    }
}
