using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Auth;

/// <summary>Disabling two-factor auth is itself step-up-gated (specification 6.2:
/// "Protects the protection itself") - enforced by [RequireStepUp] on the endpoint, not
/// by this command, which only does the actual disabling once that gate has passed.</summary>
public sealed record DisableTwoFactorCommand(UserId UserId) : IRequest;
