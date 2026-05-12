using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Web.Mvc;

namespace QuizPlatform.API.Controllers
{
    public class TestController : Controller
    {
        private readonly IQuizService service = new QuizService();

        public ActionResult Index()
        {
            var result = service.CekKoneksiDB();

            return Content(result);
        }
    }
}