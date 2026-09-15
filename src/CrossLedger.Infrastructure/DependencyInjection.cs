using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Payments;
using CrossLedger.Infrastructure.Fx;
using CrossLedger.Infrastructure.Identity;
using CrossLedger.Infrastructure.Persistence;
using CrossLedger.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
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
        services.AddScoped<IRoutingAuditLog, RoutingAuditLog>();
        services.AddScoped<IProviderStatsProvider, DefaultProviderStatsProvider>();
        services.AddSingleton<IClock, SystemClock>();

        AddExchangeRateProviders(services, configuration);
        AddIdentityAndJwt(services, configuration);

        return services;
    }

    private static void AddIdentityAndJwt(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Argon2PasswordHasher below replaces Identity's default PBKDF2 hasher;
                // these options are the account-policy side of specification 6.1.
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<CrossLedgerDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2PasswordHasher>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
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
            // frankfurter.app now redirects here permanently; pointing at the current
            // domain directly avoids paying for that redirect on every call. Frankfurter
            // is ECB-sourced, so it only covers the ~30 currencies the ECB publishes -
            // PKR is notably not one of them, so it cannot actually fall back for the
            // USD -> PKR corridor the specification uses as its own example. It still
            // covers the major/EU corridors it was chosen for.
            client.BaseAddress = new Uri("https://api.frankfurter.dev/v1/");
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
