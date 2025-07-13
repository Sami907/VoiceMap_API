using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;

namespace VoiceMap_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<UserProfiles, ProfileDTO>().ReverseMap();
            CreateMap<UserSecuritySettings, UserSecuritySettingDTO>().ReverseMap();
            CreateMap<Posts, PostDTO>().ReverseMap();
        }
           
    }
}
