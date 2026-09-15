using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.Auth;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static RefreshToken NewToken(TimeSpan? validFor = null) => new(
        RefreshTokenId.New(), UserId.New(), "hash", Guid.NewGuid(), Now, validFor ?? TimeSpan.FromDays(30));

    [Fact]
    public void A_freshly_issued_token_is_active()
    {
        var token = NewToken();

        token.IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public void A_token_is_expired_once_its_validity_window_has_passed()
    {
        var token = NewToken(TimeSpan.FromDays(30));

        token.IsExpired(Now.AddDays(31)).Should().BeTrue();
        token.IsActive(Now.AddDays(31)).Should().BeFalse();
    }

    [Fact]
    public void Revoke_marks_the_token_inactive()
    {
        var token = NewToken();

        token.Revoke(Now);

        token.IsRevoked.Should().BeTrue();
        token.IsActive(Now).Should().BeFalse();
    }

    [Fact]
    public void Revoke_is_idempotent_and_keeps_the_first_revocation()
    {
        var token = NewToken();

        token.Revoke(Now);
        token.Revoke(Now.AddMinutes(5));

        token.RevokedAt.Should().Be(Now);
    }

    [Fact]
    public void Constructor_rejects_a_non_positive_validity_window()
    {
        var act = () => new RefreshToken(RefreshTokenId.New(), UserId.New(), "hash", Guid.NewGuid(), Now, TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_rejects_a_blank_token_hash()
    {
        var act = () => new RefreshToken(RefreshTokenId.New(), UserId.New(), "  ", Guid.NewGuid(), Now, TimeSpan.FromDays(1));

        act.Should().Throw<ArgumentException>();
    }
}
