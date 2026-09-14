using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Transfers;

public sealed record CreateTransferCommand(
    QuoteId QuoteId,
    WalletId SourceWalletId,
    WalletId TargetWalletId,
    decimal SourceAmount,
    string IdempotencyKey) : IRequest<CreateTransferResult>, IIdempotentRequest;
