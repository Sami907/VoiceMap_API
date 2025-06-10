using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Repositories.DTO;

namespace VoiceMap_API.Repositories.Interface
{
    public interface ISignUp
    {
        Task<long> SignUpUser(UserDTO userDto);
        Task<List<string>> GetAllEncryptedEmailAsync();
        Task UpdateIsVerified(int UserId);
        Task<Models.Users> GetUserById(int UserId);
    }   
}
