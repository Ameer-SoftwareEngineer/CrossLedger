using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace CrossLedger.Infrastructure.Identity;

/// <summary>
/// Argon2id password hashing (specification 6.1: "Argon2-class hashing") - ASP.NET Core
/// Identity's built-in hasher uses PBKDF2-HMAC-SHA256 by default, not Argon2, so this
/// replaces it to actually match what the specification claims rather than silently
/// falling short of it. Argon2id is memory-hard, which is specifically what makes it
/// more resistant to GPU/ASIC cracking than PBKDF2.
///
/// The hash string embeds its own parameters (memory/iterations/parallelism), so
/// tightening them later doesn't invalidate passwords hashed under the old policy -
/// VerifyHashedPassword verifies against whatever parameters a given hash recorded, and
/// flags SuccessRehashNeeded when they're weaker than the current constants.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int MemorySizeKb = 19_456; // ~19 MB - OWASP's current Argon2id minimum recommendation
    private const int Iterations = 2;
    private const int Parallelism = 1;

    public string HashPassword(ApplicationUser user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, salt, MemorySizeKb, Iterations, Parallelism, HashSize);

        return string.Join('.', MemorySizeKb, Iterations, Parallelism, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 5
            || !int.TryParse(parts[0], out var memorySizeKb)
            || !int.TryParse(parts[1], out var iterations)
            || !int.TryParse(parts[2], out var parallelism))
        {
            return PasswordVerificationResult.Failed;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expectedHash = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }

        var actualHash = ComputeHash(providedPassword, salt, memorySizeKb, iterations, parallelism, expectedHash.Length);

        if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
            return PasswordVerificationResult.Failed;

        return memorySizeKb == MemorySizeKb && iterations == Iterations && parallelism == Parallelism
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.SuccessRehashNeeded;
    }

    private static byte[] ComputeHash(string password, byte[] salt, int memorySizeKb, int iterations, int parallelism, int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memorySizeKb,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };

        return argon2.GetBytes(hashSize);
    }
}
