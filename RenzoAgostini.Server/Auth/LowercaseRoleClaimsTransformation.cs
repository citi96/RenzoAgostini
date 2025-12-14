using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Linq;

namespace RenzoAgostini.Server.Auth;

public class LowercaseRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var modified = false;

        foreach (var identity in principal.Identities)
        {
            var roleClaimType = identity.RoleClaimType;
            var roleClaims = identity.FindAll(roleClaimType).ToList();

            foreach (var roleClaim in roleClaims)
            {
                var normalizedValue = roleClaim.Value.ToLowerInvariant();
                if (roleClaim.Value == normalizedValue)
                {
                    continue;
                }

                identity.RemoveClaim(roleClaim);
                identity.AddClaim(new Claim(roleClaimType, normalizedValue, roleClaim.ValueType, roleClaim.Issuer, roleClaim.OriginalIssuer));
                modified = true;
            }
        }

        return Task.FromResult(modified ? new ClaimsPrincipal(principal.Identities) : principal);
    }
}
