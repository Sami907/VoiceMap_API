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
        public DbSet<UserProfiles> UserProfiles { get; set; }
        public DbSet<UserVerification> UserVerification { get; set; }
        public DbSet<UserSecuritySettings> UserSecuritySettings { get; set; }
        public DbSet<UserLoginLogs> UserLoginLogs { get; set; }
        public DbSet<ExpertiseType> ExpertiseType { get; set; }
        public DbSet<ProfileType> ProfileType { get; set; }
        public DbSet<ReactionTypes> ReactionTypes { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<PostCategories> PostCategories { get; set; }
        public DbSet<Friendships> Friendships { get; set; }
        public DbSet<PostComments> PostComments { get; set; }
        public DbSet<PostReactions> PostReactions { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Groups> Groups { get; set; }
        public DbSet<GroupMembers> GroupMembers { get; set; }
    }
}
