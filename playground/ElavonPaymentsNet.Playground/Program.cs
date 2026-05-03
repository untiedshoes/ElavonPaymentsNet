using ElavonPaymentsNet;
using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;

var playgroundJsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
};

// Sandbox credentials — publicly available from the Opayo PI REST API documentation.
// Override with environment variables to use a different profile (e.g. sandboxEC).
var integrationKey      = Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_KEY")      ?? "hJYxsw7HLbj40cB8udES8CDRFLhuJ8G54O6rDpUXvE6hYDrria";
var integrationPassword = Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD") ?? "o2iHSrFybYMZpmWOQMuhsXP52V4fBtpuSDshrKDSWsBY1OiN6hwd9Kb12z4j5Us5u";
var vendorName          = Environment.GetEnvironmentVariable("ELAVON_VENDOR_NAME")          ?? "sandbox";

// Card details can be entered interactively for quick sandbox testing.
// Environment variables are still supported as defaults.
//
// Test card 3DS enrollment (Y=enrolled/challenge, N=not enrolled, U=unable to verify, E=error):
//   4929000000006  Visa          Y    CVV 123
//   4929000005559  Visa          N    CVV 123
//   4929000000014  Visa          U    CVV 123
//   4929000000022  Visa          E    CVV 123
//   5186150660000009  MasterCard Y    CVV 123
//   5186150660000025  MasterCard N    CVV 123
//   374200000000004   Amex       N/A  CVV 1234
// Decline testing:
//   4929602110085639  Visa       NotAuthed 2000  "Declined by the bank"
// Address for AVS checks: address1="88", postalCode="412"
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
// Magic cardholder value controls the 3DS simulation outcome in the sandbox.
// SUCCESSFUL = frictionless OK, NOTAUTH = fail, CHALLENGE = challenge flow, etc.
var magicCardholderName = PromptWithDefault(
    "Magic cardholder (3DS simulation)",
    Environment.GetEnvironmentVariable("ELAVON_MAGIC_CARDHOLDER") ?? "SUCCESSFUL");
// Apply3DSecure overrides the account-level 3DS setting for this transaction.
// "Disable" skips 3DS entirely (use for basic payment testing).
// Leave blank to use the account default (required to exercise the 3DS challenge flow).
// sandboxEC has 3DS enabled by default, so we leave blank unless explicitly overridden.
var apply3DSecureDefault = Environment.GetEnvironmentVariable("ELAVON_APPLY_3DS")
    ?? (vendorName.Equals("sandboxEC", StringComparison.OrdinalIgnoreCase) ? "" : "Disable");
var apply3DSecureInput = PromptWithDefault(
    "Apply3DSecure [Disable / Force / UseMSPSetting / blank=account default]",
    apply3DSecureDefault);
var apply3DSecure = Enum.TryParse<Apply3DSecureOption>(apply3DSecureInput, out var parsed)
    ? parsed
    : (Apply3DSecureOption?)null;

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
    testCardholderName,
    magicCardholderName,
    apply3DSecure,
    playgroundJsonOptions);

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
    string cardholderName,
    Apply3DSecureOption? apply3DSecure)
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
            Address1 = "88",
            City = "London",
            PostalCode = "412",
            Country = "GB"
        },
        Apply3DSecure = apply3DSecure,
        StrongCustomerAuthentication = BuildSandboxStrongCustomerAuthentication()
    };
}

static StrongCustomerAuthentication BuildSandboxStrongCustomerAuthentication() =>
    new()
    {
        NotificationURL = Environment.GetEnvironmentVariable("ELAVON_NOTIFICATION_URL") ?? "https://example.com/3ds-notify",
        BrowserIP = Environment.GetEnvironmentVariable("ELAVON_BROWSER_IP") ?? "203.0.113.10",
        BrowserAcceptHeader = Environment.GetEnvironmentVariable("ELAVON_BROWSER_ACCEPT") ?? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        BrowserJavascriptEnabled = true,
        BrowserJavaEnabled = false,
        BrowserLanguage = Environment.GetEnvironmentVariable("ELAVON_BROWSER_LANGUAGE") ?? "en-GB",
        BrowserColorDepth = Environment.GetEnvironmentVariable("ELAVON_BROWSER_COLOR_DEPTH") ?? "24",
        BrowserScreenHeight = Environment.GetEnvironmentVariable("ELAVON_BROWSER_SCREEN_HEIGHT") ?? "1080",
        BrowserScreenWidth = Environment.GetEnvironmentVariable("ELAVON_BROWSER_SCREEN_WIDTH") ?? "1920",
        BrowserTZ = Environment.GetEnvironmentVariable("ELAVON_BROWSER_TZ") ?? "0",
        BrowserUserAgent = Environment.GetEnvironmentVariable("ELAVON_BROWSER_USER_AGENT") ?? "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36",
        ChallengeWindowSize = Environment.GetEnvironmentVariable("ELAVON_CHALLENGE_WINDOW_SIZE") ?? "FullScreen",
        TransType = Environment.GetEnvironmentVariable("ELAVON_TRANS_TYPE") ?? "GoodsAndServicePurchase",
        ThreeDSRequestorChallengeInd = Environment.GetEnvironmentVariable("ELAVON_3DS_REQUESTOR_CHALLENGE_IND") ?? "03"
    };

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
    string cardholderName,
    string magicCardholderName,
    Apply3DSecureOption? apply3DSecure,
    JsonSerializerOptions jsonOptions)
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
                        CardholderName = magicCardholderName
                    }
                })
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(cardIdentifierResponse.CardIdentifier))
            throw new InvalidOperationException("Card identifier was not returned by the API.");

        var request = BuildCardIdentifierPurchaseRequest(
            merchantSession.MerchantSessionKey,
            cardIdentifierResponse.CardIdentifier,
            cardholderName,
            apply3DSecure);
        Console.WriteLine($"Step 3/4: Charging card identifier with VendorTxCode {request.VendorTxCode}...");

        var result = await client.Transactions.CreateTransactionAsync(request).ConfigureAwait(false);

        Console.WriteLine($"\n[{request.VendorTxCode}] Transaction result:");
        Console.WriteLine($"Status:        {result.Status}");
        Console.WriteLine($"StatusCode:    {result.StatusCode}");
        Console.WriteLine($"StatusDetail:  {result.StatusDetail}");
        Console.WriteLine($"TransactionId: {result.TransactionId}");
        PrintFullResponse("Full transaction response JSON", result, jsonOptions);

        if (result.Status == "3DAuth" && !string.IsNullOrWhiteSpace(result.TransactionId))
        {
            await Handle3DsChallengeAsync(client, result.TransactionId, result.AcsUrl, result.CReq, jsonOptions);
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

static async Task Handle3DsChallengeAsync(
    ElavonPaymentsClient client,
    string transactionId,
    string? acsUrl,
    string? cReq,
    JsonSerializerOptions jsonOptions)
{
    Console.WriteLine("\n--- 3D Secure v2 Challenge Required ---");
    Console.WriteLine();
    Console.WriteLine("ACS URL (redirect your customer here):");
    if (string.IsNullOrWhiteSpace(acsUrl) || string.IsNullOrWhiteSpace(cReq))
    {
        Console.WriteLine("ACS URL or cReq was not returned. Cannot continue 3DS challenge flow.");
        return;
    }

    var curlCommand = $"curl -X POST --data-urlencode \"creq={cReq}\" \"{acsUrl}\"";
    Console.WriteLine(acsUrl);
    Console.WriteLine();
    Console.WriteLine("cReq (POST as the 'creq' parameter to the ACS URL):");
    Console.WriteLine(cReq);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Important: opening the ACS URL directly in a browser will fail with 405 (Method Not Allowed).");
    Console.WriteLine("The simulator requires an HTTP POST with form field 'creq'.");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Quick test with curl (copy/paste):");
    Console.WriteLine(curlCommand);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("In the sandbox ACS page, enter the password to simulate success.");
    Console.WriteLine("PI REST API sandbox password: challenge");
    Console.ResetColor();
    Console.WriteLine();
    Console.Write("Paste the cRes value here and press Enter: ");
    var cRes = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(cRes))
    {
        Console.WriteLine("No cRes provided. Skipping 3DS completion.");
        return;
    }

    Complete3DsResponse completeResponse;
    try
    {
        completeResponse = await client.ThreeDs
            .Complete3DsAsync(transactionId, new Complete3DsRequest { CRes = cRes })
            .ConfigureAwait(false);
    }
    catch (ElavonApiException ex) when (!string.IsNullOrWhiteSpace(ex.RawResponse)
        && ex.RawResponse.Contains("\"code\":1029", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("3DS completion failed: ACS returned an Error message instead of a successful cRes.");
        Console.WriteLine("Likely cause: the cReq was already consumed (single-use) for this transaction.");
        Console.WriteLine("Start a new transaction and complete the challenge once with the fresh cReq.");
        Console.ResetColor();
        Console.WriteLine($"RawResponse: {ex.RawResponse}");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("3DS completion result:");
    Console.WriteLine($"Status:        {completeResponse.Status}");
    Console.WriteLine($"StatusDetail:  {completeResponse.StatusDetail}");
    Console.WriteLine($"TransactionId: {completeResponse.TransactionId}");
    Console.WriteLine($"AcsTransId:    {completeResponse.AcsTransId}");
    Console.WriteLine($"DsTransId:     {completeResponse.DsTransId}");
    if (completeResponse.ThreeDSecure is not null)
        Console.WriteLine($"3DSecure:      {completeResponse.ThreeDSecure.Status}");
    PrintFullResponse("Full 3DS completion response JSON", completeResponse, jsonOptions);
}

static void PrintFullResponse<T>(string title, T response, JsonSerializerOptions jsonOptions)
{
    Console.WriteLine();
    Console.WriteLine(title + ":");
    Console.WriteLine(JsonSerializer.Serialize(response, jsonOptions));
}
