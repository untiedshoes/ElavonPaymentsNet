using ElavonPaymentsNet;
using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

var integrationKey = GetRequiredEnv("ELAVON_INTEGRATION_KEY");
var integrationPassword = GetRequiredEnv("ELAVON_INTEGRATION_PASSWORD");
var vendorName = GetRequiredEnv("ELAVON_VENDOR_NAME");

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

Console.WriteLine("Running sandbox purchase test...");
Console.WriteLine($"Card: **** **** **** {testCardNumber[^4..]}");

await ExecuteSingleAsync(
    client,
    vendorName,
    testCardNumber,
    testExpiryDate,
    testSecurityCode,
    testCardholderName);

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

static CreateTransactionRequest BuildCardIdentifierPurchaseRequest(
    string merchantSessionKey,
    string cardIdentifier,
    string cardholderName)
{
    // Build a fresh card-identifier request each time with a unique vendor transaction code.
    var (firstName, lastName) = SplitName(cardholderName);

    return new CreateTransactionRequest
    {
        TransactionType = TransactionType.Payment,
        VendorTxCode = $"PLAYGROUND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
        Amount = 100,
        Currency = "GBP",
        Description = "Sandbox card-identifier purchase from SDK playground",
        CustomerFirstName = firstName,
        CustomerLastName = lastName,
        PaymentMethod = new PaymentMethod
        {
            Card = new CardDetails
            {
                MerchantSessionKey = merchantSessionKey,
                CardIdentifier = cardIdentifier
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

static (string FirstName, string LastName) SplitName(string fullName)
{
    var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
        return ("Sandbox", "Tester");

    if (parts.Length == 1)
        return (parts[0], "Tester");

    return (parts[0], string.Join(" ", parts.Skip(1)));
}

static async Task ExecuteSingleAsync(
    ElavonPaymentsClient client,
    string vendorName,
    string cardNumber,
    string expiryDate,
    string securityCode,
    string cardholderName)
{
    try
    {
        Console.WriteLine("Step 1/4: Creating merchant session key...");
        var merchantSession = await client.Wallets
            .CreateMerchantSessionKeyAsync(new MerchantSessionRequest { VendorName = vendorName })
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(merchantSession.MerchantSessionKey))
            throw new InvalidOperationException("Merchant session key was not returned by the API.");

        Console.WriteLine("Step 2/4: Creating card identifier...");
        var cardIdentifierResponse = await client.CardIdentifiers
            .CreateCardIdentifierAsync(
                merchantSession.MerchantSessionKey,
                new CreateCardIdentifierRequest
                {
                    CardDetails = new CardDetails
                    {
                        CardNumber = cardNumber,
                        ExpiryDate = expiryDate,
                        SecurityCode = securityCode,
                        CardholderName = cardholderName
                    }
                })
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(cardIdentifierResponse.CardIdentifier))
            throw new InvalidOperationException("Card identifier was not returned by the API.");

        var request = BuildCardIdentifierPurchaseRequest(
            merchantSession.MerchantSessionKey,
            cardIdentifierResponse.CardIdentifier,
            cardholderName);
        Console.WriteLine($"Step 3/3: Charging card identifier with VendorTxCode {request.VendorTxCode}...");

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

    }
    catch (ElavonAuthenticationException ex)
    {
        Console.WriteLine($"\nAuthentication failed ({ex.HttpStatusCode}). Check ELAVON_INTEGRATION_KEY and ELAVON_INTEGRATION_PASSWORD.");
        Console.WriteLine(ex.RawResponse);
    }
    catch (ElavonApiException ex)
    {
        Console.WriteLine($"\nAPI error ({ex.HttpStatusCode})");
        Console.WriteLine($"ErrorCode: {ex.ErrorCode}");
        Console.WriteLine($"RawResponse: {ex.RawResponse}");
    }
}
