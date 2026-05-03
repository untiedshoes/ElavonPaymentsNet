namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Specifies the type of transaction to create.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Most common transaction type for purchasing goods/services.
    /// Equivalent to authorisation and capture in a single step.
    /// </summary>
    Payment,

    /// <summary>
    /// Places a hold on funds and captures later via a release/capture operation.
    /// </summary>
    Authorise,

    /// <summary>
    /// Places a shadow on customer funds without immediate capture.
    /// Use <c>client.PostPayments.CaptureTransactionAsync</c> when ready to ship.
    /// </summary>
    Deferred,

    /// <summary>
    /// Verifies cardholder details without reserving funds.
    /// Use an authorise/payment flow later to take funds.
    /// </summary>
    Authenticate,

    /// <summary>
    /// Repeats a payment using customer/card details captured by a previous transaction.
    /// Requires <see cref="ElavonPaymentsNet.Models.Public.Requests.CreateTransactionRequest.RelatedTransactionId"/>.
    /// </summary>
    Repeat,

    /// <summary>
    /// Credits funds back to the customer for a previously captured transaction.
    /// Supports partial or full refunds up to the original amount.
    /// </summary>
    Refund
}
