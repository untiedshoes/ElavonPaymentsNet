namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Represents a single field-level validation failure returned by the Elavon API on a 400 or 422 response.
/// Exposed via <see cref="ElavonValidationException.ValidationErrors"/>.
/// </summary>
public sealed class ElavonValidationError
{
    /// <summary>The name of the field that failed validation.</summary>
    public string? Property { get; init; }

    /// <summary>A human-readable description of the validation failure, as returned by the API.</summary>
    public string? Description { get; init; }

    /// <summary>A numeric error code identifying the specific validation rule that failed.</summary>
    public int? Code { get; init; }

    /// <summary>A consumer-facing message describing the validation failure (legacy field).</summary>
    public string? ClientMessage { get; init; }

    /// <summary>An internal message describing the validation failure (legacy field).</summary>
    public string? Message { get; init; }
}
