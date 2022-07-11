using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Notebook.Entities.DbSet
{
    public class RefreshToken : BaseEntity
    {

        public string UserId { get; set; }

        public string Token { get; set; }

        public string JwtId { get; set; }

        public bool IsUsed { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime ExpiryDate { get; set; }



        [ForeignKey(nameof(UserId))]

        public IdentityUser User { get; set; }

        public string ToLower()
        {
            throw new NotImplementedException();
        }
    }
}

