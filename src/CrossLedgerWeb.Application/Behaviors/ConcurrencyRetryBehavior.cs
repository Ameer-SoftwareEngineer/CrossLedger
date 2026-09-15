using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CrossLedgerWeb.Application.Behaviors;

/// <summary>
/// "Two transfers hit the same wallet simultaneously - what happens? EF Core RowVersion
/// optimistic concurrency. The loser gets a concurrency exception and is retried with
/// backoff; insufficient funds then fails cleanly." (specification 11, "Interview
/// Defence"). Wraps UnitOfWorkBehavior (registered before it, so it wraps everything
/// downstream including the handler) rather than sitting inside it: a stale RowVersion
/// means the handler's in-memory changes were computed against data that's no longer
/// current, so retrying SaveChanges alone would just fail the same way again - the whole
/// handler has to re-run against a freshly reloaded aggregate, which ClearTrackedChanges
/// forces on the next repository read.
/// </summary>
public sealed class ConcurrencyRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int MaxAttempts = 4;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConcurrencyRetryBehavior<TRequest, TResponse>> _logger;

    public ConcurrencyRetryBehavior(IUnitOfWork unitOfWork, ILogger<ConcurrencyRetryBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    "Concurrency conflict on {RequestName}, attempt {Attempt} of {MaxAttempts} - retrying",
                    typeof(TRequest).Name, attempt, MaxAttempts);

                _unitOfWork.ClearTrackedChanges();

                var backoff = TimeSpan.FromMilliseconds(25 * Math.Pow(2, attempt - 1));
                await Task.Delay(backoff, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
