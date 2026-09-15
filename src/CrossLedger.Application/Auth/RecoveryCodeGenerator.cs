using System.Security.Cryptography;
using System.Text;

namespace CrossLedger.Application.Auth;

/// <summary>Generates and hashes the ten single-use recovery codes issued at TOTP
/// enrolment (specification 6.1). Like RefreshTokenGenerator, only the hash is ever
/// persisted - these are shown to the user exactly once, at issuance.</summary>
public static class RecoveryCodeGenerator
{
    // Excludes 0/O and 1/I - both visually ambiguous when a user is transcribing a
    // printed code by hand. 32 characters divides 256 evenly, so mapping a random byte
    // via modulo introduces no bias toward any particular character.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(10);
        var chars = new char[10];
        for (var i = 0; i < bytes.Length; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];

        return $"{new string(chars, 0, 5)}-{new string(chars, 5, 5)}";
    }

    public static string Hash(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code.ToUpperInvariant()))).ToLowerInvariant();
}
