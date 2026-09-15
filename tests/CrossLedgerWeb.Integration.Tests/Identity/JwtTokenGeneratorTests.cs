using CrossLedgerWeb.Application.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure;
using CrossLedgerWeb.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CrossLedgerWeb.Integration.Tests.Identity;

// Exercises the real token round trip (generate -> validate) rather than mocking
// IJwtTokenGenerator/IStepUpTokenValidator, because JwtSecurityTokenHandler's inbound
// claim mapping (short "sub" -> the long ClaimTypes.NameIdentifier URI) is exactly the
// kind of framework behaviour that silently breaks a claim lookup by literal name.
public sealed class JwtTokenGeneratorTests
{
    private static readonly IOptions<JwtOptions> Options = Microsoft.Extensions.Options.Options.Create(new JwtOptions
    {
        Issuer = "CrossLedgerWeb.Tests",
        Audience = "CrossLedgerWeb.Tests",
        SigningKey = "unit-test-signing-key-at-least-32-bytes-long!",
        AccessTokenLifetimeMinutes = 15,
    });

    private readonly JwtTokenGenerator _generator = new(Options, new SystemClock());
    private readonly StepUpTokenValidator _validator = new(Options);

    [Fact]
    public void A_step_up_token_validates_as_the_issuing_user_and_operation()
    {
        var userId = UserId.New();

        var token = _generator.GenerateStepUpToken(userId, StepUpOperation.HighValueTransfer);

        var result = _validator.Validate(token.Value, StepUpOperation.HighValueTransfer);

        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be(userId);
        result.Operation.Should().Be(StepUpOperation.HighValueTransfer);
    }

    [Fact]
    public void A_step_up_token_is_rejected_for_a_different_operation_than_it_was_scoped_to()
    {
        var token = _generator.GenerateStepUpToken(UserId.New(), StepUpOperation.HighValueTransfer);

        var result = _validator.Validate(token.Value, StepUpOperation.DisableTwoFactor);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void A_malformed_token_is_rejected_without_throwing()
    {
        var act = () => _validator.Validate("not-a-jwt", StepUpOperation.HighValueTransfer);

        act.Should().NotThrow();
        act().IsValid.Should().BeFalse();
    }
}
