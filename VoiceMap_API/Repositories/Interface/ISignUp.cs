using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;

namespace VoiceMap_API.Repositories.Interface
{
    public interface ISignUp
    {
        Task<(long userId, string secretKey)> SignUpUser(UserDTO userDto);
        Task<List<string>> GetAllEncryptedEmailAsync();
        Task UpdateIsVerified(int UserId);
        Task<Models.Users> GetUserById(int UserId);
        Task<Models.Users> GetUserByEmail(string email);
        Task UpdateEmail(int userId, string email);
        Task DeActivateAccount(Users usr);
        Task<List<string>> LoadEncryptedEmailsAsync();
        Task DeleteAccount(Users usr);
        Task UpdatePassword(Users usr,string password);
        Task<(long userId, bool isValidKey)> GetUserBySecretKey(string key);
    }   
}
