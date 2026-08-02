using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    [Authorize]
    public class ChatApiController : ApiController
    {
        private readonly IFriendChatService service =
            new FriendChatService();

        [HttpGet]
        [Route("api/Chat/Conversation/{friendId:int}")]
        public IHttpActionResult GetConversation(int friendId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var result = service.GetConversation(userId, friendId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Chat/Send")]
        public IHttpActionResult SendMessage(SendChatMessageDto request)
        {
            try
            {
                int userId = GetCurrentUserId();

                var result = service.SendMessage(userId, request);

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