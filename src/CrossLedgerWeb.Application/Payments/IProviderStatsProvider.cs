namespace CrossLedgerWeb.Application.Payments;

public interface IProviderStatsProvider
{
    Task<ProviderStats> GetStatsAsync(ProviderCode providerCode, CancellationToken cancellationToken);
}
