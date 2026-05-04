using System;

namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Well-known values for <see cref="Responses.PaymentResponse.Status"/>.
/// Use these constants instead of magic strings when branching on transaction outcomes.
/// </summary>
public static class TransactionStatus
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

    /// <summary>
    /// Parses a raw status string into a strongly typed <see cref="TransactionStatusKind"/>.
    /// Unknown or newly introduced statuses are mapped to <see cref="TransactionStatusKind.Unknown"/>.
    /// </summary>
    public static TransactionStatusKind ParseKind(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return TransactionStatusKind.Unknown;

        return status switch
        {
            Ok => TransactionStatusKind.Ok,
            NotAuthed => TransactionStatusKind.NotAuthed,
            Rejected => TransactionStatusKind.Rejected,
            Malformed => TransactionStatusKind.Malformed,
            Invalid => TransactionStatusKind.Invalid,
            Error => TransactionStatusKind.Error,
            Registered => TransactionStatusKind.Registered,
            Pending => TransactionStatusKind.Pending,
            ThreeDAuth => TransactionStatusKind.ThreeDAuth,
            _ => ParseKindCaseInsensitive(status)
        };
    }

    /// <summary>
    /// Returns <see langword="true"/> when the supplied raw status matches the requested kind.
    /// </summary>
    public static bool Is(string? status, TransactionStatusKind kind) => ParseKind(status) == kind;

    private static TransactionStatusKind ParseKindCaseInsensitive(string status)
    {
        if (status.Equals(Ok, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Ok;
        if (status.Equals(NotAuthed, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.NotAuthed;
        if (status.Equals(Rejected, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Rejected;
        if (status.Equals(Malformed, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Malformed;
        if (status.Equals(Invalid, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Invalid;
        if (status.Equals(Error, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Error;
        if (status.Equals(Registered, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Registered;
        if (status.Equals(Pending, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.Pending;
        if (status.Equals(ThreeDAuth, StringComparison.OrdinalIgnoreCase))
            return TransactionStatusKind.ThreeDAuth;

        return TransactionStatusKind.Unknown;
    }
}
