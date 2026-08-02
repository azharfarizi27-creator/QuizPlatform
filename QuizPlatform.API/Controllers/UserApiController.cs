using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class UserApiController : ApiController
    {
        private readonly IProfileService profileService =
            new ProfileService();

        private readonly IAdminService adminService =
            new AdminService();

        private readonly IActivityLogService activityLogService =
            new ActivityLogService();

        [Authorize]
        [HttpGet]
        [Route("api/User/GetAllUsers")]
        public IHttpActionResult GetAllUsers()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            var result =
                adminService.GetAllUser();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/User/CreateUser")]
        public IHttpActionResult CreateUser(
            [FromBody] User newUser
        )
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            if (newUser == null)
                return BadRequest("Data user tidak boleh kosong");

            try
            {
                adminService.CreateUser(newUser);

                activityLogService.CreateActivityLog(
                    GetLoginUserId(),
                    "CREATE_USER",
                    "Admin menambahkan user: " + newUser.Username
                );

                return Ok("User berhasil ditambahkan");
            }
            catch (Exception ex)
            {
                return BadRequest(
                    GetFullErrorMessage(ex)
                );
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Profile")]
        public IHttpActionResult GetProfile()
        {
            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            var result =
                profileService.GetStudentProfileStats(
                    userId.Value
                );

            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        [Route("api/Profile/Update")]
        public IHttpActionResult UpdateProfile(
            UpdateProfileDto request)
        {
            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            try
            {
                profileService.UpdateProfile(
                    userId.Value,
                    request
                );

                activityLogService.CreateActivityLog(
                    userId,
                    "UPDATE_PROFILE",
                    "User memperbarui profile"
                );

                return Ok("Profile berhasil diupdate");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/Profile/ChangePassword")]
        public IHttpActionResult ChangePassword(
            ChangePasswordDto request)
        {
            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            try
            {
                profileService.ChangePassword(
                    userId.Value,
                    request
                );

                activityLogService.CreateActivityLog(
                    userId,
                    "CHANGE_PASSWORD",
                    "User mengubah password"
                );

                return Ok("Password berhasil diubah");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/Profile/Image")]
        public IHttpActionResult UpdateProfileImage(
            UploadProfileImageDto request)
        {
            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            try
            {
                profileService.UpdateProfileImage(
                    userId.Value,
                    request.ProfileImage
                );

                activityLogService.CreateActivityLog(
                    userId,
                    "UPDATE_PROFILE_IMAGE",
                    "User memperbarui foto profile"
                );

                return Ok(
                    "Foto profile berhasil diupdate"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Profile/UploadImage")]
        public async Task<IHttpActionResult> UploadImage()
        {
            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Request harus multipart/form-data");

            try
            {
                var uploadFolder =
                    System.Web.HttpContext.Current.Server.MapPath("~/Uploads/Profile");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var provider =
                    new MultipartFormDataStreamProvider(uploadFolder);

                await Request.Content.ReadAsMultipartAsync(provider);

                if (provider.FileData == null ||
                    provider.FileData.Count == 0)
                {
                    return BadRequest("File tidak ditemukan");
                }

                var file =
                    provider.FileData[0];

                var originalName =
                    file.Headers.ContentDisposition.FileName.Replace("\"", "");

                var extension =
                    Path.GetExtension(originalName);

                var fileName =
                    "profile_" +
                    userId.Value +
                    "_" +
                    DateTime.Now.Ticks +
                    extension;

                var newPath =
                    Path.Combine(uploadFolder, fileName);

                File.Move(
                    file.LocalFileName,
                    newPath
                );

                var dbPath =
                    "/Uploads/Profile/" + fileName;

                profileService.UpdateProfileImage(
                    userId.Value,
                    dbPath
                );

                activityLogService.CreateActivityLog(
                    userId,
                    "UPLOAD_PROFILE_IMAGE",
                    "User mengupload foto profile"
                );

                return Ok(new
                {
                    message = "Foto profile berhasil diupload",
                    profileImage = dbPath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int? GetLoginUserId()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            return int.Parse(userIdClaim.Value);
        }

        private string GetFullErrorMessage(Exception ex)
        {
            if (ex == null)
                return "Terjadi kesalahan";

            var message =
                ex.Message;

            var inner =
                ex.InnerException;

            while (inner != null)
            {
                message += " | Inner: " + inner.Message;
                inner = inner.InnerException;
            }

            return message;
        }
    }
}