using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VoiceMap_API.AppClasses
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            string id = connection.User?.FindFirst(ClaimTypes.Sid.ToString())?.Value;
            return id;
        }
    }
}
