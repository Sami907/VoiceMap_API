using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;

namespace VoiceMap_API.Models
{
    public class UserProfile
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        [MaxLength(256)]
        public string FullName { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string CoverImageUrl { get; set; }

        public string Bio { get; set; }

        public int? MaritalStatus { get; set; }

        public int? Gender { get; set; }

        public int? LivesIn { get; set; }

        public int? From { get; set; }

        public int? ProfileTypeId { get; set; }

        public int? ExpertiseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual Users User { get; set; }
    }
}
