using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Shared helpers for integration tests.
/// Each method performs a real API call against the Elavon sandbox and returns
/// <see langword="null"/> on any failure. Tests that depend on a helper result
/// should return early (skip) when the result is null — a null result means a
/// prerequisite operation failed, and a separate test will surface that failure.
/// </summary>
internal static class SandboxHelpers
{
    // ----------------------------------------------------------------
    // Merchant session keys
    // ----------------------------------------------------------------

    /// <summary>Creates a merchant session key. Returns <see langword="null"/> on failure.</summary>
    internal static async Task<MerchantSessionResponse?> GetMerchantSessionKeyAsync(
        ElavonPaymentsClientOptions? options = null,
        string? vendorName = null)
    {
        try
        {
            var client = new ElavonPaymentsClient(options ?? SandboxCredentials.Basic);
            return await client.Wallets.CreateMerchantSessionKeyAsync(
                new MerchantSessionRequest { VendorName = vendorName ?? SandboxCredentials.BasicVendorName });
        }
        catch
        {
            return null;
        }
    }

    // ----------------------------------------------------------------
    // Card identifiers — named by sandbox magic cardholder value
    // ----------------------------------------------------------------

    /// <summary>
    /// Helpers for obtaining card identifiers using sandbox magic cardholder names.
    /// Magic names control the 3DS and authorisation simulation outcome.
    /// </summary>
    internal static class CardIdentifiers
    {
        // Standard success — frictionless OK, no 3DS challenge.
        internal static Task<CreateCardIdentifierResponse?> GetSuccessfulAsync(
            string merchantSessionKey, ElavonPaymentsClientOptions? options = null)
            => GetAsync(merchantSessionKey, "SUCCESSFUL", options);

        // Bank decline simulation.
        internal static Task<CreateCardIdentifierResponse?> GetDeclinedAsync(
            string merchantSessionKey, ElavonPaymentsClientOptions? options = null)
            => GetAsync(merchantSessionKey, "NOTAUTH", options);

        // 3DS challenge flow (requires sandboxEC profile).
        internal static Task<CreateCardIdentifierResponse?> GetChallengeAsync(
            string merchantSessionKey, ElavonPaymentsClientOptions? options = null)
            => GetAsync(merchantSessionKey, "CHALLENGE", options);

        // 3DS proof of attempt (frictionless, status U).
        internal static Task<CreateCardIdentifierResponse?> GetProofAttemptAsync(
            string merchantSessionKey, ElavonPaymentsClientOptions? options = null)
            => GetAsync(merchantSessionKey, "PROOFATTEMPT", options);

        // Technical difficulties simulation.
        internal static Task<CreateCardIdentifierResponse?> GetTechDifficultiesAsync(
            string merchantSessionKey, ElavonPaymentsClientOptions? options = null)
            => GetAsync(merchantSessionKey, "TECHDIFFICULTIES", options);

        private static async Task<CreateCardIdentifierResponse?> GetAsync(
            string merchantSessionKey,
            string magicName,
            ElavonPaymentsClientOptions? options)
        {
            try
            {
                var client = new ElavonPaymentsClient(options ?? SandboxCredentials.Basic);
                return await client.CardIdentifiers.CreateCardIdentifierAsync(
                    merchantSessionKey,
                    new CreateCardIdentifierRequest
                    {
                        CardDetails = new CardDetails
                        {
                            CardNumber     = "4929000000006",
                            ExpiryDate     = "1229",
                            SecurityCode   = "123",
                            CardholderName = magicName
                        }
                    });
            }
            catch
            {
                return null;
            }
        }
    }

    // ----------------------------------------------------------------
    // Full transaction setups — return the TransactionId or null
    // ----------------------------------------------------------------

    /// <summary>
    /// Creates a successful Payment transaction and returns its <c>TransactionId</c>,
    /// or <see langword="null"/> if any step fails.
    /// </summary>
    internal static async Task<string?> GetSuccessfulTransactionIdAsync(
        ElavonPaymentsClientOptions? options = null,
        string? vendorName = null,
        string tag = "HELPER")
    {
        try
        {
            options    ??= SandboxCredentials.Basic;
            vendorName ??= SandboxCredentials.BasicVendorName;

            var client = new ElavonPaymentsClient(options);

            var session = await client.Wallets.CreateMerchantSessionKeyAsync(
                new MerchantSessionRequest { VendorName = vendorName });

            if (string.IsNullOrWhiteSpace(session.MerchantSessionKey))
                return null;

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

            if (string.IsNullOrWhiteSpace(cardId.CardIdentifier))
                return null;

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

            return string.IsNullOrWhiteSpace(result.TransactionId) ? null : result.TransactionId;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a Deferred transaction and returns its <c>TransactionId</c>,
    /// or <see langword="null"/> if any step fails.
    /// </summary>
    internal static async Task<string?> GetDeferredTransactionIdAsync(
        ElavonPaymentsClientOptions? options = null,
        string? vendorName = null)
    {
        try
        {
            options    ??= SandboxCredentials.Basic;
            vendorName ??= SandboxCredentials.BasicVendorName;

            var client = new ElavonPaymentsClient(options);

            var session = await client.Wallets.CreateMerchantSessionKeyAsync(
                new MerchantSessionRequest { VendorName = vendorName });

            if (string.IsNullOrWhiteSpace(session.MerchantSessionKey))
                return null;

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

            if (string.IsNullOrWhiteSpace(cardId.CardIdentifier))
                return null;

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

            return string.IsNullOrWhiteSpace(result.TransactionId) ? null : result.TransactionId;
        }
        catch
        {
            return null;
        }
    }
}
