using CrossLedger.Application.Abstractions;
using MediatR;

namespace CrossLedger.Application.Behaviors;

/// <summary>
/// Commits everything staged during the request in one SaveChanges call, run after
/// the handler and (if it staged a new record) after IdempotencyBehavior - so a
/// transfer's ledger entries and its idempotency record land in the same database
/// transaction (specification 7.1: "persist entries + idempotency record atomically").
/// Must be registered so it wraps ValidationBehavior and IdempotencyBehavior but sits
/// inside LoggingBehavior.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }
}
