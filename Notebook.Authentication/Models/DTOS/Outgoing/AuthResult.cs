using System;
namespace Notebook.Authentication.Models.DTOS.Outgoing
{
	public class AuthResult
	{

        public string Token { get; set; }

        public bool Success { get; set; }

        public List<string> Errors { get; set; }
    }
}

