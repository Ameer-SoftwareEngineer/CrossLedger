using CrossLedger.Application.Payments;
using CrossLedger.Providers.Airwallex;
using CrossLedger.Providers.Rapyd;
using CrossLedger.Providers.Simulated;
using CrossLedger.Providers.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrossLedger.Providers;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AirwallexOptions>(configuration.GetSection(AirwallexOptions.SectionName));
        services.Configure<RapydOptions>(configuration.GetSection(RapydOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));

        services.AddHttpClient<AirwallexProvider>(client =>
            client.BaseAddress = new Uri("https://api.sandbox.airwallex.com/"));

        services.AddHttpClient<RapydProvider>(client =>
            client.BaseAddress = new Uri("https://sandboxapi.rapyd.net/"));

        services.AddHttpClient<StripeProvider>(client =>
            client.BaseAddress = new Uri("https://api.stripe.com/"));

        services.AddSingleton(_ => new SimulatedProvider());

        // Every implementation registered against the same interface - the routing
        // engine resolves IEnumerable<IPaymentProvider> and doesn't know or care how
        // many there are (specification 5.2's Open/Closed point, made in DI wiring).
        services.AddScoped<IPaymentProvider>(sp => sp.GetRequiredService<AirwallexProvider>());
        services.AddScoped<IPaymentProvider>(sp => sp.GetRequiredService<RapydProvider>());
        services.AddScoped<IPaymentProvider>(sp => sp.GetRequiredService<StripeProvider>());
        services.AddScoped<IPaymentProvider>(sp => sp.GetRequiredService<SimulatedProvider>());

        return services;
    }
}
