using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Auth;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrossLedger.Api.Security;

/// <summary>Declarative step-up enforcement (specification 6.3). Runs as an action
/// filter rather than an authorization filter so it can inspect the model-bound request
/// body for ThresholdSetting checks - MVC's authorization stage runs before model
/// binding, so the amount isn't available there yet. It still runs before the handler,
/// after [Authorize] (which is a true authorization filter and always runs first
/// regardless of attribute order), so unauthenticated/wrong-role callers never reach it.
///
/// TransfersController.cs (worked example, specification 6.3):
/// [Authorize(Roles = Roles.Customer)]
/// [RequireStepUp(Operation = StepUpOperation.HighValueTransfer, ThresholdSetting = "Limits:StepUpAbove")]
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireStepUpAttribute : Attribute, IFilterFactory
{
    public required StepUpOperation Operation { get; init; }

    /// <summary>Configuration key holding the amount above which step-up is required
    /// (e.g. "Limits:StepUpAbove"). Leave null for operations that always require
    /// step-up regardless of amount (disabling 2FA, changing password, etc.) - only
    /// "Transfer above threshold" is conditional (specification 6.2).</summary>
    public string? ThresholdSetting { get; init; }

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var validator = serviceProvider.GetRequiredService<IStepUpTokenValidator>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new RequireStepUpFilter(validator, configuration, Operation, ThresholdSetting);
    }
}
