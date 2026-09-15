using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Api.Security;

internal static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the "sub" claim IJwtTokenGenerator writes into every access and
    /// step-up token. Requires JwtBearerOptions.MapInboundClaims = false (Program.cs) -
    /// otherwise the framework silently renames "sub" to the long ClaimTypes.NameIdentifier
    /// URI before this ever sees it.</summary>
    public static bool TryGetUserId(this ClaimsPrincipal principal, out UserId userId)
    {
        var claim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (claim is not null && Guid.TryParse(claim, out var value))
        {
            userId = new UserId(value);
            return true;
        }

        userId = default;
        return false;
    }

    public static string? GetEmail(this ClaimsPrincipal principal) =>
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
}
