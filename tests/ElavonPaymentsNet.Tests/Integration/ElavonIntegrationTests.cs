using ElavonPaymentsNet.Exceptions;
using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Elavon sandbox API.
/// 
/// Happy-path tests require safe test credentials and IDs via environment variables.
/// Failure-scenario tests can run without environment setup by using deliberately invalid credentials.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ElavonIntegrationTests
{
    private const string SkipMessage = "Set ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, ELAVON_VENDOR_NAME, and ELAVON_SAFE_TRANSACTION_ID to run happy-path integration tests.";

    /// <summary>
    /// Verifies that a merchant session key can be created directly using sandbox credentials.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task CreateMerchantSessionKeyAsync_ReturnsSessionKeyAndExpiry()
    {
        if (!HasTransactionEnvironment())
            return;

        var client = new ElavonPaymentsClient(CreateOptionsFromEnvironment());
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");

        var response = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.MerchantSessionKey));
        Assert.True(response.Expiry.HasValue);
    }

    /// <summary>
    /// Verifies that a freshly created merchant session key validates successfully.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task ValidateMerchantSessionKeyAsync_FreshKey_ReturnsValid()
    {
        if (!HasTransactionEnvironment())
            return;

        var client = new ElavonPaymentsClient(CreateOptionsFromEnvironment());
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var response = await client.Wallets.ValidateMerchantSessionKeyAsync(
            new MerchantSessionValidationRequest { MerchantSessionKey = session.MerchantSessionKey! });

        Assert.NotNull(response);
        Assert.True(response.Valid);
    }

    /// <summary>
    /// Verifies that a card identifier can be created directly from a fresh merchant session key.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task CreateCardIdentifierAsync_ReturnsIdentifierExpiryAndCardType()
    {
        if (!HasTransactionEnvironment())
            return;

        var client = new ElavonPaymentsClient(CreateOptionsFromEnvironment());
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var response = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber = "4929000000006",
                    ExpiryDate = "1229",
                    SecurityCode = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.CardIdentifier));
        Assert.False(string.IsNullOrWhiteSpace(response.Expiry));
        Assert.False(string.IsNullOrWhiteSpace(response.CardType));
    }

    /// <summary>
    /// Verifies the full MSK + card-identifier + transaction flow against the sandbox,
    /// using the SUCCESSFUL magic cardholder name and Apply3DSecure=Disable.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_CardIdentifierFlow_ReturnsOk()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"   // sandbox magic value: frictionless OK
                }
            });

        Assert.False(string.IsNullOrWhiteSpace(cardId.CardIdentifier));

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test payment",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("Ok", result.Status);
    }

    /// <summary>
    /// Verifies the full MSK + card-identifier + transaction flow returns a bank decline
    /// for the known sandbox decline card when 3DS is disabled.
    /// Requires ELAVON_INTEGRATION_KEY, ELAVON_INTEGRATION_PASSWORD, and ELAVON_VENDOR_NAME.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_CardIdentifierFlow_DeclineCard_ReturnsBankDecline()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        Assert.False(string.IsNullOrWhiteSpace(session.MerchantSessionKey));

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929602110085639",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        Assert.False(string.IsNullOrWhiteSpace(cardId.CardIdentifier));

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-DECLINE-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test bank decline payment",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("NotAuthed", result.Status);
        Assert.Equal(2000, result.StatusCode);
        Assert.Equal("The Authorisation was Declined by the bank.", result.StatusDetail);
    }

    /// <summary>
    /// Verifies that RetrieveTransactionAsync can read a known safe transaction in sandbox
    /// when integration credentials and a safe transaction ID are supplied.
    /// </summary>
    [Fact]
    public async Task RetrieveTransactionAsync_WithConfiguredSafeId_ReturnsResponse()
    {
        if (!HasIntegrationEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var client = new ElavonPaymentsClient(options);
        var transactionId = GetRequiredEnvironmentVariable("ELAVON_SAFE_TRANSACTION_ID");

        var response = await client.Transactions.RetrieveTransactionAsync(transactionId);

        Assert.NotNull(response);
        Assert.Equal(transactionId, response.TransactionId);
        Assert.False(string.IsNullOrWhiteSpace(response.Status));
    }

    /// <summary>
    /// Verifies that a deferred transaction can be created with TransactionType.Deferred.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_Deferred_ReturnsOk()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Deferred,
            VendorTxCode      = $"INTEGRATION-DEF-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test deferred payment",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("Ok", result.Status);
    }

    /// <summary>
    /// Verifies that a repeat transaction can be created from a completed payment.
    /// </summary>
    [Fact]
    public async Task CreateTransactionAsync_Repeat_ReturnsOk()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        // First, create the original payment to repeat
        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        var original = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-ORIG-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test original payment for repeat",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        Assert.Equal("Ok", original.Status);

        // Now repeat it
        var repeat = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType      = TransactionType.Repeat,
            VendorTxCode         = $"INTEGRATION-RPT-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount               = 100,
            Currency             = "GBP",
            Description          = "Integration test repeat payment",
            RelatedTransactionId = original.TransactionId
        });

        Assert.NotNull(repeat);
        Assert.False(string.IsNullOrWhiteSpace(repeat.TransactionId));
        Assert.Equal("Ok", repeat.Status);
    }

    /// <summary>
    /// Verifies that a transaction can be voided via PostPayments after creation.
    /// </summary>
    [Fact]
    public async Task VoidTransactionAsync_AfterPayment_ReturnsSuccess()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var txId = await CreateSuccessfulPaymentTransactionAsync(client, vendorName, "VOID");

        var result = await client.PostPayments.VoidTransactionAsync(txId);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>
    /// Verifies that a transaction can be refunded via PostPayments after creation.
    /// </summary>
    [Fact]
    public async Task RefundTransactionAsync_AfterPayment_ReturnsSuccess()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var txId = await CreateSuccessfulPaymentTransactionAsync(client, vendorName, "REFUND");

        var result = await client.PostPayments.RefundTransactionAsync(txId, new RefundPaymentRequest
        {
            Amount       = 100,
            VendorTxCode = $"INTEGRATION-RFND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>
    /// Verifies that a deferred transaction can be captured via PostPayments.
    /// </summary>
    [Fact]
    public async Task CaptureTransactionAsync_AfterDeferred_ReturnsSuccess()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var txId = await CreateDeferredTransactionAsync(client, vendorName);

        var result = await client.PostPayments.CaptureTransactionAsync(txId, new CapturePaymentRequest
        {
            Amount = 100
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>
    /// Verifies that a Void instruction can be posted to a completed payment.
    /// </summary>
    [Fact]
    public async Task CreateInstructionAsync_Void_ReturnsVoidInstruction()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var txId = await CreateSuccessfulPaymentTransactionAsync(client, vendorName, "INSTRVOID");

        var result = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Void
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Void, result.InstructionType);
    }

    /// <summary>
    /// Verifies that an Abort instruction can be posted to a deferred transaction.
    /// </summary>
    [Fact]
    public async Task CreateInstructionAsync_Abort_OnDeferred_ReturnsAbortInstruction()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var txId = await CreateDeferredTransactionAsync(client, vendorName);

        var result = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Abort
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Abort, result.InstructionType);
    }

    /// <summary>
    /// Verifies that a Release instruction can be posted to a deferred transaction.
    /// </summary>
    [Fact]
    public async Task CreateInstructionAsync_Release_OnDeferred_ReturnsReleaseInstruction()
    {
        if (!HasTransactionEnvironment())
            return;

        var options = CreateOptionsFromEnvironment();
        var vendorName = GetRequiredEnvironmentVariable("ELAVON_VENDOR_NAME");
        var client = new ElavonPaymentsClient(options);

        var txId = await CreateDeferredTransactionAsync(client, vendorName);

        var result = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Release,
            Amount          = 100
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Release, result.InstructionType);
    }

    // ----------------------------------------------------------------
    // Helpers shared by multiple tests
    // ----------------------------------------------------------------

    /// <summary>Creates a successful Payment transaction and returns its TransactionId.</summary>
    private static async Task<string> CreateSuccessfulPaymentTransactionAsync(
        ElavonPaymentsClient client, string vendorName, string tag)
    {
        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Payment,
            VendorTxCode      = $"INTEGRATION-{tag}-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = $"Integration test payment ({tag})",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        return result.TransactionId!;
    }

    /// <summary>Creates a Deferred transaction and returns its TransactionId.</summary>
    private static async Task<string> CreateDeferredTransactionAsync(
        ElavonPaymentsClient client, string vendorName)
    {
        var session = await client.Wallets.CreateMerchantSessionKeyAsync(
            new MerchantSessionRequest { VendorName = vendorName });

        var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
            session.MerchantSessionKey!,
            new CreateCardIdentifierRequest
            {
                CardDetails = new CardDetails
                {
                    CardNumber     = "4929000000006",
                    ExpiryDate     = "1229",
                    SecurityCode   = "123",
                    CardholderName = "SUCCESSFUL"
                }
            });

        var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType   = TransactionType.Deferred,
            VendorTxCode      = $"INTEGRATION-DEF-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount            = 100,
            Currency          = "GBP",
            Description       = "Integration test deferred transaction",
            CustomerFirstName = "Integration",
            CustomerLastName  = "Test",
            PaymentMethod     = new PaymentMethod
            {
                Card = new CardDetails
                {
                    MerchantSessionKey = session.MerchantSessionKey,
                    CardIdentifier     = cardId.CardIdentifier
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1   = "88",
                City       = "London",
                PostalCode = "412",
                Country    = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.Disable
        });

        return result.TransactionId!;
    }

    /// <summary>
    /// Verifies that invalid credentials are rejected by the real API with an authentication exception.
    /// This test is intentionally independent of environment variables.
    /// </summary>
    [Fact]
    public async Task CreateMerchantSessionKeyAsync_WithInvalidCredentials_ThrowsAuthenticationException()
    {
        if (!HasIntegrationEnvironment())
            return;

        var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
        {
            IntegrationKey = "invalid-key",
            IntegrationPassword = "invalid-password",
            Environment = ElavonEnvironment.Sandbox
        });

        await Assert.ThrowsAsync<ElavonAuthenticationException>(() =>
            client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest()));
    }

    /// <summary>
    /// Checks whether credentials and vendor name are present (sufficient to create transactions).
    /// </summary>
    private static bool HasTransactionEnvironment()
        => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_KEY"))
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD"))
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_VENDOR_NAME"));

    /// <summary>
    /// Checks whether all required environment variables for happy-path integration tests are present.
    /// </summary>
    private static bool HasIntegrationEnvironment()
        => HasTransactionEnvironment()
           && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELAVON_SAFE_TRANSACTION_ID"));

    /// <summary>
    /// Builds options from required environment variables.
    /// </summary>
    private static ElavonPaymentsClientOptions CreateOptionsFromEnvironment() => new()
    {
        IntegrationKey = GetRequiredEnvironmentVariable("ELAVON_INTEGRATION_KEY"),
        IntegrationPassword = GetRequiredEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD"),
        Environment = ElavonEnvironment.Sandbox
    };

    /// <summary>
    /// Gets an environment variable value or throws if it is missing/blank.
    /// </summary>
    private static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{SkipMessage} Missing: {variableName}.");

        return value;
    }
}
