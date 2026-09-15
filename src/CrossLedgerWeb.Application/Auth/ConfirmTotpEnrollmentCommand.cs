using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Auth;

public sealed record ConfirmTotpEnrollmentCommand(UserId UserId, string Secret, string Code) : IRequest<ConfirmTotpEnrollmentResult>;

public sealed record ConfirmTotpEnrollmentResult(IReadOnlyList<string> RecoveryCodes);
