namespace CrossLedger.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "CrossLedger";
    public string Audience { get; set; } = "CrossLedger";

    /// <summary>Never commit a real value - set via user-secrets locally or Key Vault
    /// in production (specification 3.4). Must be at least 32 bytes for HS256.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 15;
}
