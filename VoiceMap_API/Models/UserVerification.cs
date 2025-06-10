using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class UserVerification
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string RandomKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
