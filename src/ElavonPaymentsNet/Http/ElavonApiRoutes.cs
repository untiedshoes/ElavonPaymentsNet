namespace ElavonPaymentsNet.Http;

/// <summary>
/// Centralized relative API routes used by service implementations.
/// </summary>
internal static class ElavonApiRoutes
{
    /// <summary>Base route for transaction creation requests.</summary>
    internal const string Transactions = "/transactions";

    /// <summary>Route for card tokenization requests.</summary>
    internal const string Token = "/token";

    /// <summary>Route for creating merchant session keys.</summary>
    internal const string MerchantSessionKeys = "/merchant-session-keys";

    /// <summary>Route for validating merchant session keys.</summary>
    internal const string MerchantSessionKeyValidation = "/merchant-session-keys/validation";

    /// <summary>Route for Apple Pay merchant session creation.</summary>
    internal const string ApplePaySession = "/applepay/session";

    /// <summary>Base route for card identifier operations.</summary>
    internal const string CardIdentifiers = "/card-identifiers";

    /// <summary>Builds the capture route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for capture.</returns>
    internal static string TransactionCapture(string transactionId) => $"/transactions/{transactionId}/capture";

    /// <summary>Builds the refund route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for refund.</returns>
    internal static string TransactionRefund(string transactionId) => $"/transactions/{transactionId}/refund";

    /// <summary>Builds the void route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for void.</returns>
    internal static string TransactionVoid(string transactionId) => $"/transactions/{transactionId}/void";

    /// <summary>Builds the 3D Secure initialise route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for 3D Secure initialisation.</returns>
    internal static string Transaction3Ds(string transactionId) => $"/transactions/{transactionId}/3d-secure";

    /// <summary>Builds the 3D Secure completion route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for 3D Secure completion.</returns>
    internal static string Transaction3DsComplete(string transactionId) => $"/transactions/{transactionId}/3d-secure/complete";

    /// <summary>Builds the instructions route for a specific transaction.</summary>
    /// <param name="transactionId">The Elavon transaction identifier.</param>
    /// <returns>The relative API route for transaction instructions.</returns>
    internal static string TransactionInstructions(string transactionId) => $"/transactions/{transactionId}/instructions";

    /// <summary>Builds the security-code linking route for a card identifier.</summary>
    /// <param name="cardIdentifier">The card identifier token.</param>
    /// <returns>The relative API route for linking a security code.</returns>
    internal static string CardIdentifierSecurityCode(string cardIdentifier) => $"/card-identifiers/{cardIdentifier}/security-code";
}
