namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Represents a single field-level validation failure returned by the Elavon API on a 400 response.
/// Exposed via <see cref="ElavonValidationException.ValidationErrors"/>.
/// </summary>
public sealed class ElavonValidationError
{
    /// <summary>The name of the field that failed validation.</summary>
    public string? Property { get; init; }

    /// <summary>A consumer-facing message describing the validation failure.</summary>
    public string? ClientMessage { get; init; }

    /// <summary>An internal message describing the validation failure.</summary>
    public string? Message { get; init; }
}
