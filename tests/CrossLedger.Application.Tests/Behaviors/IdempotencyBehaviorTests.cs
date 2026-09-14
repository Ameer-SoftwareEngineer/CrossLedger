using System.Text.Json;
using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Moq;

namespace CrossLedger.Application.Tests.Behaviors;

public class IdempotencyBehaviorTests
{
    public sealed record PlainRequest(string Value) : IRequest<string>;

    public sealed record IdempotentRequest(string IdempotencyKey, string Value) : IRequest<string>, IIdempotentRequest;

    private readonly Mock<IIdempotencyStore> _store = new();

    [Fact]
    public async Task A_non_idempotent_request_never_touches_the_store()
    {
        var behavior = new IdempotencyBehavior<PlainRequest, string>(_store.Object);

        var result = await behavior.Handle(new PlainRequest("x"), _ => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        _store.Verify(x => x.FindResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_new_idempotency_key_calls_next_and_stores_the_serialized_response()
    {
        _store.Setup(x => x.FindResponseAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var behavior = new IdempotencyBehavior<IdempotentRequest, string>(_store.Object);
        var nextCalled = false;

        var result = await behavior.Handle(
            new IdempotentRequest("key-1", "x"),
            _ => { nextCalled = true; return Task.FromResult("fresh-response"); },
            CancellationToken.None);

        result.Should().Be("fresh-response");
        nextCalled.Should().BeTrue();
        var expectedSerialized = JsonSerializer.Serialize("fresh-response");
        _store.Verify(x => x.SaveResponseAsync("key-1", expectedSerialized, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_repeated_idempotency_key_returns_the_stored_response_without_calling_next()
    {
        var stored = JsonSerializer.Serialize("stored-response");
        _store.Setup(x => x.FindResponseAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        var behavior = new IdempotencyBehavior<IdempotentRequest, string>(_store.Object);
        var nextCalled = false;

        var result = await behavior.Handle(
            new IdempotentRequest("key-1", "x"),
            _ => { nextCalled = true; return Task.FromResult("should-not-run"); },
            CancellationToken.None);

        result.Should().Be("stored-response");
        nextCalled.Should().BeFalse();
        _store.Verify(x => x.SaveResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
