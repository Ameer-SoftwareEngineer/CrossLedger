using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Wallets;

public sealed record CreateWalletCommand(UserId OwnerId, string Currency) : IRequest<CreateWalletResult>;
