using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Repositories.Interface
{
    public interface IPostReactions
    {
        Task<IEnumerable<dynamic>> SaveNGetReaction(long postId, long userId, int reactionId);
    }
}
