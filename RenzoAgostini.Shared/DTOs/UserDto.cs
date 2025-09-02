using System.Security.Claims;

namespace RenzoAgostini.Shared.DTOs
{
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PictureUri { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];

        public ClaimsPrincipal ToClaimsPrincipal() => new(new ClaimsIdentity([
           new (ClaimTypes.Name, UserName),
            new (ClaimTypes.Hash, PasswordHash ?? string.Empty),
            new (ClaimTypes.Email, Email),
            new ("picture_uri", PictureUri ?? string.Empty),
            new (ClaimTypes.GivenName, Name ?? string.Empty),
            new (ClaimTypes.Surname, Surname ?? string.Empty),
       ]));

        public static UserDto FromClaimsPrincipal(ClaimsPrincipal principal) => new()
        {
            UserName = principal.FindFirst(ClaimTypes.Name)?.Value ?? throw new NullReferenceException("No Name Claim for the provided token"),
            PasswordHash = principal.FindFirst(ClaimTypes.Hash)?.Value,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException("No Email Claim for the provided token"),
            PictureUri = principal.FindFirst("picture_uri")?.Value,
            Name = principal.FindFirst(ClaimTypes.GivenName)?.Value,
            Surname = principal.FindFirst(ClaimTypes.Surname)?.Value,
        };
    }
}