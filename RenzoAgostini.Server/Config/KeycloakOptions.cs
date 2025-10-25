using System.Collections.Generic;

namespace RenzoAgostini.Server.Config;

public class KeycloakOptions
{
    public string? Authority { get; set; }
    public string? Realm { get; set; }
    public string? ClientId { get; set; }
    public List<string> Audiences { get; set; } = [];
    public bool RequireHttpsMetadata { get; set; } = true;
}
