using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuizPlatform.API.Controllers
{
    public class UserController : Controller
    {
        private readonly IQuizService service =
            new QuizService();

        public ActionResult Index()
        {
            var users = service.GetAllUser();

            return View(users);
        }
    }
}