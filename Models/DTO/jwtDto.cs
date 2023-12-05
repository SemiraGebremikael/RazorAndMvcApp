using System;
namespace Models.DTO
{
    public class JwtUserToken
    {
        public Guid TokenId { get; set; }

        public string EncryptedToken { get; set; }
        public DateTime ExpireTime { get; set; }

        //This will be the User part of the Claim, which can later be retrieved
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
    }
}

