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
        private readonly INotifications _notifications;

        protected APIResponse _response;
        public SiteController(IMapper mapper, IUserProfiles userProfiles, INotifications notifications)
        {
            _IUProfiles = userProfiles;
            _mapper = mapper;
            _notifications = notifications;
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

        [HttpPut("updateUserInfo")]
        public async Task<ActionResult<APIResponse>> UpdateUser(UpdateUserProfileInfoDTO profileDTO)
        {
            var response = new APIResponse();

            try
            {
                var isUpdated = await _IUProfiles.UpdateProfileInfo(profileDTO);

                if (!isUpdated)
                {
                    response.IsSuccess = false;
                    response.Messages = new List<string> { "User not found or update failed." };
                    return NotFound(response);
                }

                response.Result = isUpdated;
                response.IsSuccess = true;
                response.Messages = new List<string> { "Profile updated successfully." };
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

        [HttpGet("getNotifications")]
        public async Task<ActionResult<APIResponse>> GetNotifications(int userId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _notifications.GetUserNotificationsAsync(userId);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = result.Count;
                response.Messages = new List<string> { "Notifications fetched." };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpDelete("deleteNotification")]
        public async Task<ActionResult<APIResponse>> DeleteNotification(int notificationId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _notifications.DeleteNotificationAsync(notificationId);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = result.Count;
                response.Messages = new List<string> { "Notification deleted." };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpPut("updateIsReadNotification")]
        public async Task<ActionResult<APIResponse>> UpdateIsRead(int notificationId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _notifications.IsReadNotificationAsync(notificationId);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = result.Count;
                response.Messages = new List<string> { "updated is read." };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
                response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}