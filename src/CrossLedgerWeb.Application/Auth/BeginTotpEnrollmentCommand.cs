using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Auth;

public sealed record BeginTotpEnrollmentCommand(UserId UserId, string Email) : IRequest<BeginTotpEnrollmentResult>;

public sealed record BeginTotpEnrollmentResult(string Secret, string QrCodeUri);
