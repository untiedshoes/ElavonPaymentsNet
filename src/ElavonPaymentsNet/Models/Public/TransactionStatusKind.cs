namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Strongly typed transaction status values exposed by the Elavon API.
/// </summary>
public enum TransactionStatusKind
{
    /// <summary>Status was absent or unrecognised.</summary>
    Unknown = 0,

    /// <summary>Transaction was accepted and processed successfully.</summary>
    Ok,

    /// <summary>Transaction was not authorised by the card issuer.</summary>
    NotAuthed,

    /// <summary>Transaction was rejected by the Elavon fraud rules.</summary>
    Rejected,

    /// <summary>Request was malformed or contained invalid data.</summary>
    Malformed,

    /// <summary>Request failed a validation rule (e.g. missing required field).</summary>
    Invalid,

    /// <summary>An unexpected error occurred on the Elavon platform.</summary>
    Error,

    /// <summary>Transaction has been registered but not yet processed.</summary>
    Registered,

    /// <summary>Transaction is pending further action.</summary>
    Pending,

    /// <summary>3D Secure authentication is required before completion.</summary>
    ThreeDAuth
}
