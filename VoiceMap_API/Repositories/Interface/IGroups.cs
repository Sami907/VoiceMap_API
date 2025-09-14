using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IGroups 
    {
        Task CreateGroup(Groups grp);
    }
}
