using CrossLedgerWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedgerWeb.Infrastructure.Persistence.Converters;

public sealed class UsedTotpCodeIdConverter : ValueConverter<UsedTotpCodeId, Guid>
{
    public UsedTotpCodeIdConverter() : base(id => id.Value, value => new UsedTotpCodeId(value))
    {
    }
}
