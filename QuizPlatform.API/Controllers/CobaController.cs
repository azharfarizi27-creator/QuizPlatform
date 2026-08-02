using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace QuizPlatform.API.Controllers
{
    public class CobaController : Controller
    {
        public ActionResult Index()
        {
           
            ViewBag.Title = "Home Page";

            return View();
        }

    }
}