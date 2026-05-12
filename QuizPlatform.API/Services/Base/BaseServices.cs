using QuizPlatform.API.Services.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace QuizPlatform.API.Services.Base
{
    public class BaseServices
    {
        public QuizDbContext context;

        public BaseServices()
        {
            context = new QuizDbContext();
        }
    }
}