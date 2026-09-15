using CrossLedgerWeb.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace CrossLedgerWeb.Integration.Tests.Identity;

public class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _hasher = new();
    private static readonly ApplicationUser User = new() { UserName = "user@example.com", Email = "user@example.com" };

    [Fact]
    public void The_correct_password_verifies_successfully()
    {
        var hash = _hasher.HashPassword(User, "correct horse battery staple");

        _hasher.VerifyHashedPassword(User, hash, "correct horse battery staple")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void The_wrong_password_fails_verification()
    {
        var hash = _hasher.HashPassword(User, "correct horse battery staple");

        _hasher.VerifyHashedPassword(User, hash, "wrong password")
            .Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void Hashing_the_same_password_twice_produces_different_hashes()
    {
        // A fresh random salt each time - a deterministic hash would mean two users
        // with the same password share an identical row in the database.
        var first = _hasher.HashPassword(User, "same-password");
        var second = _hasher.HashPassword(User, "same-password");

        first.Should().NotBe(second);
    }

    [Fact]
    public void A_malformed_stored_hash_fails_verification_instead_of_throwing()
    {
        var act = () => _hasher.VerifyHashedPassword(User, "not-a-valid-hash-format", "any-password");

        act.Should().NotThrow();
        act().Should().Be(PasswordVerificationResult.Failed);
    }

    [Fact]
    public void Verification_against_a_hash_produced_with_weaker_parameters_requests_a_rehash()
    {
        // Simulates a password hashed under an older, lighter policy (lower memory
        // cost) - the current constants are 19456/2/1, so a hash claiming 8/1/1
        // should verify but signal it wants upgrading.
        const string password = "legacy-password";
        var weakHasher = new LegacyParameterHasherForTest();
        var legacyHash = weakHasher.HashPassword(password);

        _hasher.VerifyHashedPassword(User, legacyHash, password)
            .Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
    }

    /// <summary>Reproduces Argon2PasswordHasher's hash format with deliberately weaker
    /// parameters, to exercise the rehash-needed path without waiting years for the
    /// production constants to actually change.</summary>
    private sealed class LegacyParameterHasherForTest
    {
        public string HashPassword(string password)
        {
            var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
            using var argon2 = new Konscious.Security.Cryptography.Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                MemorySize = 8192,
                Iterations = 1,
                DegreeOfParallelism = 1,
            };
            var hash = argon2.GetBytes(32);
            return string.Join('.', 8192, 1, 1, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }
    }
}
