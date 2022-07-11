using System;
using System.ComponentModel.DataAnnotations;

namespace Notebook.Authentication.Models.DTOS.Generic
{
	public class TokenData
	{

        
        public string JwtToken { get; set; }

        
        public string RefreshToken { get; set; }
    }
}

