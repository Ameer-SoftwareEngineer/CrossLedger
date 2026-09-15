using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.Exceptions;
using CrossLedger.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CrossLedger.Api.ExceptionHandling;

/// <summary>
/// Maps domain and application exceptions to the HTTP status codes the specification
/// calls for - e.g. QuoteExpiredException -> 409 QUOTE_EXPIRED (2.2), InsufficientFunds
/// -> a clean 409 rather than a raw 500 (2.4) - so a handler never needs its own
/// try/catch just to shape a response.
/// </summary>
public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = new Dictionary<string, string[]>(validationException.Errors);
            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

        var (statusCode, title, code) = exception switch
        {
            WalletNotFoundException => (StatusCodes.Status404NotFound, "Wallet not found", "WALLET_NOT_FOUND"),
            QuoteNotFoundException => (StatusCodes.Status404NotFound, "Quote not found", "QUOTE_NOT_FOUND"),
            QuoteExpiredException => (StatusCodes.Status409Conflict, "Quote expired", "QUOTE_EXPIRED"),
            InsufficientFundsException => (StatusCodes.Status409Conflict, "Insufficient funds", "INSUFFICIENT_FUNDS"),
            CurrencyMismatchException => (StatusCodes.Status400BadRequest, "Currency mismatch", "CURRENCY_MISMATCH"),
            ExchangeRateUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Exchange rate unavailable", "RATE_UNAVAILABLE"),
            SettlementWalletNotConfiguredException => (StatusCodes.Status500InternalServerError, "Settlement wallet not configured", "SETTLEMENT_WALLET_MISSING"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", "INVALID_ARGUMENT"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "UNEXPECTED_ERROR"),
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Extensions = { ["code"] = code },
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
