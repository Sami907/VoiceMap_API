using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IUserVerification
    {
        Task<string> UserVerification(int userID);
        Task<int> OtpVerification(string otp, int UserId);
        Task<string> ResendOtp(int UserId); 
        Task DeleteOtpRecord(int UserId, string otp);
    }
}
