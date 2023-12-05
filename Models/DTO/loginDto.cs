using System;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
	public class loginCredentialsDto
	{
        public string UserNameOrEmail { get; set; } 
        public string Password { get; set; }
    }

    public class loginUserSessionDto
    {
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public JwtUserToken JwtToken { get; set; }
    }
}

