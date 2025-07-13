using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceMap_API.Models
{
    public interface IPostCategories
    {
        Task<IEnumerable<PostCategories>> GetCategories();
    }
}
