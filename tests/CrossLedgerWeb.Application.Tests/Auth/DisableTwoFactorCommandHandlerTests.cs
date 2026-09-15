using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Auth;
using CrossLedgerWeb.Application.Exceptions;
using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace CrossLedgerWeb.Application.Tests.Auth;

public class DisableTwoFactorCommandHandlerTests
{
    private readonly Mock<ITwoFactorCredentialRepository> _credentials = new();

    [Fact]
    public async Task Handle_disables_an_existing_credential()
    {
        var credential = new TwoFactorCredential(TwoFactorCredentialId.New(), UserId.New(), "SECRET", DateTimeOffset.UtcNow);
        _credentials.Setup(x => x.GetByUserIdAsync(credential.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(credential);
        var handler = new DisableTwoFactorCommandHandler(_credentials.Object);

        await handler.Handle(new DisableTwoFactorCommand(credential.UserId), CancellationToken.None);

        credential.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_throws_when_no_credential_exists()
    {
        _credentials.Setup(x => x.GetByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync((TwoFactorCredential?)null);
        var handler = new DisableTwoFactorCommandHandler(_credentials.Object);

        var act = () => handler.Handle(new DisableTwoFactorCommand(UserId.New()), CancellationToken.None);

        await act.Should().ThrowAsync<TwoFactorNotEnabledException>();
    }
}
