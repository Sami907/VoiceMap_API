using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookPlazaAPI.AppClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories;
using VoiceMap_API.Repositories.DTO;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly IUserProfiles _IUProfiles;
        private readonly IMapper _mapper;

        protected APIResponse _response;
        public SiteController(IMapper mapper, IUserProfiles userProfiles)
        {
            _IUProfiles = userProfiles;
            _mapper = mapper;
        }

        [HttpPut("updateUserPhoto")]
        public async Task<ActionResult<APIResponse>> SaveProfile([FromForm] UpdatePhotoDTO profileDTO)
        {
            var response = new APIResponse();

            try
            {
                var isUpdated = await _IUProfiles.UpdateProfilePhoto(profileDTO);

                if (isUpdated == null)
                {
                    response.IsSuccess = false;
                    response.Messages = new List<string> { "User not found or update failed." };
                    return NotFound(response);
                }

                response.Result = isUpdated;
                response.IsSuccess = true;
                response.Messages = new List<string> { "Profile photo updated successfully." };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}