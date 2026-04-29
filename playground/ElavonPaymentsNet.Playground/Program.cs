using ElavonPaymentsNet;
using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

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

var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
{
    IntegrationKey = integrationKey,
    IntegrationPassword = integrationPassword,
    Environment = ElavonEnvironment.Sandbox,
    Timeout = TimeSpan.FromSeconds(30)
});

var request = new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode = $"PLAYGROUND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
    Amount = 100,
    Currency = "GBP",
    Description = "Sandbox purchase from SDK playground",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber = testCardNumber,
            ExpiryDate = testExpiryDate,
            SecurityCode = testSecurityCode,
            CardholderName = testCardholderName
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

Console.WriteLine("Running sandbox purchase test...");
Console.WriteLine($"VendorTxCode: {request.VendorTxCode}");
Console.WriteLine($"Card: **** **** **** {testCardNumber[^4..]}");

try
{
    var result = await client.Transactions.CreateTransactionAsync(request);

    Console.WriteLine("\nTransaction result:");
    Console.WriteLine($"Status: {result.Status}");
    Console.WriteLine($"StatusCode: {result.StatusCode}");
    Console.WriteLine($"StatusDetail: {result.StatusDetail}");
    Console.WriteLine($"TransactionId: {result.TransactionId}");

    if (result.ThreeDSecure is not null)
    {
        Console.WriteLine("3DS response detected. Complete the 3DS flow if required.");
    }
}
catch (ElavonAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed ({ex.HttpStatusCode}). Check ELAVON_INTEGRATION_KEY and ELAVON_INTEGRATION_PASSWORD.");
    Console.WriteLine(ex.RawResponse);
}
catch (ElavonApiException ex)
{
    Console.WriteLine($"API error ({ex.HttpStatusCode})");
    Console.WriteLine($"ErrorCode: {ex.ErrorCode}");
    Console.WriteLine($"RawResponse: {ex.RawResponse}");
}

static string GetRequiredEnv(string name)
{
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
    Console.Write($"{label} [{defaultValue}]: ");
    var input = Console.ReadLine();
    return string.IsNullOrWhiteSpace(input) ? defaultValue : input.Trim();
}
