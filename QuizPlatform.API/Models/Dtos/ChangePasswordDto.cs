using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class ChangePasswordDto
    {

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}