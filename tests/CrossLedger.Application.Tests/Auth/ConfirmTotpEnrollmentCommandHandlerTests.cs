using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Auth;
using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace CrossLedger.Application.Tests.Auth;

public class ConfirmTotpEnrollmentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly UserId User = UserId.New();

    private readonly Mock<ITotpProvider> _totp = new();
    private readonly Mock<ITwoFactorCredentialRepository> _credentials = new();
    private readonly Mock<IRecoveryCodeRepository> _recoveryCodes = new();
    private readonly Mock<IClock> _clock = new();

    public ConfirmTotpEnrollmentCommandHandlerTests()
    {
        _clock.Setup(x => x.UtcNow).Returns(Now);
    }

    private ConfirmTotpEnrollmentCommandHandler CreateHandler() =>
        new(_totp.Object, _credentials.Object, _recoveryCodes.Object, _clock.Object);

    [Fact]
    public async Task Handle_enrolls_the_credential_and_returns_ten_recovery_codes_when_the_code_verifies()
    {
        _totp.Setup(x => x.VerifyCode("SECRET123", "123456")).Returns(true);
        var handler = CreateHandler();

        var result = await handler.Handle(new ConfirmTotpEnrollmentCommand(User, "SECRET123", "123456"), CancellationToken.None);

        result.RecoveryCodes.Should().HaveCount(10);
        result.RecoveryCodes.Should().OnlyHaveUniqueItems();
        _credentials.Verify(x => x.Add(It.Is<TwoFactorCredential>(c => c.UserId == User && c.Secret == "SECRET123" && c.IsEnabled)), Times.Once);
        _recoveryCodes.Verify(x => x.AddRange(It.Is<IEnumerable<RecoveryCode>>(codes => codes.Count() == 10)), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_and_persists_nothing_when_the_code_does_not_verify()
    {
        _totp.Setup(x => x.VerifyCode(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var handler = CreateHandler();

        var act = () => handler.Handle(new ConfirmTotpEnrollmentCommand(User, "SECRET123", "000000"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidTotpCodeException>();
        _credentials.Verify(x => x.Add(It.IsAny<TwoFactorCredential>()), Times.Never);
        _recoveryCodes.Verify(x => x.AddRange(It.IsAny<IEnumerable<RecoveryCode>>()), Times.Never);
    }
}
