using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class Posts
    {
        public long Id { get; set; }
        public int userId { get; set; }
        public string Content { get; set; }
        public string PostImageUrl { get; set; }
        public string VoiceUrl { get; set; }
        public int categoryId { get; set; }
        public string tagLocation { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public DateTime PostTime { get; set; } = DateTime.Now;
        public string PostUrl { get; set; }
        public int groupid { get; set; } = 0;
    }
}
