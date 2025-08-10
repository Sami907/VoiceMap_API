using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.Models;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IPosts
    {
        Task SavePost(Posts posts);
        Task<IEnumerable<dynamic>> GetFeed(int userId, bool applyIdFilter);
        Task <bool> DeletePostWithDependencies(long postId);
    }
}
