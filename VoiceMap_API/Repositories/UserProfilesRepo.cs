using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoiceMap_API.AppClasses;
using VoiceMap_API.Models;
using VoiceMap_API.Repositories.DTO;
using VoiceMap_API.Repositories.Interface;

namespace VoiceMap_API.Repositories
{
    public class UserProfilesRepo : IUserProfiles
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext.AppDbContext _context;

        public UserProfilesRepo(AppDbContext.AppDbContext context,  IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<dynamic> SaveProfileAsync(UserProfiles up)
        {
            _context.UserProfiles.Add(up);
            await _context.SaveChangesAsync();

            return _context.UserProfiles.Where(w=> w.UserId == up.UserId).FirstOrDefaultAsync();
        }

        public async Task<dynamic> GetUserProfileById(long userId)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var result = await (from user in _context.UserProfiles
                                where user.UserId == userId
                                join u in _context.Users on user.UserId equals u.Id
                                where user.UserId == userId && u.IsDeleted == false && u.IsActivated == true

                                join profileType in _context.ProfileType on user.ProfileTypeId equals profileType.id into pt
                                from profileType in pt.DefaultIfEmpty()

                                join expertise in _context.ExpertiseType on user.ExpertiseId equals expertise.id into exp
                                from expertise in exp.DefaultIfEmpty()

                                select new
                                {
                                    user.UserId,
                                    user.FullName,
                                    user.Gender,
                                    user.Bio,
                                    ProfileType = profileType != null ? profileType.Name : null,
                                    Expertise = expertise != null ? expertise.Name : null,
                                    user.ProfilePictureUrl,
                                    user.CoverImageUrl,
                                    user.ProfileTypeId,
                                    user.ExpertiseId,
                                    user.LivesIn,
                                    user.From
                                }).FirstOrDefaultAsync();

            if (result == null)
                return null;

            var profilePic = result.ProfilePictureUrl != null
                ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(result.ProfilePictureUrl)}"
                : null;

            var coverPic = result.CoverImageUrl != null
                ? $"{scheme}://{host}/User/CoverPictures/{Path.GetFileName(result.CoverImageUrl)}"
                : null;

            return new
            {
                result.UserId,
                result.FullName,
                result.Gender,
                result.Bio,
                ProfileType = result.ProfileType,
                Expertise = result.Expertise,
                ProfilePictureUrl = profilePic,
                CoverImageUrl = coverPic,
                result.ProfileTypeId,
                result.ExpertiseId,
                result.LivesIn,
                result.From
            };
        }

        public async Task<dynamic> UpdateProfilePhoto(UpdatePhotoDTO dto)
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(w => w.UserId == dto.userId);
            if (userProfile == null)
                return false;

            if (dto.update == "profile")
            {
                if (!string.IsNullOrEmpty(userProfile.ProfilePictureUrl))
                {
                    var relativePath = userProfile.ProfilePictureUrl.TrimStart('/', '\\');
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var profilePicPath = await Methods.UploadFileAsync(dto.profilePhoto, "User/ProfilePictures");
                userProfile.ProfilePictureUrl = profilePicPath ?? userProfile.ProfilePictureUrl;
            }
            else if (dto.update == "cover")
            {
                if (!string.IsNullOrEmpty(userProfile.CoverImageUrl))
                {
                    var relativePath = userProfile.CoverImageUrl.TrimStart('/', '\\');
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var coverPicPath = await Methods.UploadFileAsync(dto.coverPhoto, "User/CoverPictures");
                userProfile.CoverImageUrl = coverPicPath ?? userProfile.CoverImageUrl;
            }
            else
            {
                return null;
            }

            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();

            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var profilePic = userProfile.ProfilePictureUrl != null
               ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(userProfile.ProfilePictureUrl)}"
               : null;

            userProfile.ProfilePictureUrl = profilePic;

            return userProfile;
        }

        public async Task<bool> UpdateProfileInfo(UpdateUserProfileInfoDTO dto)
        {
            var existUser = await _context.Users.Where(w => w.Id == dto.UserId && w.IsActivated == true && w.IsDeleted == false).FirstOrDefaultAsync();
           
            if (existUser == null)
                return false;

            var userProfile = await _context.UserProfiles.Where(w => w.UserId == dto.UserId).FirstOrDefaultAsync();

            if (userProfile == null)
                return false;

            userProfile.Bio = dto.Bio;
            userProfile.ProfileTypeId = dto.ProfileTypeId;
            userProfile.ExpertiseId = dto.ExpertiseId;
            userProfile.Gender = dto.Gender;
            userProfile.LivesIn = dto.country;
            userProfile.From = dto.city;

            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<dynamic> SearchProfiles(string query, int userId, int skip = 0, int take = 20)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var host = _httpContextAccessor.HttpContext.Request.Host;

            var profiles = await _context.UserProfiles
                .Where(u => u.FullName.Contains(query) && u.UserId != userId)
                .OrderBy(u => u.Id) 
                .Skip(skip)
                .Take(take)
                .Select(u => new
                {
                    id = u.Id,
                    userId = u.UserId,
                    FullName = u.FullName,
                    Bio = u.Bio,
                    ProfilePicture = u.ProfilePictureUrl != null
                        ? $"{scheme}://{host}/User/ProfilePictures/{Path.GetFileName(u.ProfilePictureUrl)}"
                        : null
                })
                .ToListAsync();

            return profiles;
        }
    }
}
