using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BookPlazaAPI.AppClasses;
using Microsoft.AspNetCore.Mvc;
using VoiceMap_API.AppClasses;
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
        protected APIResponse _response;
        public HomeController(ISignUp SignUpRepository,IUserVerification userVerification)
        {
            _SignUpRepository = SignUpRepository;
            _IUserVerification = userVerification;
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

                long result = await _SignUpRepository.SignUpUser(userDto); //reuturn UserId

                if(result > 0)
                {
                    var subject = "";
                    var message = "";
                    var resultRecord = await _IUserVerification.UserVerification(Convert.ToInt32(result));
                    if(resultRecord != "")
                    {
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

                        This code is unique to your account and will expire in 24 hours. Please do not share it with anyone.

                        <br /><br />

                        If you did not request this verification, please ignore this email.

                        <br /><br />

                        Thank you for joining the Voice Map community. We look forward to your valuable contributions!

                        <br /><br />

                        Best regards,<br />
                        The Voice Map Team
                        ";

                       
                    }
                    await Methods.SendEmailAsync(userDto.Email, subject, message);
                }

                response.IsSuccess = true;
                response.Result = result;
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

        [HttpGet("OtpVerification")]
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
                        This code is unique to your account and will expire in <strong>24 hours</strong>. Please do not share it with anyone.<br /><br />
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
    }
}