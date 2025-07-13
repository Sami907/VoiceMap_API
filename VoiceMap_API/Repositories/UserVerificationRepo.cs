using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class UserVerificationRepo : IUserVerification
    {
        private readonly AppDbContext.AppDbContext _context;

        public UserVerificationRepo(AppDbContext.AppDbContext context)
        {
            _context = context;
        }
        public async Task<string> UserVerification(int userId)
        {
            try
            {
                string otpCode = Methods.GenerateOtp(userId);
                var userVer = new UserVerification
                {
                    UserId = userId,
                    RandomKey = otpCode,
                    CreatedAt = DateTime.Now
                };

                _context.UserVerification.Add(userVer);
                await _context.SaveChangesAsync();
                return otpCode;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while signing up the user.", ex);
            }
        }

        public async Task<int> OtpVerification(string otp, int UserId)
        {
            try
            {
                var user = await _context.UserVerification.FirstOrDefaultAsync(u => u.RandomKey == otp && u.UserId == UserId);

                // OTP not found → invalid
                if (user == null)
                {
                    return 2;
                }

                // OTP expired → older than 1 minute
                if ((DateTime.UtcNow - user.CreatedAt).TotalSeconds > 60)
                {
                    return 1;
                }

                // Valid and not expired
                return 3;

            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while signing up the user.", ex);
            }
        }

        public async Task<string> ResendOtp(int UserId)
        {
            var userVer = await _context.UserVerification.FirstOrDefaultAsync(u => u.Id == UserId);

            if (userVer != null)
            {
                string otp = Methods.GenerateOtp(UserId);
                userVer.RandomKey = otp;
                userVer.CreatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return otp;
            }

            return "";
        }

        public async Task DeleteOtpRecord(int UserId, string otp)
        {
            var userVer = await _context.UserVerification.FirstOrDefaultAsync(u => u.UserId == UserId && u.RandomKey == otp);

            if (userVer != null)
            {
                _context.Remove(userVer);
                await _context.SaveChangesAsync();
            }
        }
        
    }
}
