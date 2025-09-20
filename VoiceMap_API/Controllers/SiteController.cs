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
        private readonly IPosts _iPost;
        private readonly IGroups _iGrp;
        protected APIResponse _response;
        public SiteController(IMapper mapper, IUserProfiles userProfiles, INotifications notifications, IPosts posts, IGroups iGrp)
        {
            _IUProfiles = userProfiles;
            _mapper = mapper;
            _notifications = notifications;
            _iPost = posts;
            _iGrp = iGrp;
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

        [HttpGet("searchProfilesByQueryParam")]
        public async Task<ActionResult<APIResponse>> SearchProfiles(string query, int userId, int skip = 0, int take = 20)
        {
            var response = new APIResponse();
            try
            {
                var result = await _IUProfiles.SearchProfiles(query, userId, skip, take);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = result.Count;
                response.Messages = new List<string> { "successfull" };
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

        [HttpGet("searchPostByQueryParam")]
        public async Task<ActionResult<APIResponse>> searchPost(string query, int userId, int skip = 0, int take = 20)
        {
            var response = new APIResponse();
            try
            {
                var result = await _iPost.GetPostByQueryParam(query, userId, skip, take);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = result.Count;
                response.Messages = new List<string> { "successfull" };
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

        [HttpPost("CreateGroup")]
        public async Task<ActionResult<APIResponse>> CreateGroup([FromForm] GroupDTO grpDTO)
        {
            var response = new APIResponse();
            try
            {
                var profileImage = await Methods.UploadFileAsync(grpDTO.GroupPic == null ? null : grpDTO.GroupPic, "User/GroupProfilePhotos");
                var voice = await Methods.UploadFileAsync(grpDTO.CoverPhoto == null ? null : grpDTO.CoverPhoto, "User/GroupCoverPhotos");

                var group = _mapper.Map<Groups>(grpDTO);

                group.GroupPic = profileImage;
                group.CoverPhoto = voice;

                group.GroupUrl = Methods.GenerateGroupUrl(grpDTO.GroupName);

                await _iGrp.CreateGroup(group);

                response.IsSuccess = true;
                response.Result = null;
                response.Messages = new List<string> { "Group created successfully!" };
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }

            return Ok(response);
        }

        [HttpGet("getCurrentUserGroups")]
        public async Task<ActionResult<APIResponse>> GetUserGroups(int userId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _iGrp.GetCurrentUserGroup(userId);
                response.IsSuccess = true;
                response.Result = result;
                response.Messages = new List<string> { "successfull" };
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

        [HttpGet("searchGroupsByQueryParam")]
        public async Task<ActionResult<APIResponse>> SearchGroups(string query, int userId, int skip = 0, int take = 20)
        {
            var response = new APIResponse();
            try
            {
                var result = await _iGrp.SearchGroups(query, userId, skip, take);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = result.Count;
                response.Messages = new List<string> { "successfull" };
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