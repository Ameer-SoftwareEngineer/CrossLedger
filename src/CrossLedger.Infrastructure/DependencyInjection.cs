using CrossLedger.Application.Abstractions;
using CrossLedger.Infrastructure.Persistence;
using CrossLedger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrossLedger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CrossLedgerDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IFxSettlementWalletResolver, FxSettlementWalletResolver>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
