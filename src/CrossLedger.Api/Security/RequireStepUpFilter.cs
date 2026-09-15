using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Auth;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Shared.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CrossLedger.Api.Security;

public sealed class RequireStepUpFilter : IAsyncActionFilter
{
    public const string StepUpTokenHeaderName = "X-Step-Up-Token";

    private readonly IStepUpTokenValidator _validator;
    private readonly IConfiguration _configuration;
    private readonly StepUpOperation _operation;
    private readonly string? _thresholdSetting;

    public RequireStepUpFilter(
        IStepUpTokenValidator validator,
        IConfiguration configuration,
        StepUpOperation operation,
        string? thresholdSetting)
    {
        _validator = validator;
        _configuration = configuration;
        _operation = operation;
        _thresholdSetting = thresholdSetting;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_thresholdSetting is not null && !ExceedsThreshold(context))
        {
            await next();
            return;
        }

        if (!context.HttpContext.User.TryGetUserId(out var callerId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var token = context.HttpContext.Request.Headers[StepUpTokenHeaderName].ToString();
        if (string.IsNullOrEmpty(token))
        {
            context.Result = StepUpRequiredResult();
            return;
        }

        var validation = _validator.Validate(token, _operation);
        if (!validation.IsValid || validation.UserId != callerId)
        {
            context.Result = StepUpRequiredResult();
            return;
        }

        await next();
    }

    /// <summary>Fails closed on either an unrecognised request shape or a missing/
    /// unparseable configuration value - a misconfigured threshold must never silently
    /// waive step-up (specification 6.2 is a security control, not a UX nicety).</summary>
    private bool ExceedsThreshold(ActionExecutingContext context)
    {
        var amountSource = context.ActionArguments.Values.OfType<IStepUpAmountSource>().FirstOrDefault();
        if (amountSource is null)
            return true;

        var threshold = _configuration.GetValue<decimal?>(_thresholdSetting!);
        if (threshold is null)
            return true;

        return amountSource.StepUpAmount >= threshold.Value;
    }

    private static ObjectResult StepUpRequiredResult() =>
        new(new { code = "STEP_UP_REQUIRED", message = "This operation requires a fresh step-up verification." })
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
}
