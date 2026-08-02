using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class ChangePasswordWithCodeDto
    {

        public string Code { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}