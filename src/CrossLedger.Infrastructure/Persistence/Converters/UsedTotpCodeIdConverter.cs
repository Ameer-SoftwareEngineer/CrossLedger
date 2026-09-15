using CrossLedger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedger.Infrastructure.Persistence.Converters;

public sealed class UsedTotpCodeIdConverter : ValueConverter<UsedTotpCodeId, Guid>
{
    public UsedTotpCodeIdConverter() : base(id => id.Value, value => new UsedTotpCodeId(value))
    {
    }
}
