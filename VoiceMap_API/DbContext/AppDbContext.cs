using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;

namespace VoiceMap_API.AppDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserVerification> UserVerification { get; set; }
    }
}
