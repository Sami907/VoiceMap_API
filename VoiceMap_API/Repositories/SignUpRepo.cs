using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Repositories.Interface;
using VoiceMap_API.Models;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Repositories.DTO;
using Microsoft.EntityFrameworkCore;

namespace VoiceMap_API.Repositories
{
    public class SignUpRepo : ISignUp
    {
        private readonly AppDbContext.AppDbContext _context;

        public SignUpRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<long> SignUpUser(UserDTO userDto)
        {
            var EncryptEmail = await Methods.EncryptAsync(userDto.Email);
            var user = new Users
            {
                Email = BitConverter.ToString(EncryptEmail),
                PasswordHash = Methods.HashPassword(userDto.PasswordHash)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

       
        public async Task<List<string>> GetAllEncryptedEmailAsync()
        {
            return await _context.Users.Where(s => s.IsDeleted == false && s.IsActivated == false)
                .Select(s => s.Email)
                .ToListAsync();
        }
        public async Task UpdateIsVerified(int UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);

            if (user != null)
            {
                user.IsVerified = true;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Users> GetUserById(int UserId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        }
    }
}
