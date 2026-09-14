using CrossLedger.Application.Abstractions;

namespace CrossLedger.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
