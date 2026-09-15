using CrossLedger.Api.Security;
using CrossLedger.Application.Auth;
using CrossLedger.Shared.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CrossLedger.Api.Controllers;

/// <summary>TOTP enrolment and step-up (specification 6). Any authenticated user can
/// manage their own two-factor credential regardless of role - this isn't a
/// Customer-only feature, so there's no [Authorize(Roles = ...)] restriction beyond
/// requiring a valid access token.</summary>
[ApiController]
[Route("api/v1/auth/2fa")]
[Authorize]
public sealed class TwoFactorController : ControllerBase
{
    private readonly IMediator _mediator;

    public TwoFactorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Persists nothing - the client holds the returned secret until Confirm
    /// proves it can generate a valid code from it (enrolment integrity, 6.1).</summary>
    [HttpPost("enroll/begin")]
    [ProducesResponseType<BeginTotpEnrollmentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BeginTotpEnrollmentResponse>> BeginEnrollment(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var email = User.GetEmail();
        if (email is null)
            return Unauthorized();

        var result = await _mediator.Send(new BeginTotpEnrollmentCommand(userId, email), cancellationToken);

        return Ok(new BeginTotpEnrollmentResponse(result.Secret, result.QrCodeUri));
    }

    [HttpPost("enroll/confirm")]
    [EnableRateLimiting(RateLimiterPolicies.TotpVerification)]
    [ProducesResponseType<ConfirmTotpEnrollmentResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfirmTotpEnrollmentResponse>> ConfirmEnrollment(
        ConfirmTotpEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _mediator.Send(
            new ConfirmTotpEnrollmentCommand(userId, request.Secret, request.Code), cancellationToken);

        return Ok(new ConfirmTotpEnrollmentResponse(result.RecoveryCodes));
    }

    /// <summary>Issues the short-lived, operation-scoped token that [RequireStepUp]
    /// checks for on protected endpoints (specification 6.3).</summary>
    [HttpPost("step-up")]
    [EnableRateLimiting(RateLimiterPolicies.TotpVerification)]
    [ProducesResponseType<StepUpTokenResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StepUpTokenResponse>> RequestStepUp(
        RequestStepUpTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        if (!Enum.TryParse<StepUpOperation>(request.Operation, ignoreCase: true, out var operation))
        {
            return BadRequest(new { code = "INVALID_OPERATION", message = $"Unknown step-up operation '{request.Operation}'." });
        }

        var result = await _mediator.Send(new RequestStepUpTokenCommand(userId, operation, request.Code), cancellationToken);

        return Ok(new StepUpTokenResponse(result.StepUpToken, result.ExpiresAt));
    }

    /// <summary>"Protects the protection itself" (specification 6.2) - disabling 2FA is
    /// itself step-up-gated.</summary>
    [HttpPost("disable")]
    [RequireStepUp(Operation = StepUpOperation.DisableTwoFactor)]
    public async Task<IActionResult> Disable(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        await _mediator.Send(new DisableTwoFactorCommand(userId), cancellationToken);

        return NoContent();
    }
}
