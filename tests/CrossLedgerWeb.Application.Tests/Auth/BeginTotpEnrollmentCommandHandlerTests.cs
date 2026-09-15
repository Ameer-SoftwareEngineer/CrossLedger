using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace CrossLedgerWeb.Application.Tests.Auth;

public class BeginTotpEnrollmentCommandHandlerTests
{
    private readonly Mock<ITotpProvider> _totp = new();

    [Fact]
    public async Task Handle_returns_a_secret_and_qr_uri_without_persisting_anything()
    {
        _totp.Setup(x => x.GenerateSecret()).Returns("JBSWY3DPEHPK3PXP");
        _totp.Setup(x => x.GenerateQrCodeUri("JBSWY3DPEHPK3PXP", "user@example.com", "CrossLedgerWeb"))
            .Returns("otpauth://totp/CrossLedgerWeb:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=CrossLedgerWeb");
        var handler = new BeginTotpEnrollmentCommandHandler(_totp.Object);

        var result = await handler.Handle(new BeginTotpEnrollmentCommand(UserId.New(), "user@example.com"), CancellationToken.None);

        result.Secret.Should().Be("JBSWY3DPEHPK3PXP");
        result.QrCodeUri.Should().StartWith("otpauth://");
    }
}
