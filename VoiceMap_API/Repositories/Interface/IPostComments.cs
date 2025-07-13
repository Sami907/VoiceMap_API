using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IPostComments
    {
        Task<IEnumerable<dynamic>> SaveNGetComments(long postId, long userId, string comment);
    }
}
