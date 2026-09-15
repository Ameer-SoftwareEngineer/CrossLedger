using MediatR;

namespace CrossLedgerWeb.Application.Fx;

public sealed record CreateQuoteCommand(string FromCurrency, string ToCurrency, decimal Amount) : IRequest<CreateQuoteResult>;
