using System.Collections.Generic;
using System.Security.Claims;

namespace Webclient.Models
{
    public class UserIdentity : ClaimsIdentity
    {
        public UserIdentity(User user) : base(CreateClaims(user), "Custom")
        {
            UserId = user.UserId;
            Username = user.Username;
            Email = user.Email;
            Role = user.Role;
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        private static IEnumerable<Claim> CreateClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            return claims;
        }
    }
}
