using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Auth;

public sealed record RequestStepUpTokenCommand(UserId UserId, StepUpOperation Operation, string Code) : IRequest<StepUpTokenResult>;

public sealed record StepUpTokenResult(string StepUpToken, DateTimeOffset ExpiresAt);
