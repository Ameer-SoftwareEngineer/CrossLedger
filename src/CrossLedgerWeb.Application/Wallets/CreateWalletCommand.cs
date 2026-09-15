using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Wallets;

public sealed record CreateWalletCommand(UserId OwnerId, string Currency) : IRequest<CreateWalletResult>;
