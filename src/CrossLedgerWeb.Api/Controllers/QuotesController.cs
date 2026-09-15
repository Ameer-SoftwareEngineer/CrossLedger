using CrossLedgerWeb.Application.Auth;
using CrossLedgerWeb.Application.Fx;
using CrossLedgerWeb.Shared.Fx;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrossLedgerWeb.Api.Controllers;

[ApiController]
[Route("api/v1/quotes")]
[Authorize(Roles = Roles.Customer)]
public sealed class QuotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Specification 2.2's QUOTE LIFECYCLE: locks a mid-market rate plus
    /// platform spread for 30 seconds. The returned QuoteId is what POST
    /// /api/v1/transfers accepts - never a raw amount.</summary>
    [HttpPost]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<QuoteResponse>> Create(CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateQuoteCommand(request.FromCurrency, request.ToCurrency, request.Amount), cancellationToken);

        var response = new QuoteResponse(
            result.QuoteId.Value, result.FromCurrency.Code, result.ToCurrency.Code, result.Rate, result.ExpiresAt);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
