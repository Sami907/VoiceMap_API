using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class UpdatePhotoDTO
    {
        public int userId { get; set; }
        public IFormFile profilePhoto { get; set; }
        public IFormFile coverPhoto { get; set; }
        public string update { get; set; }
    }
}
