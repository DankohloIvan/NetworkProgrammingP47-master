using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkProgrammingP47.Models
{
    internal class UserSignupModel
    {
        public String Email { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String Password { get; set; } = null!;
        public String ConfirmCode { get; set; } = null!;
    }
}
