using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrossLedgerWeb.Infrastructure.Identity;

public sealed class StepUpTokenValidator : IStepUpTokenValidator
{
    private readonly IOptions<JwtOptions> _options;

    public StepUpTokenValidator(IOptions<JwtOptions> options)
    {
        _options = options;
    }

    public StepUpTokenValidationResult Validate(string token, StepUpOperation expectedOperation)
    {
        var options = _options.Value;
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        System.Security.Claims.ClaimsPrincipal principal;
        try
        {
            // Without this, JwtSecurityTokenHandler silently renames the "sub" claim to
            // the long ClaimTypes.NameIdentifier URI on the way out, so a literal
            // FindFirst("sub") below would always come back null.
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            principal = handler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception)
        {
            // An expired, malformed, or wrongly-signed token all end up here - callers
            // only need "valid or not", not the specific failure reason.
            return StepUpTokenValidationResult.Invalid;
        }

        var operationClaim = principal.FindFirst(JwtTokenGenerator.StepUpOperationClaimType)?.Value;
        var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (operationClaim is null || subClaim is null)
            return StepUpTokenValidationResult.Invalid;

        if (!Enum.TryParse<StepUpOperation>(operationClaim, out var operation) || operation != expectedOperation)
            return StepUpTokenValidationResult.Invalid;

        if (!Guid.TryParse(subClaim, out var userGuid))
            return StepUpTokenValidationResult.Invalid;

        return StepUpTokenValidationResult.Valid(new UserId(userGuid), operation);
    }
}
