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
    apply3DSecure);

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
        Apply3DSecure = apply3DSecure
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
    string cardholderName,
    string magicCardholderName,
    Apply3DSecureOption? apply3DSecure)
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

        if (result.Status == "3DAuth" && !string.IsNullOrWhiteSpace(result.TransactionId))
        {
            await Handle3DsChallengeAsync(client, result.TransactionId);
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

static async Task Handle3DsChallengeAsync(ElavonPaymentsClient client, string transactionId)
{
    Console.WriteLine("\n--- 3D Secure Challenge Required ---");

    // Step 4a: Initialise the challenge to get the ACS URL and cReq.
    // The notification URL receives the cRes POST from the ACS; a placeholder is
    // sufficient for sandbox testing since we paste the cRes manually.
    var initResponse = await client.ThreeDs
        .Initialise3DsAsync(transactionId, new Initialise3DsRequest
        {
            NotificationUrl = "https://localhost/3ds-notify"
        })
        .ConfigureAwait(false);

    Console.WriteLine($"Step 4/4: 3DS initialised. Status: {initResponse.Status}");
    Console.WriteLine();
    Console.WriteLine("ACS URL (open this in a browser):");
    Console.WriteLine(initResponse.AcsUrl);
    Console.WriteLine();
    Console.WriteLine("cReq (POST this to the ACS URL as the 'creq' field):");
    Console.WriteLine(initResponse.CReq);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("In the sandbox ACS page, enter the password to simulate success.");
    Console.WriteLine("PI REST API sandbox password: challenge");
    Console.WriteLine("(Older hosted-form sandbox uses: password)");
    Console.ResetColor();
    Console.WriteLine();
    Console.Write("Paste the cRes value here and press Enter: ");
    var cRes = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(cRes))
    {
        Console.WriteLine("No cRes provided. Skipping 3DS completion.");
        return;
    }

    var completeResponse = await client.ThreeDs
        .Complete3DsAsync(transactionId, new Complete3DsRequest { Cres = cRes })
        .ConfigureAwait(false);

    Console.WriteLine();
    Console.WriteLine("3DS completion result:");
    Console.WriteLine($"Status:        {completeResponse.Status}");
    Console.WriteLine($"TransactionId: {completeResponse.TransactionId}");
}
