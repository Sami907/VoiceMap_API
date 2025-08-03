using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IUserProfiles
    {
        Task <dynamic> SaveProfileAsync(UserProfiles up);
        Task <dynamic> GetUserProfileById(long userId);
        Task <dynamic> UpdateProfilePhoto(UpdatePhotoDTO dto);
        Task<bool> UpdateProfileInfo(UpdateUserProfileInfoDTO dto);
    }
}
