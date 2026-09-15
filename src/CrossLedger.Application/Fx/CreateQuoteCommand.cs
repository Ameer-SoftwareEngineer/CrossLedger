using MediatR;

namespace CrossLedger.Application.Fx;

public sealed record CreateQuoteCommand(string FromCurrency, string ToCurrency, decimal Amount) : IRequest<CreateQuoteResult>;
