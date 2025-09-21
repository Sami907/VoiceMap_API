using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IGroupMembers
    {
        Task<int> ToggleGroupMembership(int userId, int groupId);
    }
}
