namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Specifies the type of transaction to create.
/// </summary>
public enum TransactionType
{
    /// <summary>Creates and immediately processes a payment.</summary>
    Payment,

    /// <summary>Reserves (authorises) funds without capturing. Use <c>client.PostPayments.CaptureTransactionAsync</c> to capture later.</summary>
    Authorise,

    /// <summary>Defers a payment for later capture.</summary>
    Deferred,

    /// <summary>Repeats a previously successful payment. Requires <see cref="CreateTransactionRequest.RelatedTransactionId"/>.</summary>
    Repeat
}
