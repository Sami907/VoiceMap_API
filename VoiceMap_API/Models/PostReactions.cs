using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class PostReactions
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public long UserId { get; set; }
        public int ReactionTypeId { get; set; }
        public DateTime reactedAt { get; set; }
    }
}
