using CrossLedgerWeb.Infrastructure.Identity;
using FluentAssertions;
using OtpNet;

namespace CrossLedgerWeb.Integration.Tests.Identity;

public sealed class TotpProviderTests
{
    private readonly TotpProvider _provider = new();

    [Fact]
    public void A_code_generated_from_the_secret_verifies_successfully()
    {
        var secret = _provider.GenerateSecret();
        var code = new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();

        _provider.VerifyCode(secret, code).Should().BeTrue();
    }

    [Fact]
    public void An_arbitrary_six_digit_code_is_rejected()
    {
        var secret = _provider.GenerateSecret();

        _provider.VerifyCode(secret, "000000").Should().BeFalse();
    }

    [Fact]
    public void The_qr_code_uri_carries_the_secret_issuer_and_account_for_authenticator_apps()
    {
        var secret = _provider.GenerateSecret();

        var uri = _provider.GenerateQrCodeUri(secret, "user@example.com", "CrossLedgerWeb");

        uri.Should().StartWith("otpauth://totp/CrossLedgerWeb:user%40example.com");
        uri.Should().Contain($"secret={secret}");
    }
}
