using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class PayoutIdConverter : ValueConverter<PayoutId, Guid>
{
    public PayoutIdConverter() : base(id => id.Value, value => new PayoutId(value))
    {
    }
}
