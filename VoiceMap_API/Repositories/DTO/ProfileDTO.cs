using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.DTO
{
    public class ProfileDTO
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public IFormFile? ProfilePictureUrl { get; set; }
        public IFormFile? CoverImageUrl { get; set; }
        public string? Bio { get; set; }
        public int MaritalStatus { get; set; }
        public int Gender { get; set; }
        public string? LivesIn { get; set; }
        public string? From { get; set; }
        public int ProfileTypeId { get; set; }
        public int ExpertiseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
    }
}
