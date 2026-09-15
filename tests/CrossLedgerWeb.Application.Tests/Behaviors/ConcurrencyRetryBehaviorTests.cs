using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Behaviors;
using CrossLedgerWeb.Application.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CrossLedgerWeb.Application.Tests.Behaviors;

public class ConcurrencyRetryBehaviorTests
{
    public sealed record Request(string Value) : IRequest<string>;

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ConcurrencyRetryBehavior<Request, string> CreateBehavior() =>
        new(_unitOfWork.Object, NullLogger<ConcurrencyRetryBehavior<Request, string>>.Instance);

    [Fact]
    public async Task Returns_the_handlers_result_on_the_first_attempt_when_there_is_no_conflict()
    {
        var behavior = CreateBehavior();

        var result = await behavior.Handle(new Request("x"), _ => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        _unitOfWork.Verify(x => x.ClearTrackedChanges(), Times.Never);
    }

    [Fact]
    public async Task Clears_tracked_changes_and_retries_after_a_concurrency_conflict()
    {
        var attempts = 0;
        RequestHandlerDelegate<string> next = _ =>
        {
            attempts++;
            if (attempts == 1)
                throw new ConcurrencyConflictException(new Exception("stale RowVersion"));
            return Task.FromResult("ok-on-retry");
        };
        var behavior = CreateBehavior();

        var result = await behavior.Handle(new Request("x"), next, CancellationToken.None);

        result.Should().Be("ok-on-retry");
        attempts.Should().Be(2);
        _unitOfWork.Verify(x => x.ClearTrackedChanges(), Times.Once);
    }

    [Fact]
    public async Task Gives_up_after_the_maximum_number_of_attempts()
    {
        var attempts = 0;
        RequestHandlerDelegate<string> next = _ =>
        {
            attempts++;
            throw new ConcurrencyConflictException(new Exception("stale RowVersion"));
        };
        var behavior = CreateBehavior();

        var act = () => behavior.Handle(new Request("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        attempts.Should().Be(4);
    }

    [Fact]
    public async Task Does_not_retry_a_failure_that_is_not_a_concurrency_conflict()
    {
        var attempts = 0;
        RequestHandlerDelegate<string> next = _ =>
        {
            attempts++;
            throw new InvalidOperationException("unrelated failure");
        };
        var behavior = CreateBehavior();

        var act = () => behavior.Handle(new Request("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(1);
        _unitOfWork.Verify(x => x.ClearTrackedChanges(), Times.Never);
    }
}
