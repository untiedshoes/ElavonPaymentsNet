namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Well-known values for <see cref="Responses.PaymentResponse.Status"/>.
/// Use these constants instead of magic strings when branching on transaction outcomes.
/// </summary>
public static class ElavonTransactionStatus
{
    /// <summary>Transaction was accepted and processed successfully.</summary>
    public const string Ok = "Ok";

    /// <summary>Transaction was not authorised by the card issuer.</summary>
    public const string NotAuthed = "NotAuthed";

    /// <summary>Transaction was rejected by the Elavon fraud rules.</summary>
    public const string Rejected = "Rejected";

    /// <summary>Request was malformed or contained invalid data.</summary>
    public const string Malformed = "Malformed";

    /// <summary>Request failed a validation rule (e.g. missing required field).</summary>
    public const string Invalid = "Invalid";

    /// <summary>An unexpected error occurred on the Elavon platform.</summary>
    public const string Error = "Error";

    /// <summary>Transaction has been registered but not yet processed (deferred flows).</summary>
    public const string Registered = "Registered";

    /// <summary>Transaction is pending further action.</summary>
    public const string Pending = "Pending";

    /// <summary>3D Secure authentication is required before the transaction can proceed.</summary>
    public const string ThreeDAuth = "3DAuth";
}
