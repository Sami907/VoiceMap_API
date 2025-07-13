using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class UserProfiles
    {
        public Int64 Id { get; set; }
        public long UserId { get; set; }              
        public string FullName { get; set; }          
        public string? ProfilePictureUrl { get; set; }
        public string? CoverImageUrl { get; set; }    
        public string? Bio { get; set; }               
        public int MaritalStatus { get; set; }         
        public int Gender { get; set; }                
        public string? LivesIn { get; set; }                
        public string? From { get; set; }                   
        public int ProfileTypeId { get; set; }          
        public int ExpertiseId { get; set; }            
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
