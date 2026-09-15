using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class WalletIdConverter : ValueConverter<WalletId, Guid>
{
    public WalletIdConverter() : base(id => id.Value, value => new WalletId(value))
    {
    }
}
