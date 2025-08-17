using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public class Users
    {
        [Key]
        public long Id { get; set; }

        [Required, MaxLength(256)]
        public string Email { get; set; }

        [Required, MaxLength(512)]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsVerified { get; set; } = false;

        public bool IsActivated { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        [MaxLength(45)]
        public string IpAddress { get; set; }
        public string SecretKey { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }

        [NotMapped]
        public List<UserSecuritySettings> UserSecuritySettings { get; set; }
    }
}
