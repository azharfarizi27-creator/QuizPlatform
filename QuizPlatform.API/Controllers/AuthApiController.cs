using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Models.ViewModel;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class AuthApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();


        [HttpPost]
        [Route("api/auth/login")]
        public IHttpActionResult Login(LoginDto dto)
        {
            var result = service.Login(dto.Username, dto.Password);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [HttpPost]
        [Route("api/Auth/RegisterStudent")]
        public IHttpActionResult RegisterStudent([FromBody] User user)
        {
            if (user == null)
                return BadRequest("Data kosong");

            user.RoleId = 3; // Student
            user.IsActive = true;

            service.CreateUser(user);

            return Ok("Register student berhasil");
        }   



    }
}