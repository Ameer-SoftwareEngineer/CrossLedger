using System.Reflection;
using CrossLedger.Application.Behaviors;
using CrossLedger.Application.Payments;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CrossLedger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Order matters: logging wraps everything; unit-of-work wraps validation and
        // idempotency so the handler's ledger entries and IdempotencyBehavior's stored
        // response commit in one SaveChanges call; validation runs before idempotency
        // so a malformed request never occupies an idempotency key.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

        services.AddScoped<IProviderQuoteScorer, ProviderQuoteScorer>();
        services.AddScoped<PaymentRoutingEngine>();

        return services;
    }
}
