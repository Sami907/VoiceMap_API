using System.Collections.Generic;
using System.Net;

namespace BookPlazaAPI.AppClasses
{
    public class APIResponse
    {
        public APIResponse()
        {
            ErrorMessages = new List<string>();
        }
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }
        public List<string> Messages { get; set; }
        public int? Count { get; set; }
        public object Result { get; set; }
        public int? Code { get; set; } = 01;
    }
}
