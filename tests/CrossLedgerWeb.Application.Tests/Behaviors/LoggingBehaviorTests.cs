using CrossLedgerWeb.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace CrossLedgerWeb.Application.Tests.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record Request(string Value) : IRequest<string>;

    private readonly LoggingBehavior<Request, string> _behavior = new(NullLogger<LoggingBehavior<Request, string>>.Instance);

    [Fact]
    public async Task Returns_the_handlers_response_unchanged()
    {
        var result = await _behavior.Handle(new Request("x"), _ => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Propagates_an_exception_from_the_handler()
    {
        RequestHandlerDelegate<string> next = _ => throw new InvalidOperationException("boom");

        var act = () => _behavior.Handle(new Request("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
