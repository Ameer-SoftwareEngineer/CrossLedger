using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrossLedgerWeb.Infrastructure.Identity;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string StepUpOperationClaimType = "step_up_operation";

    private static readonly TimeSpan StepUpTokenLifetime = TimeSpan.FromMinutes(5);

    private readonly IOptions<JwtOptions> _options;
    private readonly IClock _clock;

    public JwtTokenGenerator(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options;
        _clock = clock;
    }

    public AccessToken GenerateAccessToken(UserId userId, string email, IReadOnlyList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, email),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return BuildToken(userId, claims, TimeSpan.FromMinutes(_options.Value.AccessTokenLifetimeMinutes));
    }

    public AccessToken GenerateStepUpToken(UserId userId, StepUpOperation operation)
    {
        var claims = new List<Claim>
        {
            new(StepUpOperationClaimType, operation.ToString()),
        };

        return BuildToken(userId, claims, StepUpTokenLifetime);
    }

    private AccessToken BuildToken(UserId userId, IReadOnlyList<Claim> additionalClaims, TimeSpan lifetime)
    {
        var options = _options.Value;
        var now = _clock.UtcNow;
        var expiresAt = now + lifetime;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(additionalClaims);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(value, expiresAt);
    }
}
