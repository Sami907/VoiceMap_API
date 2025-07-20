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
    public class HomeController : ControllerBase
    {
        private readonly ISignUp _SignUpRepository;
        private readonly IUserVerification _IUserVerification;
        private readonly IUserLoginLogs _ILoginLogs;
        private readonly IUserProfiles _IUProfiles;
        private readonly IMapper _mapper;
        private readonly IExpertiseType _IExpTypes;
        private readonly IProfileType _IProfileTypes;
        private readonly IUserSecuritySettings _IUserSecSetting;
        private readonly IReactionTypes _reactionTypes;
        private readonly IPosts _posts;
        private readonly IPostCategories _postCategories;
        private readonly IPostReactions _postReactions;
        private readonly IPostComments _postComments;

        protected APIResponse _response;
        public HomeController(ISignUp SignUpRepository,IUserVerification userVerification, IUserLoginLogs userLoginLogs,IUserProfiles userProfiles,
            IMapper mapper,IExpertiseType expertiseType,IProfileType profileType,IUserSecuritySettings userSecuritySettings, IReactionTypes reactionTypes,
            IPosts posts,IPostCategories postCategories,IPostReactions postReactions,IPostComments postComments)
        {
            _SignUpRepository = SignUpRepository;
            _IUserVerification = userVerification;
            _ILoginLogs = userLoginLogs;
            _IUProfiles = userProfiles;
            _mapper = mapper;
            _IExpTypes = expertiseType;
            _IProfileTypes = profileType;
            _IUserSecSetting = userSecuritySettings;
            _reactionTypes = reactionTypes;
            _posts = posts;
            _postCategories = postCategories;
            _postReactions = postReactions;
            _postComments = postComments;
        }

        [HttpPost("saveUser")]
        public async Task<ActionResult<APIResponse>> SaveUser(UserDTO userDto)
        {
            var response = new APIResponse();
            try
            {
                if (userDto == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "Invalid data" };
                    return BadRequest(response);
                }

                
                var existingEmails = await _SignUpRepository.GetAllEncryptedEmailAsync();
                if(existingEmails.Count > 0){
                    foreach (var encrypted in existingEmails)
                    {
                        var decryptedEmail = await Methods.DecryptAsync(Methods.HexStringToByteArray(encrypted));
                        if (decryptedEmail.Equals(userDto.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            response.StatusCode = HttpStatusCode.Conflict; // 409 Conflict
                            response.IsSuccess = false;
                            response.ErrorMessages = new List<string> { "Email already exists. Please use a different email." };
                            return Conflict(response);
                        }
                    }
                }

                (long userId, string secretKey) = await _SignUpRepository.SignUpUser(userDto); //reuturn UserId

                if(userId > 0)
                {
                    var subject = "";
                    var message = "";
                    var resultRecord = await _IUserVerification.UserVerification(Convert.ToInt32(userId));
                    if(resultRecord != "")
                    {
                        string fileContent = $"Your secret key is: {secretKey}";
                        byte[] byteArray = Encoding.UTF8.GetBytes(fileContent);

                        using var ms = new MemoryStream(byteArray);

                        subject = "Verify Your Voice Map Account";
                        message = $@"
                        Dear User,

                        <br /><br />

                        Thank you for registering on Voice Map — your go-to platform for sharing local knowledge and tourism tips!

                        <br /><br />

                        To complete your registration and activate your account, please verify your email by entering the following 6-digit verification code in the app:

                        <br /><br />

                        <h2 style='letter-spacing: 3px;'>{resultRecord}</h2>

                        <br />

                        This code is unique to your account and will expire in 1 minute. Please do not share it with anyone.

                        <br /><br />

                        For your convenience, we have also attached a Notepad file that contains your secret key. This can be used for future account recovery or in case you forget your login credentials.<br /><br />


                        If you did not request this verification, please ignore this email.

                        <br /><br />

                        Thank you for joining the Voice Map community. We look forward to your valuable contributions!

                        <br /><br />

                        Best regards,<br />
                        The Voice Map Team
                        ";

                        await Methods.SendEmailWithAttachment(userDto.Email, subject, message, ms, "SecretKey.txt");
                    }
                }

                response.IsSuccess = true;
                response.Result = userId;
                response.Messages = new List<string> { "Account Created Successfully" };
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

        [HttpGet("OtpVerificationForAccount")]
        public async Task<ActionResult<APIResponse>> OtpVerification(string otp, int UserId)
        {
            var response = new APIResponse();

            try
            {
                var result = await _IUserVerification.OtpVerification(otp, UserId);
                response.IsSuccess = true;
                response.Result = result;
                if (result == 1)
                {
                    response.Messages = new List<string> { "OTP has expired. Please request a new one." };
                }
                else if (result == 2)
                {
                    response.Messages = new List<string> { "Invalid OTP. Please check and try again." };
                }
                else if (result == 3)
                {
                    response.Messages = new List<string> { "OTP verified successfully." };
                    await _SignUpRepository.UpdateIsVerified(UserId);
                    await _IUserVerification.DeleteOtpRecord(UserId, otp);
                }
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

        [HttpPut("ResendOtp")]
        public async Task<ActionResult<APIResponse>> ResendOtp(int UserId)
        {
            var response = new APIResponse();

            try
            {
                var result = await _IUserVerification.ResendOtp(UserId);
                response.IsSuccess = true;
                response.Result = result;
                if (result != "")
                {
                    var subject = "Verify Your Voice Map Account";

                    var message = $@"
                        Dear User,<br /><br />
                        Thank you for registering on <strong>Voice Map</strong> — your go-to platform for sharing local knowledge and tourism tips!<br /><br />
                        To complete your registration and activate your account, please verify your email by entering the following <strong>6-digit verification code</strong> in the app:<br /><br />
                        <h2 style='letter-spacing: 3px;'>{result}</h2><br />
                        This code is unique to your account and will expire in <strong>1 minute</strong>. Please do not share it with anyone.<br /><br />
                        If you did not request this verification, please ignore this email.<br /><br />
                        Thank you for joining the Voice Map community. We look forward to your valuable contributions!<br /><br />
                        Best regards,<br />
                        <strong>The Voice Map Team</strong>
                    ";

                    var userDto = await _SignUpRepository.GetUserById(UserId);
                    var decryptedEmail = await Methods.DecryptAsync(Methods.HexStringToByteArray(userDto.Email));
                    await Methods.SendEmailAsync(decryptedEmail, subject, message);

                    response.Messages = new List<string> { "OTP has been sent to your email address." };
                }
                else
                {
                    response.Messages = new List<string> { "Unable to generate OTP. Please try again." };
                    response.IsSuccess = false;
                }
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

        [HttpPost("login")]
        public async Task<APIResponse> LoginUserAsync(LoginDTO loginDTO)
        {
            var response = new APIResponse();

            try
            {
                var encryptedEmails = await _SignUpRepository.LoadEncryptedEmailsAsync();

                foreach (var encryptedEmail in encryptedEmails)
                {
                    string decryptedEmail = null;

                    if (!Methods.IsHexString(encryptedEmail))
                    {
                        decryptedEmail = await Methods.DecryptAsync(Methods.HexStringToByteArray(encryptedEmail));
                    }
                    else
                    {
                        decryptedEmail = encryptedEmail; // already decrypted;
                    }

                    if (!string.IsNullOrEmpty(decryptedEmail) &&
                        decryptedEmail.Equals(loginDTO.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var user = await _SignUpRepository.GetUserByEmail(encryptedEmail); // Note: encryptedEmail used here
                        if (user == null)
                        {
                            response.StatusCode = HttpStatusCode.NotFound;
                            response.IsSuccess = false;
                            response.ErrorMessages = new List<string> { "User not found." };
                            return response;
                        }

                        string hashPass = Methods.HashPassword(loginDTO.Password);

                        if (hashPass != user.PasswordHash)
                        {
                            response.StatusCode = HttpStatusCode.Unauthorized;
                            response.IsSuccess = false;
                            response.ErrorMessages = new List<string> { "Invalid password." };
                            return response;
                        }

                        if(!user.IsActivated)
                        {
                            response.IsSuccess = false;
                            response.StatusCode = HttpStatusCode.BadRequest;
                            response.Result = new { User = user, decryptedEmail};
                            response.ErrorMessages = new List<string> { "Your account is inactive." };
                            return response;
                        }

                        bool isSuccessfull = true;

                        if (user.UserSecuritySettings.Count > 0)
                        {
                            isSuccessfull = user.UserSecuritySettings[0].TwoFactorAuth;
                            if (isSuccessfull == true)
                            {
                                string otp = await _IUserVerification.UserVerification(Convert.ToInt32(user.Id));
                                if (otp != "")
                                {
                                    var subject = "Your Voice Map Two-Factor Authentication Code";

                                    var message = $@"
                                        Dear User,<br /><br />
                                        As part of our commitment to keeping your <strong>Voice Map</strong> account secure, we use <strong>Two-Factor Authentication (2FA)</strong>.<br /><br />
                                        To continue with your login, please enter the following <strong>6-digit verification code</strong> in the app:<br /><br />
                                        <h2 style='letter-spacing: 3px;'>{otp}</h2><br />
                                        This code is valid for <strong>1 minute</strong> and is required to complete your sign-in process. Please do not share this code with anyone.<br /><br />
                                        If you did not attempt to log in, we recommend changing your password immediately or contacting our support team.<br /><br />
                                        Thank you for helping us keep your Voice Map account secure.<br /><br />
                                        Best regards,<br />
                                        <strong>The Voice Map Team</strong>
                                    ";

                                    await Methods.SendEmailAsync(decryptedEmail, subject, message);
                                }
                            }
                        }

                        long loginid = await _ILoginLogs.SaveLoginLogs(user.Id, isSuccessfull, loginDTO.ipAddress, loginDTO.deviceInfo);

                        var profileRecord = await _IUProfiles.GetUserProfileById(user.Id);
                        string token = Methods.GenerateJwtToken(user.Id.ToString());
                        response.IsSuccess = true;
                        response.StatusCode = HttpStatusCode.OK;
                        response.Result = new { User = user, LoginId = loginid, IsSuccessfull = isSuccessfull, decryptedEmail,
                            isProfileExists = profileRecord != null ? true : false,loginToken = token,profile = profileRecord};
                        response.Messages = new List<string> { "Login successful." };
                        return response;
                    }
                }

                response.StatusCode = HttpStatusCode.NotFound;
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { "No account found for this email." };
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages = new List<string> { ex.Message };
                return response;
            }
        }

        [HttpPost("saveUserProfile")]
        public async Task<ActionResult<APIResponse>> SaveProfile([FromForm] CreateProfileDTO profileDTO)
        {
            var response = new APIResponse();
            try
            {
                var profilePicPath = await Methods.UploadFileAsync(profileDTO.Profile.ProfilePictureUrl == null ? null : profileDTO.Profile.ProfilePictureUrl, "User/ProfilePictures");
                var coverPicPath = await Methods.UploadFileAsync(profileDTO.Profile.CoverImageUrl, "User/CoverPictures");
                
                var profile = _mapper.Map<UserProfiles>(profileDTO.Profile);
                var userSetting = _mapper.Map<UserSecuritySettings>(profileDTO.UserSecuritySetting);

                profile.CoverImageUrl = coverPicPath;
                profile.ProfilePictureUrl = profilePicPath;
                profile.CreatedAt = DateTime.UtcNow;

                var result = await _IUProfiles.SaveProfileAsync(profile);
                if(result != null)
                {
                    await _IUserSecSetting.SaveSecuritySettings(userSetting);
                }

                var profileRecord = await _IUProfiles.GetUserProfileById(profileDTO.Profile.UserId);
                response.IsSuccess = true;
                response.Result = null;
                response.Messages = new List<string> { "Profile completed successfully, You're all set!" };
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

        [HttpGet("getExpertiseTypes")]
        public async Task<ActionResult<APIResponse>> GetExpertise()
        {
            var response = new APIResponse();

            try
            {
                var result = await _IExpTypes.GetTypes();
                response.IsSuccess = true;
                response.Result = result;
                response.Messages = new List<string> { "successfully get." };
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

        [HttpGet("getProfileTypes")]
        public async Task<ActionResult<APIResponse>> GetProfileTypes()
        {
            var response = new APIResponse();

            try
            {
                var result = await _IProfileTypes.GetTypes();
                response.IsSuccess = true;
                response.Result = result;
                response.Messages = new List<string> { "successfully get." };
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

        [HttpGet("getUser")]
        public async Task<ActionResult<APIResponse>> GetUserById(int userId)
        {
            var response = new APIResponse();

            try
            {
                var user = await _SignUpRepository.GetUserById(userId);
                
                user.Email = await Methods.DecryptAsync(Methods.HexStringToByteArray(user.Email));
                user.SecretKey = await Methods.DecryptAsync(Methods.HexStringToByteArray(user.SecretKey));

                var result = user;
                response.IsSuccess = true;
                response.Result = result;
                response.Messages = new List<string> { "successfully get." };
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

        [HttpGet("change-email-request")]
        public async Task<ActionResult<APIResponse>> ChangeEmailRequest(int userId, string email, string password)
        {
            var response = new APIResponse();

            try
            {
                var existingEmails = await _SignUpRepository.GetAllEncryptedEmailAsync();
                if (existingEmails.Count > 0)
                {
                    foreach (var encrypted in existingEmails)
                    {
                        var decryptedEmail = await Methods.DecryptAsync(Methods.HexStringToByteArray(encrypted));
                        if (decryptedEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                        {
                            response.StatusCode = HttpStatusCode.Conflict; // 409 Conflict
                            response.IsSuccess = false;
                            response.ErrorMessages = new List<string> { "Email already exists. Please use a different email." };
                            return Conflict(response);
                        }
                    }
                }

                string hashPass = Methods.HashPassword(password);
                var user = await _SignUpRepository.GetUserById(userId);

                if (hashPass != user.PasswordHash)
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "Invalid password." };
                    return Conflict(response);
                }

                string otp = await _IUserVerification.UserVerification(Convert.ToInt32(user.Id));
                var subject = "Verify Your New Email Address – Voice Map";
                var message = $@"
                        Dear User,

                        <br /><br />

                        We received a request to change the email address associated with your Voice Map account.

                        <br /><br />

                        To confirm this change and keep your account secure, please enter the following 6-digit verification code in the app:

                        <br /><br />

                        <h2 style='letter-spacing: 3px;'>{otp}</h2>

                        <br />

                        This code is valid for 1 minute. Please do not share it with anyone.

                        <br /><br />

                        If you did not request this email change, please ignore this message or contact our support team immediately.

                        <br /><br />

                        Thank you for being part of the Voice Map community!

                        <br /><br />

                        Best regards,<br />
                        The Voice Map Team
                        ";

                await Methods.SendEmailAsync(email, subject, message);
                response.IsSuccess = true;
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

        [HttpPut("UpdateEmail")]
        public async Task<ActionResult<APIResponse>> UpdateUserEmail(int userId, string email)
        {
            var response = new APIResponse();

            try
            {
                await _SignUpRepository.UpdateEmail(userId, email);
                response.IsSuccess = true;
                response.Messages = new List<string> { "Email successfully changed." };
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

        [HttpGet("OtpVerification")]
        public async Task<ActionResult<APIResponse>> OtpVerificationForOther(string otp, int UserId)
        {
            var response = new APIResponse();

            try
            {
                var result = await _IUserVerification.OtpVerification(otp, UserId);
                response.IsSuccess = true;
                response.Result = result;
                if (result == 1)
                {
                    response.Messages = new List<string> { "OTP has expired. Please request a new one." };
                }
                else if (result == 2)
                {
                    response.Messages = new List<string> { "Invalid OTP. Please check and try again." };
                }
                else if (result == 3)
                {
                    response.Messages = new List<string> { "OTP verified successfully." };
                    await _IUserVerification.DeleteOtpRecord(UserId, otp);
                }
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

        [HttpPut("DeactivateAccount")]
        public async Task<ActionResult<APIResponse>> DeactivateAccount(int userId,string password)
        {
            var response = new APIResponse();

            try
            {
                var result = await _SignUpRepository.GetUserById(userId);
                if(result != null)
                {
                    string hashPass = Methods.HashPassword(password);
                    if(result.PasswordHash != hashPass)
                    {
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        response.IsSuccess = false;
                        response.ErrorMessages = new List<string> { "Invalid password." };
                        return Conflict(response);
                    }

                    await _SignUpRepository.DeActivateAccount(result);
                    response.IsSuccess = true;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NotFound; // 409 Conflict
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "user not exists.." };
                    return Conflict(response);
                }
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

        [HttpPut("ReactivateAccount")]
        public async Task<ActionResult<APIResponse>> DeactivateAccount(int userId)
        {
            var response = new APIResponse();

            try
            {
                var result = await _SignUpRepository.GetUserById(userId);
                if (result != null)
                {
                    await _SignUpRepository.DeActivateAccount(result);
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    response.Messages = new List<string> { "Your account is reactive login again." };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NotFound; // 409 Conflict
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "user not exists.." };
                    return Conflict(response);
                }
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

        [HttpPut("DeleteAccount")]
        public async Task<ActionResult<APIResponse>> DeleteAccount(int userId, string password)
        {
            var response = new APIResponse();

            try
            {
                var result = await _SignUpRepository.GetUserById(userId);
                if (result != null)
                {
                    string hashPass = Methods.HashPassword(password);
                    if (result.PasswordHash != hashPass)
                    {
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        response.IsSuccess = false;
                        response.ErrorMessages = new List<string> { "Invalid password." };
                        return Conflict(response);
                    }

                    await _SignUpRepository.DeleteAccount(result);
                    var subject = "Your Voice Map Account Has Been Deleted";
                    var message = $@"
                    Dear User,

                    <br /><br />

                    We're sorry to see you go.

                    <br /><br />

                    Your Voice Map account has been successfully deleted. If this was a mistake or you change your mind, you're always welcome to sign up again.

                    <br /><br />

                    Thank you for being part of the Voice Map community.

                    <br /><br />

                    Best regards,<br />
                    <strong>The Voice Map Team</strong>
                ";
                    string decryptedEmail = await Methods.DecryptAsync(Methods.HexStringToByteArray(result.Email));
                    await Methods.SendEmailAsync(decryptedEmail, subject, message);
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    response.Messages = new List<string> { "Your account is deleted." };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NotFound; // 409 Conflict
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "user not exists.." };
                    return Conflict(response);
                }
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

        [HttpPut("updatePassword")]
        public async Task<ActionResult<APIResponse>> UpdatePassword(int userId, string oldPassword, string newPassword)
        {
            var response = new APIResponse();

            try
            {
                var result = await _SignUpRepository.GetUserById(userId);
                if (result != null)
                {
                    string hashPass = Methods.HashPassword(oldPassword);
                    if (result.PasswordHash != hashPass)
                    {
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        response.IsSuccess = false;
                        response.ErrorMessages = new List<string> { "Invalid password." };
                        return Conflict(response);
                    }

                    string hashpass = Methods.HashPassword(newPassword);
                    await _SignUpRepository.UpdatePassword(result, hashpass);
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    response.Messages = new List<string> { "Your password has been changed successfully." };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NotFound; // 409 Conflict
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "user not exists.." };
                    return Conflict(response);
                }
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

        [HttpGet("getAccountBySecretKey")]
        public async Task<ActionResult<APIResponse>> GetUserAccount(string key)
        {
            var response = new APIResponse();

            try
            {
                (long userId, bool isvalidkey) = await _SignUpRepository.GetUserBySecretKey(key);
                if(isvalidkey == false)
                {
                    response.StatusCode = HttpStatusCode.NotFound; // 409 Conflict
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "Invalid secret key.." };
                    return Conflict(response);
                }

                var profileRecord = await _IUProfiles.GetUserProfileById(userId);
                response.IsSuccess = true;
                response.Result = profileRecord;
                response.Messages = new List<string> { "successfully get." };
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

        [HttpPut("UpdatePasswordFromForgetPassword")]
        public async Task<ActionResult<APIResponse>> ForgetPassword(int userId, string newPassword)
        {
            var response = new APIResponse();

            try
            {
                var result = await _SignUpRepository.GetUserById(userId);
                if (result != null)
                {
                    string hashpass = Methods.HashPassword(newPassword);
                    await _SignUpRepository.UpdatePassword(result, hashpass);
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    response.Messages = new List<string> { "Your password has been changed successfully." };
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NotFound; // 409 Conflict
                    response.IsSuccess = false;
                    response.ErrorMessages = new List<string> { "user not exists.." };
                    return Conflict(response);
                }
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

        [HttpGet("getReactionTypes")]
        public async Task<ActionResult<APIResponse>> ReactionTypes()
        {
            var response = new APIResponse();

            try
            {
                var result = await _reactionTypes.GetReactions();
                response.IsSuccess = true;
                response.Result = result;
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

        [HttpPost("savePost")]
        public async Task<ActionResult<APIResponse>> SavePost([FromForm] PostDTO postDTO)
        {
            var response = new APIResponse();
            try
            {
                var postImage = await Methods.UploadFileAsync(postDTO.PostImageUrl == null ? null : postDTO.PostImageUrl, "User/PostImages");
                var voice = await Methods.UploadFileAsync(postDTO.VoiceUrl == null ? null : postDTO.VoiceUrl, "User/Voices");

                var post = _mapper.Map<Posts>(postDTO);

                post.PostImageUrl = postImage;
                post.VoiceUrl = voice;
                post.PostTime = DateTime.Now;

                await _posts.SavePost(post);

                response.IsSuccess = true;
                response.Result = null;
                response.Messages = new List<string> { "posted.." };
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

        [HttpGet("getCategoryTypes")]
        public async Task<ActionResult<APIResponse>> CategoryTypes()
        {
            var response = new APIResponse();

            try
            {
                var result = await _postCategories.GetCategories();
                response.IsSuccess = true;
                response.Result = result;
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

        //Feed API
        [HttpGet("getFeed")]
        public async Task<ActionResult<APIResponse>> GetFeed(int userId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _posts.GetFeed(userId);
                response.IsSuccess = true;
                response.Result = result;
                response.Messages = new List<string> { "fetching.." };
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

        
        [HttpPost("saveNGetReactions")]
        public async Task<ActionResult<APIResponse>> SaveReactions(long postId, long userId, int reactionId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _postReactions.SaveNGetReaction(postId, userId, reactionId);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = System.Linq.Enumerable.Count(result);
                response.Messages = new List<string> { "reaction added.." };
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

        [HttpPost("saveNGetComments")]
        public async Task<ActionResult<APIResponse>> SaveComment(long postId, long userId, string commentText)
        {
            var response = new APIResponse();
            try
            {
                var result = await _postComments.SaveNGetComments(postId, userId, commentText);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = System.Linq.Enumerable.Count(result);
                response.Messages = new List<string> { "comment added.." };
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

        [HttpPut("updateNGetComments")]
        public async Task<ActionResult<APIResponse>> UpdateComment(long postId, long commentId, string commentText)
        {
            var response = new APIResponse();
            try
            {
                var result = await _postComments.UpdateNGetComments(postId, commentId, commentText);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = System.Linq.Enumerable.Count(result);
                response.Messages = new List<string> { "comment updated.." };
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

        [HttpDelete("deleteNGetComments")]
        public async Task<ActionResult<APIResponse>> DeleteComment(long postId, long commentId)
        {
            var response = new APIResponse();
            try
            {
                var result = await _postComments.DeleteNGetComments(postId, commentId);
                response.IsSuccess = true;
                response.Result = result;
                response.Count = System.Linq.Enumerable.Count(result);
                response.Messages = new List<string> { "comment deleted.." };
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

        [HttpDelete("deletePostWithDependencies")]
        public async Task<ActionResult<APIResponse>> DeletePost(long postId)
        {
            var response = new APIResponse();
            try
            {
                bool result = await _posts.DeletePostWithDependencies(postId);
                response.IsSuccess = true;
                response.Result = result;
                response.Messages = new List<string> { "post deleted.." };
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
    }
}