using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class PostDTO
    {
        public int userId { get; set; }
        public string Content { get; set; }
        public IFormFile PostImageUrl { get; set; }
        public IFormFile VoiceUrl { get; set; }
        public int categoryId { get; set; }
        public string tagLocation { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
