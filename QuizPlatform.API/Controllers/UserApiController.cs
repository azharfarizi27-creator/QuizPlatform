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
        private readonly IQuizService service =
            new QuizService();

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

            // hanya admin
            if (role != "Admin")
                return Unauthorized();

            var result =
                service.GetAllUser();

            return Ok(
                result
            );
        }

        [Authorize]
        [HttpPost]
        [Route("api/User/CreateUser")]
        public IHttpActionResult CreateUser(
            [FromBody] User newUser)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya admin
            if (role != "Admin")
                return Unauthorized();

            service.CreateUser(
                newUser
            );

            return Ok(
                "User berhasil ditambahkan"
            );
        }

        [Authorize]
        [HttpGet]
        [Route("api/Profile")]
        public IHttpActionResult GetProfile()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            var userId =
                int.Parse(userIdClaim.Value);

            var result =
                service.GetStudentProfileStats(userId);

            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        [Route("api/Profile/Update")]
        public IHttpActionResult UpdateProfile(
    UpdateProfileDto request)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            var userId =
                int.Parse(userIdClaim.Value);

            service.UpdateProfile(userId, request);

            return Ok("Profile berhasil diupdate");
        }


        [Authorize]
        [HttpPut]
        [Route("api/Profile/ChangePassword")]
        public IHttpActionResult ChangePassword(
    ChangePasswordDto request)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            var userId =
                int.Parse(userIdClaim.Value);

            service.ChangePassword(userId, request);

            return Ok("Password berhasil diubah");
        }


        [Authorize]
        [HttpPut]
        [Route("api/Profile/Image")]
        public IHttpActionResult UpdateProfileImage(
    UploadProfileImageDto request)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            var userId =
                int.Parse(userIdClaim.Value);

            service.UpdateProfileImage(
                userId,
                request.ProfileImage
            );

            return Ok(
                "Foto profile berhasil diupdate"
            );
        }

        [Authorize]
        [HttpPost]
        [Route("api/Profile/UploadImage")]
        public async Task<IHttpActionResult> UploadImage()
        {
            var identity = User.Identity as ClaimsIdentity;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            var userId = int.Parse(userIdClaim.Value);

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Request harus multipart/form-data");

            var uploadFolder =
                System.Web.HttpContext.Current.Server.MapPath("~/Uploads/Profile");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var provider =
                new MultipartFormDataStreamProvider(uploadFolder);

            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FileData == null || provider.FileData.Count == 0)
                return BadRequest("File tidak ditemukan");

            var file = provider.FileData[0];

            var originalName =
                file.Headers.ContentDisposition.FileName.Replace("\"", "");

            var extension =
                Path.GetExtension(originalName);

            var fileName =
                "profile_" + userId + "_" + DateTime.Now.Ticks + extension;

            var newPath =
                Path.Combine(uploadFolder, fileName);

            File.Move(file.LocalFileName, newPath);

            var dbPath =
                "/Uploads/Profile/" + fileName;

            service.UpdateProfileImage(userId, dbPath);

            return Ok(new
            {
                message = "Foto profile berhasil diupload",
                profileImage = dbPath
            });
        }
    
    }
}