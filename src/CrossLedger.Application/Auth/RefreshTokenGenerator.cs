using System.Security.Cryptography;
using System.Text;

namespace CrossLedger.Application.Auth;

/// <summary>
/// Refresh tokens are high-entropy random values, not user-chosen secrets, so a fast
/// cryptographic hash (SHA-256) is appropriate here - unlike passwords, which need a
/// slow, memory-hard hash (see Argon2PasswordHasher) specifically because they're
/// low-entropy and guessable. Only the hash is ever persisted (specification 6.4).
/// </summary>
public static class RefreshTokenGenerator
{
    public static string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
}
