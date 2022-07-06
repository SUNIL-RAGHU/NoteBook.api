using System;
using System.ComponentModel.DataAnnotations;

namespace Notebook.Authentication.Models.DTOS.Incoming
{
	public class UserRegisterationRequestDto
	{
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PassWord { get; set; }
    }
}

