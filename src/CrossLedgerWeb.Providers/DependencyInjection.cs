using CrossLedgerWeb.Application.Payments;
using CrossLedgerWeb.Providers.Airwallex;
using CrossLedgerWeb.Providers.Rapyd;
using CrossLedgerWeb.Providers.Simulated;
using CrossLedgerWeb.Providers.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrossLedgerWeb.Providers;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AirwallexOptions>(configuration.GetSection(AirwallexOptions.SectionName));
        services.Configure<RapydOptions>(configuration.GetSection(RapydOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<SimulatedProviderOptions>(configuration.GetSection(SimulatedProviderOptions.SectionName));

        services.AddHttpClient<AirwallexProvider>(client =>
            client.BaseAddress = new Uri("https://api.sandbox.airwallex.com/"));

        services.AddHttpClient<RapydProvider>(client =>
            client.BaseAddress = new Uri("https://sandboxapi.rapyd.net/"));

        services.AddHttpClient<StripeProvider>(client =>
            client.BaseAddress = new Uri("https://api.stripe.com/"));

        // Singleton (not Scoped, like the real providers) because its idempotency
        // dictionary needs to persist across requests to mean anything - a Scoped
        // instance would reset it every call. IOptions<SimulatedProviderOptions>, unlike
        // the constructor's own default, actually resolves from configuration - there was
        // previously no way to set its chaos knobs (RejectAllPayouts etc.) outside a test
        // constructing the provider directly.
        services.AddSingleton(sp => new SimulatedProvider(sp.GetRequiredService<IOptions<SimulatedProviderOptions>>().Value));

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
