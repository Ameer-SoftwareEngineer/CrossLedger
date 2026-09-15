using CrossLedger.Domain.Auth;
using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.Auth;

public class RecoveryCodeTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static RecoveryCode NewCode() => new(RecoveryCodeId.New(), UserId.New(), "hashed-code", Now);

    [Fact]
    public void A_freshly_issued_code_is_unused()
    {
        var code = NewCode();

        code.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void Consume_marks_it_used()
    {
        var code = NewCode();

        code.Consume(Now.AddMinutes(5));

        code.IsUsed.Should().BeTrue();
        code.UsedAt.Should().Be(Now.AddMinutes(5));
    }

    [Fact]
    public void Consuming_an_already_used_code_throws_instead_of_silently_reusing_it()
    {
        var code = NewCode();
        code.Consume(Now);

        var act = () => code.Consume(Now.AddMinutes(1));

        act.Should().Throw<RecoveryCodeAlreadyUsedException>();
    }

    [Fact]
    public void Constructor_rejects_a_blank_hash()
    {
        var act = () => new RecoveryCode(RecoveryCodeId.New(), UserId.New(), "", Now);

        act.Should().Throw<ArgumentException>();
    }
}
