using System.Security.Cryptography;
using System.Text;

namespace CrossLedger.Providers.Common;

/// <summary>
/// HMAC-SHA256 payload signing/verification shared by every provider (specification 3.4)
/// - the algorithm is identical across Airwallex, Rapyd, Stripe and the simulated
/// provider; only header names and encoding conventions differ per provider. Uses
/// FixedTimeEquals so signature comparison doesn't leak timing information a real
/// attacker could exploit to forge a valid signature byte by byte.
/// </summary>
public static class HmacSignatureVerifier
{
    public static bool Verify(string payload, string signatureHex, string secret)
    {
        if (string.IsNullOrEmpty(signatureHex))
            return false;

        byte[] signatureBytes;
        try
        {
            signatureBytes = Convert.FromHexString(signatureHex);
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = ComputeHash(payload, secret);
        return computed.Length == signatureBytes.Length && CryptographicOperations.FixedTimeEquals(computed, signatureBytes);
    }

    public static string Sign(string payload, string secret) =>
        Convert.ToHexString(ComputeHash(payload, secret)).ToLowerInvariant();

    private static byte[] ComputeHash(string payload, string secret) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
}
