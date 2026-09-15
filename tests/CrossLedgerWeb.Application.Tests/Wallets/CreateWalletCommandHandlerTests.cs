using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Wallets;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;
using FluentAssertions;
using Moq;

namespace CrossLedgerWeb.Application.Tests.Wallets;

public class CreateWalletCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _wallets = new();

    [Fact]
    public async Task Handle_creates_a_new_customer_wallet_and_stages_it_for_insertion()
    {
        var ownerId = UserId.New();
        var handler = new CreateWalletCommandHandler(_wallets.Object);

        var result = await handler.Handle(new CreateWalletCommand(ownerId, "USD"), CancellationToken.None);

        result.OwnerId.Should().Be(ownerId);
        result.Currency.Should().Be(Currency.From("USD"));
        _wallets.Verify(x => x.Add(It.Is<Wallet>(w => w.Id == result.WalletId && w.Kind == WalletKind.Customer)), Times.Once);
    }
}
