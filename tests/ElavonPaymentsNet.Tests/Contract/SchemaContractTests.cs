using ElavonPaymentsNet.Models.Internal.Dto;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElavonPaymentsNet.Tests.Contract;

/// <summary>
/// Contract tests that verify JSON serialisation compatibility between the SDK's request/response
/// models and the wire-format fixtures in docs/schema/**. Each test proves that a given SDK type
/// can correctly round-trip through the JSON layer without silently dropping properties.
/// </summary>
[Trait("Category", "Contract")]
public sealed class SchemaContractTests
{
    // -------------------------------------------------------------------------
    // JSON options — must match ElavonApiClient.JsonOptions exactly
    // -------------------------------------------------------------------------

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    // -------------------------------------------------------------------------
    // Response round-trip contract
    //
    // Each fixture is deserialised into its SDK response type, then re-serialised.
    // Every top-level key present in the fixture must survive the round-trip.
    // A missing key means the property name has drifted and the SDK cannot read
    // real API responses for that field.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Theory data: (schemaRelativePath, responseType).
    /// </summary>
    public static TheoryData<string, Type> ResponseSchemas =>
        new()
        {
            { "transactions/create-payment.response.json",          typeof(PaymentResponse) },
            { "transactions/authorise-payment.response.json",       typeof(PaymentResponse) },
            { "transactions/deferred-payment.response.json",        typeof(PaymentResponse) },
            { "transactions/repeat-payment.response.json",          typeof(PaymentResponse) },
            { "postpayment/capture.response.json",                  typeof(PostPaymentResponse) },
            { "postpayment/refund.response.json",                   typeof(PostPaymentResponse) },
            { "postpayment/void.response.json",                     typeof(PostPaymentResponse) },
            { "threeds/complete.response.json",                     typeof(Complete3DsResponse) },
            { "tokens/create-token.response.json",                  typeof(CreateTokenResponse) },
            { "tokens/pay-with-token.response.json",                typeof(PaymentResponse) },
            { "wallets/merchant-session.response.json",             typeof(MerchantSessionResponse) },
            { "wallets/merchant-session-validation.response.json",  typeof(MerchantSessionValidationResponse) },
            { "wallets/applepay-session.response.json",             typeof(ApplePaySessionResponse) },
        };

    /// <summary>
    /// Deserialises a schema fixture into the SDK response type, re-serialises it,
    /// and asserts that every top-level key from the fixture survives the round-trip.
    /// </summary>
    [Theory]
    [MemberData(nameof(ResponseSchemas))]
    public void ResponseSchema_AllSchemaKeysSurviveRoundTrip(string schemaPath, Type responseType)
    {
        var json = LoadSchema(schemaPath);
        using var fixtureDoc = JsonDocument.Parse(json);
        var expectedKeys = fixtureDoc.RootElement
            .EnumerateObject()
            .Select(property => property.Name)
            .ToArray();

        var result = JsonSerializer.Deserialize(json, responseType, JsonOptions);
        Assert.NotNull(result);

        var reJson = JsonSerializer.Serialize(result, responseType, JsonOptions);
        using var doc = JsonDocument.Parse(reJson);

        foreach (var key in expectedKeys)
        {
            Assert.True(
                doc.RootElement.TryGetProperty(key, out _),
                $"Expected key '{key}' is missing after round-trip. Schema: {schemaPath}. Re-serialised JSON: {reJson}");
        }
    }

    // -------------------------------------------------------------------------
    // Request serialisation contract
    //
    // A reference SDK request instance is serialised to JSON. Every key listed
    // in the expectedKeys array must appear in the output. A missing key means
    // a property name has drifted and the SDK would send the wrong wire format.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Theory data: (schemaNickname, reference request instance, keys that must appear in serialised output).
    /// </summary>
    public static TheoryData<string, object, string[]> RequestSchemas =>
        new()
        {
            {
                "transactions/create-payment",
                new CreateTransactionRequestDto
                {
                    TransactionType = "Payment",
                    VendorTxCode    = "ORDER-1",
                    Amount          = 100,
                    Currency        = "GBP",
                    PaymentMethod   = new() { Card = new() { CardNumber = "4929000000006", ExpiryDate = "1229" } }
                },
                new[] { "transactionType", "vendorTxCode", "amount", "currency", "paymentMethod" }
            },
            {
                "transactions/authorise-payment",
                new CreateTransactionRequestDto
                {
                    TransactionType = "Authorise",
                    VendorTxCode    = "ORDER-2",
                    Amount          = 100,
                    Currency        = "GBP",
                    PaymentMethod   = new() { Card = new() { CardNumber = "4929000000006", ExpiryDate = "1229" } }
                },
                new[] { "transactionType", "vendorTxCode", "amount", "currency", "paymentMethod" }
            },
            {
                "transactions/deferred-payment",
                new CreateTransactionRequestDto
                {
                    TransactionType = "Deferred",
                    VendorTxCode    = "ORDER-3",
                    Amount          = 100,
                    Currency        = "GBP",
                    PaymentMethod   = new() { Card = new() { CardNumber = "4929000000006", ExpiryDate = "1229" } }
                },
                new[] { "transactionType", "vendorTxCode", "amount", "currency", "paymentMethod" }
            },
            {
                "transactions/repeat-payment",
                new CreateTransactionRequestDto
                {
                    TransactionType         = "Repeat",
                    VendorTxCode            = "ORDER-4",
                    Amount                  = 100,
                    Currency                = "GBP",
                    Description             = "Repeat payment",
                    ReferenceTransactionId  = "tx-orig"
                },
                new[] { "transactionType", "vendorTxCode", "amount", "currency", "description", "referenceTransactionId" }
            },
            {
                "postpayment/capture",
                new CapturePaymentRequest { Amount = 100 },
                new[] { "amount" }
            },
            {
                "postpayment/refund",
                new RefundPaymentRequest { Amount = 50, VendorTxCode = "REFUND-1", Description = "Refund payment" },
                new[] { "amount", "vendorTxCode", "description" }
            },
            {
                "threeds/complete",
                new Complete3DsRequest { CRes = "sample-cres" },
                new[] { "cRes" }
            },
            {
                "tokens/create-token",
                new CreateTokenRequest { Card = new() { CardNumber = "4929000000006", ExpiryDate = "1229" } },
                new[] { "card" }
            },
            {
                "tokens/pay-with-token",
                new PayWithTokenRequestDto
                {
                    TransactionType = "Payment",
                    VendorTxCode    = "ORDER-5",
                    Amount          = 100,
                    Currency        = "GBP",
                    PaymentMethod   = new() { Token = "stored-token" }
                },
                new[] { "transactionType", "vendorTxCode", "amount", "currency", "paymentMethod" }
            },
            {
                "wallets/merchant-session",
                new MerchantSessionRequest { MerchantSessionKey = "msk-123" },
                new[] { "merchantSessionKey" }
            },
            {
                "wallets/merchant-session-validation",
                new MerchantSessionValidationRequest { MerchantSessionKey = "msk-123" },
                new[] { "merchantSessionKey" }
            },
            {
                "wallets/applepay-session",
                new ApplePaySessionRequest { ValidationUrl = "https://apple.example.com/validate", DomainName = "example.com" },
                new[] { "validationUrl", "domainName" }
            },
        };

    /// <summary>
    /// Serialises a reference SDK request instance and asserts that all expected
    /// wire-format keys are present in the serialised JSON output.
    /// </summary>
    [Theory]
    [MemberData(nameof(RequestSchemas))]
    public void RequestSchema_AllExpectedKeysSerialise(string schemaNickname, object requestInstance, string[] expectedKeys)
    {
        var json = JsonSerializer.Serialize(requestInstance, requestInstance.GetType(), JsonOptions);
        using var doc = JsonDocument.Parse(json);

        foreach (var key in expectedKeys)
        {
            Assert.True(
                doc.RootElement.TryGetProperty(key, out _),
                $"Expected key '{key}' is missing from serialised output. Schema: {schemaNickname}. JSON: {json}");
        }
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static string LoadSchema(string relativePath)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Schema",
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllText(path);
    }
}
