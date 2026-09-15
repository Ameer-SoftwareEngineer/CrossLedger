using CrossLedgerWeb.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using ValidationException = CrossLedgerWeb.Application.Exceptions.ValidationException;

namespace CrossLedgerWeb.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record Request(string Value) : IRequest<string>;

    private static RequestHandlerDelegate<string> Next(string response) => _ => Task.FromResult(response);

    [Fact]
    public async Task No_registered_validators_calls_next()
    {
        var behavior = new ValidationBehavior<Request, string>(Array.Empty<IValidator<Request>>());

        var result = await behavior.Handle(new Request("x"), Next("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task A_passing_validator_calls_next()
    {
        var validator = new Mock<IValidator<Request>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var behavior = new ValidationBehavior<Request, string>(new[] { validator.Object });

        var result = await behavior.Handle(new Request("x"), Next("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task A_failing_validator_throws_instead_of_calling_next()
    {
        var validator = new Mock<IValidator<Request>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Value", "must not be empty") }));
        var behavior = new ValidationBehavior<Request, string>(new[] { validator.Object });
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ => { nextCalled = true; return Task.FromResult("ok"); };

        var act = () => behavior.Handle(new Request(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}
