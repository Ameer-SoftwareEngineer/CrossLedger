using System.Net;
using System.Text;

namespace CrossLedger.Integration.Tests.TestSupport;

/// <summary>Returns a canned response (or throws) for every request, so HTTP-backed
/// providers can be tested without a real network call.</summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    private FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public static FakeHttpMessageHandler ReturningJson(HttpStatusCode statusCode, string json) =>
        new(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });

    public static FakeHttpMessageHandler Throwing(Exception exception) =>
        new(_ => throw exception);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(_respond(request));

    public static HttpClient CreateClient(FakeHttpMessageHandler handler, Uri baseAddress) =>
        new(handler) { BaseAddress = baseAddress };
}
