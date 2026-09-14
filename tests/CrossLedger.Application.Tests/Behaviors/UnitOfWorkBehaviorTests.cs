using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Moq;

namespace CrossLedger.Application.Tests.Behaviors;

public class UnitOfWorkBehaviorTests
{
    public sealed record Request(string Value) : IRequest<string>;

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Commits_after_the_handler_succeeds()
    {
        var behavior = new UnitOfWorkBehavior<Request, string>(_unitOfWork.Object);

        var result = await behavior.Handle(new Request("x"), _ => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Never_commits_when_the_handler_throws()
    {
        var behavior = new UnitOfWorkBehavior<Request, string>(_unitOfWork.Object);
        RequestHandlerDelegate<string> next = _ => throw new InvalidOperationException("boom");

        var act = () => behavior.Handle(new Request("x"), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
