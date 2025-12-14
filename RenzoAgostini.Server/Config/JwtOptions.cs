namespace RenzoAgostini.Server.Config;

public class JwtOptions
{
    public string Issuer { get; set; } = "https://localhost:7189";
    public string Audience { get; set; } = "https://localhost:7189";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 30;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
