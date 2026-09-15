using CrossLedgerWeb.Application.Abstractions;

namespace CrossLedgerWeb.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
