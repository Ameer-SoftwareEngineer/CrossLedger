using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.Auth;

public class UsedTotpCodeTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsActive_is_true_before_expiry()
    {
        var used = new UsedTotpCode(UsedTotpCodeId.New(), UserId.New(), "123456", Now, Now.AddSeconds(90));

        used.IsActive(Now.AddSeconds(30)).Should().BeTrue();
    }

    [Fact]
    public void IsActive_is_false_once_the_window_has_passed()
    {
        var used = new UsedTotpCode(UsedTotpCodeId.New(), UserId.New(), "123456", Now, Now.AddSeconds(90));

        used.IsActive(Now.AddSeconds(91)).Should().BeFalse();
    }

    [Fact]
    public void Constructor_rejects_a_blank_code()
    {
        var act = () => new UsedTotpCode(UsedTotpCodeId.New(), UserId.New(), "", Now, Now.AddSeconds(90));

        act.Should().Throw<ArgumentException>();
    }
}
