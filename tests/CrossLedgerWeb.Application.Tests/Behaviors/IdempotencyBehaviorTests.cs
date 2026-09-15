using System.Text.Json;
using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Behaviors;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Moq;

namespace CrossLedgerWeb.Application.Tests.Behaviors;

public class IdempotencyBehaviorTests
{
    public sealed record PlainRequest(string Value) : IRequest<string>;

    public sealed record IdempotentRequest(string IdempotencyKey, string Value) : IRequest<string>, IIdempotentRequest;

    public sealed record MoneyRequest(string IdempotencyKey) : IRequest<Money>, IIdempotentRequest;

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

    // Regression test: Currency's constructor is private (only Currency.From
    // validates and builds one), so System.Text.Json's default reflection-based
    // materialization silently produces a default(Currency) with a null Code instead
    // of throwing - discovered by replaying an idempotent transfer against a real
    // running API and getting back correct amounts but null currency codes.
    [Fact]
    public async Task A_replayed_response_containing_a_currency_round_trips_its_code_correctly()
    {
        var original = new Money(86.14m, Currency.From("EUR"));
        _store.Setup(x => x.FindResponseAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var behavior = new IdempotencyBehavior<MoneyRequest, Money>(_store.Object);
        string? captured = null;
        _store.Setup(x => x.SaveResponseAsync("key-1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, payload, _) => captured = payload);

        await behavior.Handle(new MoneyRequest("key-1"), _ => Task.FromResult(original), CancellationToken.None);

        captured.Should().NotBeNull();
        _store.Setup(x => x.FindResponseAsync("key-1", It.IsAny<CancellationToken>())).ReturnsAsync(captured);

        var replayed = await behavior.Handle(
            new MoneyRequest("key-1"), _ => throw new InvalidOperationException("must not re-run"), CancellationToken.None);

        replayed.Should().Be(original);
        replayed.Currency.Code.Should().Be("EUR");
    }
}
