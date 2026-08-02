using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    [Authorize]
    public class FriendApiController : ApiController
    {
        private readonly IFriendChatService service =
            new FriendChatService();

        [HttpGet]
        [Route("api/Friend/SearchUsers")]
        public IHttpActionResult SearchUsers(string keyword = "")
        {
            try
            {
                int userId = GetCurrentUserId();

                var result = service.SearchUsers(userId, keyword);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Friend/Add/{receiverId:int}")]
        public IHttpActionResult AddFriend(int receiverId)
        {
            try
            {
                int userId = GetCurrentUserId();

                service.SendFriendRequest(userId, receiverId);

                return Ok("Request pertemanan berhasil dikirim");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/Friend/Requests")]
        public IHttpActionResult GetRequests()
        {
            try
            {
                int userId = GetCurrentUserId();

                var result = service.GetPendingRequests(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Friend/Accept/{requestId:int}")]
        public IHttpActionResult AcceptRequest(int requestId)
        {
            try
            {
                int userId = GetCurrentUserId();

                service.AcceptFriendRequest(userId, requestId);

                return Ok("Request pertemanan diterima");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Friend/Reject/{requestId:int}")]
        public IHttpActionResult RejectRequest(int requestId)
        {
            try
            {
                int userId = GetCurrentUserId();

                service.RejectFriendRequest(userId, requestId);

                return Ok("Request pertemanan ditolak");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/Friend/MyFriends")]
        public IHttpActionResult GetMyFriends()
        {
            try
            {
                int userId = GetCurrentUserId();

                var result = service.GetMyFriends(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
                throw new Exception("Token tidak valid");

            var idClaim =
                identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                identity.FindFirst("UserId")?.Value ??
                identity.FindFirst("userId")?.Value ??
                identity.FindFirst("Id")?.Value ??
                identity.FindFirst("id")?.Value;

            if (!int.TryParse(idClaim, out int userId))
                throw new Exception("UserId tidak ditemukan di token");

            return userId;
        }
    }
}