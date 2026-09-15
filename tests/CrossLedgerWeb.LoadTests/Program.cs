using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

// Proves specification 3.6/5.6's concurrency claim against the REAL transfer path
// (POST /api/v1/transfers, backed by EF Core RowVersion optimistic concurrency -
// specification 2.4), not the standalone usp_PostTransfer stored procedure, since the
// stored procedure isn't the code path the running API actually uses. Many concurrent
// requests race to debit the exact same wallet by exactly the amount it holds, so at
// most one can ever legitimately succeed; the assertion isn't "the load test didn't
// crash", it's a direct SQL check afterwards (see run-load-test.sh) that the wallet
// never went negative and the whole ledger still sums to zero per currency.
//
// Two modes because the API has no wallet-funding endpoint yet - the driving shell
// script needs to seed a credit directly in SQL between them:
//   setup - registers a user, creates a USD source + EUR target wallet, writes their ids
//           and a bearer token to state.json, then exits.
//   run   - reads state.json (by then the source wallet has been funded directly in SQL)
//           and fires the actual concurrent burst at it.

const string BaseUrl = "http://localhost:5090";
const decimal AttemptAmount = 100.00m;
const string StateFile = "state.json";

if (args.Length == 0 || (args[0] != "setup" && args[0] != "run"))
{
    Console.WriteLine("Usage: dotnet run -- setup | dotnet run -- run");
    return 1;
}

if (args[0] == "setup")
{
    using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };

    var email = $"loadtest.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}@example.com";
    const string password = "Str0ng!Passw0rd";

    var registerResponse = await client.PostAsJsonAsync("api/v1/auth/register", new { email, password });
    registerResponse.EnsureSuccessStatusCode();
    var registered = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
    var userId = registered.GetProperty("userId").GetString();

    var loginResponse = await client.PostAsJsonAsync("api/v1/auth/login", new { email, password });
    loginResponse.EnsureSuccessStatusCode();
    var login = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
    var token = login.GetProperty("accessToken").GetString();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var sourceResponse = await client.PostAsJsonAsync("api/v1/wallets", new { ownerId = userId, currency = "USD" });
    sourceResponse.EnsureSuccessStatusCode();
    var sourceWallet = await sourceResponse.Content.ReadFromJsonAsync<JsonElement>();
    var sourceWalletId = sourceWallet.GetProperty("walletId").GetString();

    var targetResponse = await client.PostAsJsonAsync("api/v1/wallets", new { ownerId = userId, currency = "EUR" });
    targetResponse.EnsureSuccessStatusCode();
    var targetWallet = await targetResponse.Content.ReadFromJsonAsync<JsonElement>();
    var targetWalletId = targetWallet.GetProperty("walletId").GetString();

    await File.WriteAllTextAsync(StateFile, JsonSerializer.Serialize(new
    {
        token,
        sourceWalletId,
        targetWalletId,
    }));

    Console.WriteLine($"Setup complete. sourceWalletId={sourceWalletId} targetWalletId={targetWalletId}");
    return 0;
}

// args[0] == "run"
var state = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(StateFile));
var runToken = state.GetProperty("token").GetString();
var runSourceWalletId = state.GetProperty("sourceWalletId").GetString();
var runTargetWalletId = state.GetProperty("targetWalletId").GetString();

var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", runToken);

var attemptCounter = 0;

var scenario = Scenario.Create("concurrent_transfers_same_wallet", async _ =>
{
    var attemptId = Interlocked.Increment(ref attemptCounter);

    // A fresh quote per attempt avoids racing the 30-second quote expiry window
    // (specification 2.2) entirely - each concurrent "customer" gets their own quote,
    // exactly as they would in reality.
    var quoteResponse = await httpClient.PostAsJsonAsync("api/v1/quotes", new
    {
        fromCurrency = "USD",
        toCurrency = "EUR",
        amount = AttemptAmount,
    });

    if (!quoteResponse.IsSuccessStatusCode)
        return Response.Fail(statusCode: ((int)quoteResponse.StatusCode).ToString());

    var quote = await quoteResponse.Content.ReadFromJsonAsync<JsonElement>();
    var quoteId = quote.GetProperty("quoteId").GetString();

    var transferRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/transfers")
    {
        Content = JsonContent.Create(new
        {
            quoteId,
            sourceWalletId = runSourceWalletId,
            targetWalletId = runTargetWalletId,
            sourceAmount = AttemptAmount,
        }),
    };
    transferRequest.Headers.Add("Idempotency-Key", $"loadtest-{attemptId}-{Guid.NewGuid()}");

    var transferResponse = await httpClient.SendAsync(transferRequest);

    return transferResponse.IsSuccessStatusCode
        ? Response.Ok(statusCode: ((int)transferResponse.StatusCode).ToString())
        : Response.Fail(statusCode: ((int)transferResponse.StatusCode).ToString());
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.Inject(rate: 60, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(2))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();

return 0;
