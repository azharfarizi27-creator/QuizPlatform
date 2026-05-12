using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using QuizPlatform.API.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using QuizPlatform.API.Models.DTO;

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



    }
}