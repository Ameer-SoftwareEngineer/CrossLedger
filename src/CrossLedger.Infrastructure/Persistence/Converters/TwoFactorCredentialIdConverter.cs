using CrossLedger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrossLedger.Infrastructure.Persistence.Converters;

public sealed class TwoFactorCredentialIdConverter : ValueConverter<TwoFactorCredentialId, Guid>
{
    public TwoFactorCredentialIdConverter() : base(id => id.Value, value => new TwoFactorCredentialId(value))
    {
    }
}
