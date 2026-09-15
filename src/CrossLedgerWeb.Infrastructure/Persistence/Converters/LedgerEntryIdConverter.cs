using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class LedgerEntryIdConverter : ValueConverter<LedgerEntryId, Guid>
{
    public LedgerEntryIdConverter() : base(id => id.Value, value => new LedgerEntryId(value))
    {
    }
}
