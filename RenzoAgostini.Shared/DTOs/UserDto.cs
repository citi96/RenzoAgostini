using System.Security.Claims;

namespace RenzoAgostini.Shared.DTOs
{
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PictureUri { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];

        public ClaimsPrincipal ToClaimsPrincipal(string accessToken)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Email ?? UserName),
                new(ClaimTypes.Name, UserName ?? string.Empty),
                new(ClaimTypes.Email, Email ?? string.Empty),
                new(ClaimTypes.GivenName, Name ?? string.Empty),
                new(ClaimTypes.Surname, Surname ?? string.Empty),
            };

            if (!string.IsNullOrWhiteSpace(PictureUri))
                claims.Add(new Claim("picture_uri", PictureUri));

            var identity = new ClaimsIdentity(claims, authenticationType: "app-jwt");
            return new ClaimsPrincipal(identity);
        }

        public static UserDto FromClaimsPrincipal(ClaimsPrincipal principal) => new()
        {
            UserName = principal.FindFirst(ClaimTypes.Name)?.Value
                 ?? principal.FindFirst(ClaimTypes.Email)?.Value
                 ?? string.Empty,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            PictureUri = principal.FindFirst("picture_uri")?.Value,
            Name = principal.FindFirst(ClaimTypes.GivenName)?.Value,
            Surname = principal.FindFirst(ClaimTypes.Surname)?.Value,
        };
    }
}