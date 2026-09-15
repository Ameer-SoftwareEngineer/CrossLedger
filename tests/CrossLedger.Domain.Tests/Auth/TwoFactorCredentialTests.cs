using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.Auth;

public class TwoFactorCredentialTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_newly_enrolled_credential_is_enabled()
    {
        var credential = new TwoFactorCredential(TwoFactorCredentialId.New(), UserId.New(), "JBSWY3DPEHPK3PXP", Now);

        credential.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Disable_turns_it_off()
    {
        var credential = new TwoFactorCredential(TwoFactorCredentialId.New(), UserId.New(), "JBSWY3DPEHPK3PXP", Now);

        credential.Disable();

        credential.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Constructor_rejects_a_blank_secret()
    {
        var act = () => new TwoFactorCredential(TwoFactorCredentialId.New(), UserId.New(), "  ", Now);

        act.Should().Throw<ArgumentException>();
    }
}
