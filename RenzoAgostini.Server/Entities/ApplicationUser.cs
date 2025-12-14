using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace RenzoAgostini.Server.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? PictureUri { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public static ApplicationUser? FromGoogleJwt(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.CanReadToken(token))
            {
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

                return new()
                {
                    Id = jwtSecurityToken.Subject,
                    UserName = jwtSecurityToken.Claims.First(c => c.Type == "preferred_username").Value,
                    Email = jwtSecurityToken.Claims.First(c => c.Type == "email").Value,
                    Name = jwtSecurityToken.Claims.First(c => c.Type == "given_name").Value,
                    Surname = jwtSecurityToken.Claims.First(c => c.Type == "family_name").Value
                };
            }

            return null;
        }
    }
}