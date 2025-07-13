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
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime PostTime { get; set; } = DateTime.Now;
    }
}
