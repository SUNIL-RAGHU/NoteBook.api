using System;
using System.ComponentModel.DataAnnotations;

namespace Notebook.Authentication.Models.DTOS.Incoming
{
	public class UserLoginRequestDto
	{
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}

