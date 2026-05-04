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

// ---------------------------------------------------------------------------
// Test card reference
// ---------------------------------------------------------------------------
// 3DS enrollment (Y=enrolled/challenge, N=not enrolled, U=unable, E=error):
//   4929000000006     Visa          Y    CVV 123
//   4929000005559     Visa          N    CVV 123
//   4929000000014     Visa          U    CVV 123
//   4929000000022     Visa          E    CVV 123
//   5186150660000009  MasterCard    Y    CVV 123
//   5186150660000025  MasterCard    N    CVV 123
//   374200000000004   Amex          N/A  CVV 1234
// Decline testing:
//   4929602110085639  Visa          NotAuthed 2000 "Declined by the bank"
// AVS address: address1="88", postalCode="412"
// 3DS sandbox challenge password: challenge
// ---------------------------------------------------------------------------

var testCardNumber    = PromptWithDefault("Test card number",  Environment.GetEnvironmentVariable("ELAVON_TEST_CARD_NUMBER") ?? "4929000000006");
var testExpiryDate    = PromptWithDefault("Expiry (MMYY)",     Environment.GetEnvironmentVariable("ELAVON_TEST_CARD_EXPIRY") ?? "1229");
var testSecurityCode  = PromptWithDefault("CVV/CVC",           Environment.GetEnvironmentVariable("ELAVON_TEST_CARD_CVV")   ?? "123");
var testCardholderName = PromptWithDefault("Cardholder name",  Environment.GetEnvironmentVariable("ELAVON_TEST_CARDHOLDER") ?? "Sandbox Tester");

// Magic cardholder name controls 3DS simulation outcome.
var magicCardholderName = PromptWithDefault(
    "Magic cardholder (3DS simulation — SUCCESSFUL / NOTAUTH / CHALLENGE)",
    Environment.GetEnvironmentVariable("ELAVON_MAGIC_CARDHOLDER") ?? "SUCCESSFUL");

// Apply3DSecure override.
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
    IntegrationKey      = integrationKey,
    IntegrationPassword = integrationPassword,
    Environment         = ElavonEnvironment.Sandbox,
    Timeout             = TimeSpan.FromSeconds(30)
});

// ---------------------------------------------------------------------------
// Scenario menu
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== ElavonPaymentsNet Playground ===");
Console.WriteLine();
Console.WriteLine("  1  Payment (basic card-identifier purchase)");
Console.WriteLine("  2  Deferred payment → Capture");
Console.WriteLine("  3  Payment → Full refund");
Console.WriteLine("  4  Payment → Partial refund");
Console.WriteLine("  5  Payment → Void");
Console.WriteLine("  6  Token flow (CreateToken → PayWithToken)");
Console.WriteLine();
Console.Write("Select scenario [1]: ");
var scenarioInput = Console.ReadLine()?.Trim();
var scenario = string.IsNullOrWhiteSpace(scenarioInput) ? "1" : scenarioInput;

Console.WriteLine();

switch (scenario)
{
    case "1":
        await RunPaymentScenarioAsync(client, vendorName, testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, magicCardholderName, apply3DSecure, playgroundJsonOptions);
        break;
    case "2":
        await RunCapturScenarioAsync(client, vendorName, testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, magicCardholderName, apply3DSecure, playgroundJsonOptions);
        break;
    case "3":
        await RunRefundScenarioAsync(client, vendorName, testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, magicCardholderName, apply3DSecure, full: true, playgroundJsonOptions);
        break;
    case "4":
        await RunRefundScenarioAsync(client, vendorName, testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, magicCardholderName, apply3DSecure, full: false, playgroundJsonOptions);
        break;
    case "5":
        await RunVoidScenarioAsync(client, vendorName, testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, magicCardholderName, apply3DSecure, playgroundJsonOptions);
        break;
    case "6":
        await RunTokenScenarioAsync(client, testCardNumber, testExpiryDate, testSecurityCode, testCardholderName, playgroundJsonOptions);
        break;
    default:
        Console.WriteLine($"Unknown scenario '{scenario}'. Exiting.");
        break;
}

// ===========================================================================
// Scenario implementations
// ===========================================================================

static async Task RunPaymentScenarioAsync(
    ElavonPaymentsClient client,
    string vendorName,
    string cardNumber, string expiryDate, string securityCode,
    string cardholderName, string magicCardholderName,
    Apply3DSecureOption? apply3DSecure,
    JsonSerializerOptions jsonOptions)
{
    PrintHeading("Scenario 1: Payment");
    try
    {
        var (msk, cardIdentifier) = await CreateMskAndCardIdentifierAsync(client, vendorName, cardNumber, expiryDate, securityCode, magicCardholderName);

        var request = BuildCardIdentifierPurchaseRequest(msk, cardIdentifier, cardholderName, apply3DSecure, TransactionType.Payment, 100);
        PrintStep(3, 3, $"Charging card — VendorTxCode {request.VendorTxCode}");
        var result = await client.Transactions.CreateTransactionAsync(request).ConfigureAwait(false);
        PrintTransactionResult(result, jsonOptions);

        if (result.Status == "3DAuth" && !string.IsNullOrWhiteSpace(result.TransactionId))
            await Handle3DsChallengeAsync(client, result.TransactionId, result.AcsUrl, result.CReq, jsonOptions);
    }
    catch (ElavonApiException ex) { PrintApiException(ex); }
}

static async Task RunCapturScenarioAsync(
    ElavonPaymentsClient client,
    string vendorName,
    string cardNumber, string expiryDate, string securityCode,
    string cardholderName, string magicCardholderName,
    Apply3DSecureOption? apply3DSecure,
    JsonSerializerOptions jsonOptions)
{
    PrintHeading("Scenario 2: Deferred payment → Capture");
    try
    {
        var (msk, cardIdentifier) = await CreateMskAndCardIdentifierAsync(client, vendorName, cardNumber, expiryDate, securityCode, magicCardholderName);

        var request = BuildCardIdentifierPurchaseRequest(msk, cardIdentifier, cardholderName, apply3DSecure, TransactionType.Deferred, 500);
        PrintStep(3, 4, $"Creating deferred payment — VendorTxCode {request.VendorTxCode}");
        var deferred = await client.Transactions.CreateTransactionAsync(request).ConfigureAwait(false);
        PrintTransactionResult(deferred, jsonOptions);

        if (string.IsNullOrWhiteSpace(deferred.TransactionId) || deferred.StatusKind != TransactionStatusKind.Ok)
        {
            Console.WriteLine("Deferred payment did not succeed — cannot capture.");
            return;
        }

        PrintStep(4, 4, $"Capturing transaction {deferred.TransactionId}");
        var capture = await client.PostPayments.CaptureTransactionAsync(
            deferred.TransactionId,
            new CapturePaymentRequest { Amount = 500 }).ConfigureAwait(false);
        Console.WriteLine($"Capture status: {capture.Status}");
        Console.WriteLine($"Capture detail: {capture.StatusDetail}");
        PrintFullResponse("Full capture response JSON", capture, jsonOptions);
    }
    catch (ElavonApiException ex) { PrintApiException(ex); }
}

static async Task RunRefundScenarioAsync(
    ElavonPaymentsClient client,
    string vendorName,
    string cardNumber, string expiryDate, string securityCode,
    string cardholderName, string magicCardholderName,
    Apply3DSecureOption? apply3DSecure,
    bool full,
    JsonSerializerOptions jsonOptions)
{
    var label = full ? "Full refund" : "Partial refund (50%)";
    PrintHeading($"Scenario {(full ? 3 : 4)}: Payment → {label}");
    try
    {
        var (msk, cardIdentifier) = await CreateMskAndCardIdentifierAsync(client, vendorName, cardNumber, expiryDate, securityCode, magicCardholderName);

        var request = BuildCardIdentifierPurchaseRequest(msk, cardIdentifier, cardholderName, apply3DSecure, TransactionType.Payment, 200);
        PrintStep(3, 4, $"Creating payment — VendorTxCode {request.VendorTxCode}");
        var payment = await client.Transactions.CreateTransactionAsync(request).ConfigureAwait(false);
        PrintTransactionResult(payment, jsonOptions);

        if (string.IsNullOrWhiteSpace(payment.TransactionId) || payment.StatusKind != TransactionStatusKind.Ok)
        {
            Console.WriteLine("Payment did not succeed — cannot refund.");
            return;
        }

        var refundAmount = full ? 200 : 100;
        PrintStep(4, 4, $"Refunding {refundAmount}p against transaction {payment.TransactionId}");
        var refund = await client.PostPayments.RefundTransactionAsync(
            payment.TransactionId,
            new RefundPaymentRequest
            {
                Amount      = refundAmount,
                VendorTxCode = $"REFUND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
                Description = full ? "Full refund" : "Partial refund (50%)"
            }).ConfigureAwait(false);
        Console.WriteLine($"Refund status: {refund.Status}");
        Console.WriteLine($"Refund detail: {refund.StatusDetail}");
        PrintFullResponse("Full refund response JSON", refund, jsonOptions);
    }
    catch (ElavonApiException ex) { PrintApiException(ex); }
}

static async Task RunVoidScenarioAsync(
    ElavonPaymentsClient client,
    string vendorName,
    string cardNumber, string expiryDate, string securityCode,
    string cardholderName, string magicCardholderName,
    Apply3DSecureOption? apply3DSecure,
    JsonSerializerOptions jsonOptions)
{
    PrintHeading("Scenario 5: Payment → Void");
    try
    {
        var (msk, cardIdentifier) = await CreateMskAndCardIdentifierAsync(client, vendorName, cardNumber, expiryDate, securityCode, magicCardholderName);

        var request = BuildCardIdentifierPurchaseRequest(msk, cardIdentifier, cardholderName, apply3DSecure, TransactionType.Payment, 150);
        PrintStep(3, 4, $"Creating payment — VendorTxCode {request.VendorTxCode}");
        var payment = await client.Transactions.CreateTransactionAsync(request).ConfigureAwait(false);
        PrintTransactionResult(payment, jsonOptions);

        if (string.IsNullOrWhiteSpace(payment.TransactionId) || payment.StatusKind != TransactionStatusKind.Ok)
        {
            Console.WriteLine("Payment did not succeed — cannot void.");
            return;
        }

        PrintStep(4, 4, $"Voiding transaction {payment.TransactionId}");
        var voidResult = await client.PostPayments.VoidTransactionAsync(payment.TransactionId).ConfigureAwait(false);
        Console.WriteLine($"Void status: {voidResult.Status}");
        Console.WriteLine($"Void detail: {voidResult.StatusDetail}");
        PrintFullResponse("Full void response JSON", voidResult, jsonOptions);
    }
    catch (ElavonApiException ex) { PrintApiException(ex); }
}

static async Task RunTokenScenarioAsync(
    ElavonPaymentsClient client,
    string cardNumber, string expiryDate, string securityCode,
    string cardholderName,
    JsonSerializerOptions jsonOptions)
{
    PrintHeading("Scenario 6: CreateToken → PayWithToken");
    try
    {
        PrintStep(1, 2, "Creating card token...");
        var tokenResponse = await client.Tokens.CreateTokenAsync(new CreateTokenRequest
        {
            Card = new CardDetails
            {
                CardNumber     = cardNumber,
                ExpiryDate     = expiryDate,
                SecurityCode   = securityCode,
                CardholderName = cardholderName
            }
        }).ConfigureAwait(false);

        Console.WriteLine($"Token: {tokenResponse.Token}");
        PrintFullResponse("Full create-token response JSON", tokenResponse, jsonOptions);

        if (string.IsNullOrWhiteSpace(tokenResponse.Token))
        {
            Console.WriteLine("Token was not returned — cannot proceed.");
            return;
        }

        var vendorTxCode = $"TOKEN-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";
        PrintStep(2, 2, $"Paying with token — VendorTxCode {vendorTxCode}");
        var tokenPayment = await client.Tokens.PayWithTokenAsync(new PayWithTokenRequest
        {
            VendorTxCode = vendorTxCode,
            Amount       = 100,
            Currency     = "GBP",
            Token        = tokenResponse.Token
        }).ConfigureAwait(false);

        Console.WriteLine($"Status:        {tokenPayment.Status}");
        Console.WriteLine($"StatusCode:    {tokenPayment.StatusCode}");
        Console.WriteLine($"StatusDetail:  {tokenPayment.StatusDetail}");
        Console.WriteLine($"TransactionId: {tokenPayment.TransactionId}");
        PrintFullResponse("Full token payment response JSON", tokenPayment, jsonOptions);
    }
    catch (ElavonApiException ex) { PrintApiException(ex); }
}

// ===========================================================================
// Shared helpers
// ===========================================================================

static async Task<(string Msk, string CardIdentifier)> CreateMskAndCardIdentifierAsync(
    ElavonPaymentsClient client,
    string vendorName,
    string cardNumber, string expiryDate, string securityCode,
    string magicCardholderName)
{
    PrintStep(1, 4, "Creating merchant session key...");
    var mskResponse = await client.Wallets
        .CreateMerchantSessionKeyAsync(new MerchantSessionRequest { VendorName = vendorName })
        .ConfigureAwait(false);

    if (string.IsNullOrWhiteSpace(mskResponse.MerchantSessionKey))
        throw new InvalidOperationException("Merchant session key was not returned.");

    PrintStep(2, 4, "Creating card identifier...");
    var cardIdResponse = await client.CardIdentifiers
        .CreateCardIdentifierAsync(
            mskResponse.MerchantSessionKey,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = cardNumber,
                    ExpiryDate     = expiryDate,
                    SecurityCode   = securityCode,
                    CardholderName = magicCardholderName
                }
            })
        .ConfigureAwait(false);

    if (string.IsNullOrWhiteSpace(cardIdResponse.CardIdentifier))
        throw new InvalidOperationException("Card identifier was not returned.");

    return (mskResponse.MerchantSessionKey, cardIdResponse.CardIdentifier);
}

static CreateTransactionRequest BuildCardIdentifierPurchaseRequest(
    string merchantSessionKey,
    string cardIdentifier,
    string cardholderName,
    Apply3DSecureOption? apply3DSecure,
    TransactionType transactionType,
    int amountPence)
{
    var (firstName, lastName) = SplitName(cardholderName);

    return new CreateTransactionRequest
    {
        TransactionType   = transactionType,
        VendorTxCode      = $"PLAYGROUND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
        Amount            = amountPence,
        Currency          = "GBP",
        Description       = $"Sandbox {transactionType} from SDK playground",
        CustomerFirstName = firstName,
        CustomerLastName  = lastName,
        PaymentMethod = new PaymentMethod
        {
            Card = new CardDetails
            {
                MerchantSessionKey = merchantSessionKey,
                CardIdentifier     = cardIdentifier
            }
        },
        BillingAddress = new BillingAddress
        {
            Address1   = "88",
            City       = "London",
            PostalCode = "412",
            Country    = "GB"
        },
        Apply3DSecure                = apply3DSecure,
        StrongCustomerAuthentication = BuildSandboxSca()
    };
}

static StrongCustomerAuthentication BuildSandboxSca() => new()
{
    NotificationURL      = Environment.GetEnvironmentVariable("ELAVON_NOTIFICATION_URL")        ?? "https://example.com/3ds-notify",
    BrowserIP            = Environment.GetEnvironmentVariable("ELAVON_BROWSER_IP")              ?? "203.0.113.10",
    BrowserAcceptHeader  = Environment.GetEnvironmentVariable("ELAVON_BROWSER_ACCEPT")          ?? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
    BrowserJavascriptEnabled = true,
    BrowserJavaEnabled       = false,
    BrowserLanguage           = Environment.GetEnvironmentVariable("ELAVON_BROWSER_LANGUAGE")   ?? "en-GB",
    BrowserColorDepth         = Environment.GetEnvironmentVariable("ELAVON_BROWSER_COLOR_DEPTH") ?? "24",
    BrowserScreenHeight       = Environment.GetEnvironmentVariable("ELAVON_BROWSER_SCREEN_HEIGHT") ?? "1080",
    BrowserScreenWidth        = Environment.GetEnvironmentVariable("ELAVON_BROWSER_SCREEN_WIDTH")  ?? "1920",
    BrowserTZ                 = Environment.GetEnvironmentVariable("ELAVON_BROWSER_TZ")           ?? "0",
    BrowserUserAgent          = Environment.GetEnvironmentVariable("ELAVON_BROWSER_USER_AGENT")    ?? "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36",
    ChallengeWindowSize       = Environment.GetEnvironmentVariable("ELAVON_CHALLENGE_WINDOW_SIZE") ?? "FullScreen",
    TransType                 = Environment.GetEnvironmentVariable("ELAVON_TRANS_TYPE")            ?? "GoodsAndServicePurchase",
    ThreeDSRequestorChallengeInd = Environment.GetEnvironmentVariable("ELAVON_3DS_REQUESTOR_CHALLENGE_IND") ?? "03",
    ThreeDSExemptionIndicatorType = Enum.TryParse<ThreeDSExemptionIndicatorType>(
        Environment.GetEnvironmentVariable("ELAVON_3DS_EXEMPTION_INDICATOR"), out var exemptionParsed)
        ? exemptionParsed
        : ThreeDSExemptionIndicatorType.LowValue
};

static async Task Handle3DsChallengeAsync(
    ElavonPaymentsClient client,
    string transactionId,
    string? acsUrl,
    string? cReq,
    JsonSerializerOptions jsonOptions)
{
    Console.WriteLine("\n--- 3D Secure v2 Challenge Required ---");

    if (string.IsNullOrWhiteSpace(acsUrl) || string.IsNullOrWhiteSpace(cReq))
    {
        Console.WriteLine("ACS URL or cReq was not returned. Cannot continue 3DS challenge flow.");
        return;
    }

    Console.WriteLine($"ACS URL:  {acsUrl}");
    Console.WriteLine($"cReq:     {cReq}");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Important: open the ACS URL with HTTP POST, not GET (GET returns 405).");
    Console.WriteLine("Quick test with curl:");
    Console.ResetColor();
    Console.WriteLine($"  curl -X POST --data-urlencode \"creq={cReq}\" \"{acsUrl}\"");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Sandbox ACS challenge password: challenge");
    Console.ResetColor();
    Console.WriteLine();
    Console.Write("Paste the cRes value here and press Enter: ");
    var cRes = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(cRes))
    {
        Console.WriteLine("No cRes provided — skipping 3DS completion.");
        return;
    }

    try
    {
        var complete = await client.ThreeDs
            .Complete3DsAsync(transactionId, new Complete3DsRequest { CRes = cRes })
            .ConfigureAwait(false);

        Console.WriteLine($"\n3DS completion — Status: {complete.Status}");
        Console.WriteLine($"StatusDetail:  {complete.StatusDetail}");
        Console.WriteLine($"TransactionId: {complete.TransactionId}");
        if (complete.ThreeDSecure is not null)
            Console.WriteLine($"3DSecure:      {complete.ThreeDSecure.Status}");
        PrintFullResponse("Full 3DS completion response JSON", complete, jsonOptions);
    }
    catch (ElavonApiException ex) when (!string.IsNullOrWhiteSpace(ex.RawResponse)
        && ex.RawResponse.Contains("\"code\":1029", StringComparison.OrdinalIgnoreCase))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("3DS completion failed: cReq already consumed (single-use). Start a fresh transaction.");
        Console.ResetColor();
        Console.WriteLine($"RawResponse: {ex.RawResponse}");
    }
}

static void PrintTransactionResult(PaymentResponse result, JsonSerializerOptions jsonOptions)
{
    Console.WriteLine($"Status:        {result.Status}");
    Console.WriteLine($"StatusCode:    {result.StatusCode}");
    Console.WriteLine($"StatusDetail:  {result.StatusDetail}");
    Console.WriteLine($"TransactionId: {result.TransactionId}");
    PrintFullResponse("Full transaction response JSON", result, jsonOptions);
}

static void PrintFullResponse<T>(string title, T response, JsonSerializerOptions jsonOptions)
{
    Console.WriteLine();
    Console.WriteLine(title + ":");
    Console.WriteLine(JsonSerializer.Serialize(response, jsonOptions));
    Console.WriteLine();
}

static void PrintHeading(string title)
{
    Console.WriteLine("======================");
    Console.WriteLine(title);
    Console.WriteLine("======================");
    Console.WriteLine();
}

static void PrintStep(int step, int total, string description)
    => Console.WriteLine($"Step {step}/{total}: {description}");

static void PrintApiException(ElavonApiException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    if (ex is ElavonAuthenticationException)
        Console.WriteLine($"\nAuthentication failed ({ex.HttpStatusCode}). Check credentials.");
    else
        Console.WriteLine($"\nAPI error ({ex.HttpStatusCode}) — ErrorCode: {ex.ErrorCode}");
    Console.ResetColor();
    Console.WriteLine(ex.RawResponse);
}

static string PromptWithDefault(string label, string defaultValue)
{
    Console.Write($"{label} [{defaultValue}]: ");
    var input = Console.ReadLine();
    return string.IsNullOrWhiteSpace(input) ? defaultValue : input.Trim();
}

static (string FirstName, string LastName) SplitName(string fullName)
{
    var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return parts.Length switch
    {
        0 => ("Sandbox", "Tester"),
        1 => (parts[0], "Tester"),
        _ => (parts[0], string.Join(" ", parts.Skip(1)))
    };
}

