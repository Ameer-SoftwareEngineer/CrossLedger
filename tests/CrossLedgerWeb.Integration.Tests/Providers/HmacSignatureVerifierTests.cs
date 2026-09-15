using CrossLedgerWeb.Providers.Common;
using FluentAssertions;

namespace CrossLedgerWeb.Integration.Tests.Providers;

public class HmacSignatureVerifierTests
{
    [Fact]
    public void A_signature_produced_by_sign_verifies_successfully()
    {
        var signature = HmacSignatureVerifier.Sign("{\"event\":\"payout.completed\"}", "webhook-secret");

        HmacSignatureVerifier.Verify("{\"event\":\"payout.completed\"}", signature, "webhook-secret").Should().BeTrue();
    }

    [Fact]
    public void A_tampered_payload_fails_verification()
    {
        var signature = HmacSignatureVerifier.Sign("{\"amount\":100}", "webhook-secret");

        HmacSignatureVerifier.Verify("{\"amount\":999}", signature, "webhook-secret").Should().BeFalse();
    }

    [Fact]
    public void The_wrong_secret_fails_verification()
    {
        var signature = HmacSignatureVerifier.Sign("payload", "correct-secret");

        HmacSignatureVerifier.Verify("payload", signature, "wrong-secret").Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-valid-hex")]
    public void A_malformed_signature_fails_verification_instead_of_throwing(string malformed)
    {
        var act = () => HmacSignatureVerifier.Verify("payload", malformed, "secret");

        act.Should().NotThrow();
        act().Should().BeFalse();
    }
}
