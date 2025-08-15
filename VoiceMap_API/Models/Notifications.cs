using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class Notifications
    {
        public long Id { get; set; }
        public int recipient_user_id { get; set; }
        public int reactor_user_id { get; set; }
        public int? post_id { get; set; }
        public int? comment_id { get; set; }
        public int? typId { get; set; }
        public string message { get; set; }
        public bool is_read { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        
    }
}
