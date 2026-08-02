using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class VerifyRegisterCodeDto
    {

        public string Email { get; set; }

        public string Code { get; set; }
    }
}