using CrossLedger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedger.Infrastructure.Persistence.Converters;

public sealed class RecoveryCodeIdConverter : ValueConverter<RecoveryCodeId, Guid>
{
    public RecoveryCodeIdConverter() : base(id => id.Value, value => new RecoveryCodeId(value))
    {
    }
}
