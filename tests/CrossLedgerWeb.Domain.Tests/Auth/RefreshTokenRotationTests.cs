using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedgerWeb.Domain.Tests.Auth;

public class RefreshTokenRotationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly UserId User = UserId.New();
    private static readonly Guid FamilyId = Guid.NewGuid();

    private static RefreshToken IssuedToken(TimeSpan? validFor = null) => new(
        RefreshTokenId.New(), User, "original-hash", FamilyId, Now, validFor ?? TimeSpan.FromDays(30));

    [Fact]
    public void Rotating_an_active_token_revokes_it_and_returns_a_new_one_in_the_same_family()
    {
        var presented = IssuedToken();

        var next = RefreshTokenRotation.Rotate(
            presented, [presented], Now, RefreshTokenId.New(), "next-hash", TimeSpan.FromDays(30));

        presented.IsRevoked.Should().BeTrue();
        presented.ReplacedByTokenId.Should().Be(next.Id);
        next.FamilyId.Should().Be(presented.FamilyId);
        next.UserId.Should().Be(User);
        next.IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public void Rotating_an_expired_token_throws_and_does_not_revoke_it()
    {
        var presented = IssuedToken(TimeSpan.FromDays(1));
        var afterExpiry = Now.AddDays(2);

        var act = () => RefreshTokenRotation.Rotate(
            presented, [presented], afterExpiry, RefreshTokenId.New(), "next-hash", TimeSpan.FromDays(30));

        act.Should().Throw<RefreshTokenExpiredException>();
        presented.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Presenting_an_already_revoked_token_revokes_every_other_active_token_in_the_family()
    {
        var reused = IssuedToken();
        reused.Revoke(Now); // simulates: this token was already rotated away once before
        var stillActiveSibling = IssuedToken();

        var act = () => RefreshTokenRotation.Rotate(
            reused, [reused, stillActiveSibling], Now.AddMinutes(1), RefreshTokenId.New(), "attacker-hash", TimeSpan.FromDays(30));

        act.Should().Throw<RefreshTokenReuseDetectedException>()
            .Which.FamilyId.Should().Be(FamilyId);
        stillActiveSibling.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Reuse_detection_does_not_touch_tokens_outside_the_supplied_family()
    {
        var reused = IssuedToken();
        reused.Revoke(Now);
        var unrelatedToken = new RefreshToken(RefreshTokenId.New(), UserId.New(), "hash", Guid.NewGuid(), Now, TimeSpan.FromDays(30));

        try
        {
            RefreshTokenRotation.Rotate(reused, [reused], Now.AddMinutes(1), RefreshTokenId.New(), "hash2", TimeSpan.FromDays(30));
        }
        catch (RefreshTokenReuseDetectedException)
        {
            // expected
        }

        unrelatedToken.IsRevoked.Should().BeFalse();
    }
}
