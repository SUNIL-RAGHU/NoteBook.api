using System;
using System.ComponentModel.DataAnnotations;

namespace Notebook.Authentication.Models.DTOS.Incoming
{
	public class TokenRequestDto
	{
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}

