using ElavonPaymentsNet;
using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using System.Collections.Concurrent;

var integrationKey = GetRequiredEnv("ELAVON_INTEGRATION_KEY");
var integrationPassword = GetRequiredEnv("ELAVON_INTEGRATION_PASSWORD");

// Card details can be entered interactively for quick sandbox testing.
// Environment variables are still supported as defaults.
var testCardNumber = PromptWithDefault(
    "Test card number",
    Environment.GetEnvironmentVariable("ELAVON_TEST_CARD_NUMBER") ?? "4929000000006");
var testExpiryDate = PromptWithDefault(
    "Expiry (MMYY)",
    Environment.GetEnvironmentVariable("ELAVON_TEST_CARD_EXPIRY") ?? "1229");
var testSecurityCode = PromptWithDefault(
    "CVV/CVC",
    Environment.GetEnvironmentVariable("ELAVON_TEST_CARD_CVV") ?? "123");
var testCardholderName = PromptWithDefault(
    "Cardholder name",
    Environment.GetEnvironmentVariable("ELAVON_TEST_CARDHOLDER") ?? "Sandbox Tester");
var requestCount = PromptIntWithDefault("Number of purchase requests", 1, 1, 200);
var maxParallelism = PromptIntWithDefault("Max parallel requests", 1, 1, 50);
var useConcurrencyGate = PromptYesNoWithDefault("Enable concurrency gate", true);

var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
{
    IntegrationKey = integrationKey,
    IntegrationPassword = integrationPassword,
    Environment = ElavonEnvironment.Sandbox,
    Timeout = TimeSpan.FromSeconds(30)
});

Console.WriteLine("Running sandbox purchase test...");
Console.WriteLine($"Card: **** **** **** {testCardNumber[^4..]}");
Console.WriteLine($"Requests: {requestCount} | Max parallel: {maxParallelism}");
Console.WriteLine($"Concurrency gate: {(useConcurrencyGate ? "enabled" : "disabled")}");

if (requestCount == 1)
{
    // Keep the common case simple and readable when only one request is needed.
    var request = BuildPurchaseRequest(testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, 1);
    Console.WriteLine($"VendorTxCode: {request.VendorTxCode}");
    await ExecuteSingleAsync(client, request);
    return;
}

var results = new ConcurrentBag<bool>();

if (!useConcurrencyGate)
{
    Console.WriteLine("Running without gate: all requests may run concurrently.");
    var unboundedTasks = Enumerable.Range(1, requestCount).Select(async index =>
    {
        var request = BuildPurchaseRequest(testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, index);
        var ok = await ExecuteSingleAsync(client, request).ConfigureAwait(false);
        results.Add(ok);
    });

    await Task.WhenAll(unboundedTasks).ConfigureAwait(false);
}
else
{
    // Concurrency gate: limits active requests while still allowing batched parallelism.
    var gate = new SemaphoreSlim(maxParallelism);
    var gatedTasks = Enumerable.Range(1, requestCount).Select(async index =>
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var request = BuildPurchaseRequest(testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, index);
            var ok = await ExecuteSingleAsync(client, request).ConfigureAwait(false);
            results.Add(ok);
        }
        finally
        {
            gate.Release();
        }
    });

    await Task.WhenAll(gatedTasks).ConfigureAwait(false);
}

var successCount = results.Count(x => x);
var failureCount = requestCount - successCount;
Console.WriteLine("\nRun summary:");
Console.WriteLine($"Succeeded: {successCount}");
Console.WriteLine($"Failed: {failureCount}");

static string GetRequiredEnv(string name)
{
    // Credentials are required to run against sandbox; fail fast with a clear warning.
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: {name} is not configured.");
        Console.WriteLine("Set sandbox credentials before running the playground.");
        Console.ResetColor();
        throw new InvalidOperationException($"Environment variable '{name}' is required.");
    }

    return value;
}

static string PromptWithDefault(string label, string defaultValue)
{
    // Empty input means "accept default" to keep repetitive runs fast.
    Console.Write($"{label} [{defaultValue}]: ");
    var input = Console.ReadLine();
    return string.IsNullOrWhiteSpace(input) ? defaultValue : input.Trim();
}

static int PromptIntWithDefault(string label, int defaultValue, int min, int max)
{
    // Re-prompt until valid to avoid accidental oversized or invalid load tests.
    while (true)
    {
        Console.Write($"{label} [{defaultValue}]: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultValue;
        }

        if (int.TryParse(input.Trim(), out var parsed) && parsed >= min && parsed <= max)
        {
            return parsed;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Please enter a number between {min} and {max}.");
        Console.ResetColor();
    }
}

static bool PromptYesNoWithDefault(string label, bool defaultValue)
{
    var defaultToken = defaultValue ? "Y/n" : "y/N";
    while (true)
    {
        Console.Write($"{label} [{defaultToken}]: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultValue;
        }

        var normalized = input.Trim().ToLowerInvariant();
        if (normalized is "y" or "yes")
        {
            return true;
        }

        if (normalized is "n" or "no")
        {
            return false;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Please enter y/yes or n/no.");
        Console.ResetColor();
    }
}

static CreateTransactionRequest BuildPurchaseRequest(
    string cardNumber,
    string expiryDate,
    string securityCode,
    string cardholderName,
    int index)
{
    // Build a fresh request each time with a unique vendor transaction code.
    return new CreateTransactionRequest
    {
        TransactionType = TransactionType.Payment,
        VendorTxCode = $"PLAYGROUND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{index:D3}",
        Amount = 100,
        Currency = "GBP",
        Description = "Sandbox purchase from SDK playground",
        PaymentMethod = new PaymentMethod
        {
            Card = new CardDetails
            {
                CardNumber = cardNumber,
                ExpiryDate = expiryDate,
                SecurityCode = securityCode,
                CardholderName = cardholderName
            }
        },
        BillingAddress = new BillingAddress
        {
            Address1 = "1 Sandbox Street",
            City = "London",
            PostalCode = "EC1A 1BB",
            Country = "GB"
        }
    };
}

static async Task<bool> ExecuteSingleAsync(ElavonPaymentsClient client, CreateTransactionRequest request)
{
    // Returns true/false so batch mode can produce a final success/failure summary.
    try
    {
        var result = await client.Transactions.CreateTransactionAsync(request).ConfigureAwait(false);

        Console.WriteLine($"\n[{request.VendorTxCode}] Transaction result:");
        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"StatusCode: {result.StatusCode}");
        Console.WriteLine($"StatusDetail: {result.StatusDetail}");
        Console.WriteLine($"TransactionId: {result.TransactionId}");

        if (result.ThreeDSecure is not null)
        {
            Console.WriteLine("3DS response detected. Complete the 3DS flow if required.");
        }

        return true;
    }
    catch (ElavonAuthenticationException ex)
    {
        Console.WriteLine($"\n[{request.VendorTxCode}] Authentication failed ({ex.HttpStatusCode}). Check ELAVON_INTEGRATION_KEY and ELAVON_INTEGRATION_PASSWORD.");
        Console.WriteLine(ex.RawResponse);
        return false;
    }
    catch (ElavonApiException ex)
    {
        Console.WriteLine($"\n[{request.VendorTxCode}] API error ({ex.HttpStatusCode})");
        Console.WriteLine($"ErrorCode: {ex.ErrorCode}");
        Console.WriteLine($"RawResponse: {ex.RawResponse}");
        return false;
    }
}
