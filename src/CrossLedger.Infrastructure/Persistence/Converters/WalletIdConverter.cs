using CrossLedger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedger.Infrastructure.Persistence.Converters;

public sealed class WalletIdConverter : ValueConverter<WalletId, Guid>
{
    public WalletIdConverter() : base(id => id.Value, value => new WalletId(value))
    {
    }
}
