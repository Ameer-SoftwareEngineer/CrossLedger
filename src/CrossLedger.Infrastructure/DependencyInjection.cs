using CrossLedger.Application.Abstractions;
using CrossLedger.Infrastructure.Fx;
using CrossLedger.Infrastructure.Persistence;
using CrossLedger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrossLedger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CrossLedgerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IFxSettlementWalletResolver, FxSettlementWalletResolver>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddSingleton<IClock, SystemClock>();

        AddExchangeRateProviders(services, configuration);

        return services;
    }

    private static void AddExchangeRateProviders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExchangeRateApiOptions>(configuration.GetSection(ExchangeRateApiOptions.SectionName));

        services.AddHttpClient<ExchangeRateApiProvider>(client =>
        {
            client.BaseAddress = new Uri("https://v6.exchangerate-api.com/");
        });

        services.AddHttpClient<FrankfurterProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.frankfurter.app/");
        });

        services.AddMemoryCache();

        // Composition order: Caching(Resilient(Primary, Fallback)) - a singleton so the
        // circuit breaker inside ResilientExchangeRateProvider actually accumulates
        // state across requests instead of resetting every call.
        services.AddSingleton<IExchangeRateProvider>(sp =>
        {
            var primary = sp.GetRequiredService<ExchangeRateApiProvider>();
            var fallback = sp.GetRequiredService<FrankfurterProvider>();
            var resilient = new ResilientExchangeRateProvider(primary, fallback);
            var cache = sp.GetRequiredService<IMemoryCache>();
            return new CachingExchangeRateProvider(resilient, cache);
        });
    }
}
