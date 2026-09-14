using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Wallets;

public sealed record GetWalletBalanceQuery(WalletId WalletId) : IRequest<Money>;
