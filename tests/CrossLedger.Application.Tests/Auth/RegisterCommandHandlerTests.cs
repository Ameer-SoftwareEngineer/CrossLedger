using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Auth;
using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace CrossLedger.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identity = new();

    [Fact]
    public async Task Handle_returns_the_new_user_id_on_success()
    {
        var userId = UserId.New();
        _identity.Setup(x => x.RegisterAsync("user@example.com", "password123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(RegistrationOutcome.Success(userId));
        var handler = new RegisterCommandHandler(_identity.Object);

        var result = await handler.Handle(new RegisterCommand("user@example.com", "password123"), CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Handle_throws_when_registration_fails()
    {
        _identity.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RegistrationOutcome.Failure(["Email already taken"]));
        var handler = new RegisterCommandHandler(_identity.Object);

        var act = () => handler.Handle(new RegisterCommand("user@example.com", "password123"), CancellationToken.None);

        await act.Should().ThrowAsync<RegistrationFailedException>();
    }
}
