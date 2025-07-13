using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Repositories.Interface;
using VoiceMap_API.Models;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Repositories.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace VoiceMap_API.Repositories
{
    public class SignUpRepo : ISignUp
    {
        private readonly AppDbContext.AppDbContext _context;

        public SignUpRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }

        public async Task<(long userId, string secretKey)> SignUpUser(UserDTO userDto)
        {
            string secretKey = Methods.GenerateSecretKey();
            var EncryptSecKet = await Methods.EncryptAsync(secretKey);
            var EncryptEmail = await Methods.EncryptAsync(userDto.Email);
            var user = new Users
            {
                Email = BitConverter.ToString(EncryptEmail),
                PasswordHash = Methods.HashPassword(userDto.PasswordHash),
                IpAddress = userDto.ipAddress,
                SecretKey = BitConverter.ToString(EncryptSecKet)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (user.Id, secretKey);
        }

        public async Task<List<string>> GetAllEncryptedEmailAsync()
        {
            return await _context.Users.Where(s => s.IsDeleted == false && s.IsActivated == true)
                .Select(s => s.Email)
                .ToListAsync();
        }
        public async Task<List<string>> LoadEncryptedEmailsAsync()
        {
            return await _context.Users.Where(s => s.IsDeleted == false)
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
        public async Task<bool> CheckIsEmailExists(string email)
        {
            var existingEmails = await GetAllEncryptedEmailAsync();
            if (existingEmails.Count > 0)
            {
                foreach (var encrypted in existingEmails)
                {
                    var decryptedEmail = await Methods.DecryptAsync(Methods.HexStringToByteArray(encrypted));
                    if (decryptedEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<Users> GetUserByEmail(string email)
        {
            var user = await _context.Users
         .FirstOrDefaultAsync(u => u.Email == email && u.IsDeleted == false);

            if (user == null)
                return null;

            var securitySettings = await (from setting in _context.UserSecuritySettings
                                          where setting.UserId == user.Id
                                          select setting).ToListAsync();


            user.UserSecuritySettings = securitySettings;

            return user;
        }

        public async Task UpdateEmail(int userId, string email)
        {
            var user = await GetUserById(userId);
            if (user != null)
            {
                var EncryptEmail = await Methods.EncryptAsync(email);
                user.Email = BitConverter.ToString(EncryptEmail);
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeActivateAccount(Users usr)
        {
            if (usr != null)
            {
                usr.IsActivated = !usr.IsActivated;
                await _context.SaveChangesAsync();
            }
        }

        private async Task DeleteLoginHistory(long userId)
        {
            var loginHistory = await _context.UserLoginLogs.Where(w => w.UserId == userId).ToListAsync();

            if (loginHistory.Any())
            {
                _context.UserLoginLogs.RemoveRange(loginHistory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAccount(Users usr)
        {
            if (usr != null)
            {
                await DeleteLoginHistory(usr.Id);
                usr.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePassword(Users usr,string hashPass)
        {
            if (usr != null)
            {
                await DeleteLoginHistory(usr.Id);
                usr.PasswordHash = hashPass;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<(long userId, bool isValidKey)> GetUserBySecretKey(string key)
        {
            var result = await _context.Users.Where(u => u.IsActivated == true && u.IsDeleted == false).Select(s => 
            new 
            {
                s.SecretKey,
                s.Id
            }
            ).ToListAsync();
            if (result.Count > 0)
            {
                foreach (var encrypted in result)
                {
                    var decryptedkey = await Methods.DecryptAsync(Methods.HexStringToByteArray(encrypted.SecretKey));
                    if (decryptedkey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return (encrypted.Id, true);
                    }
                }

                return (0, false);
            }
            return (0, false);
        }
    }
}
