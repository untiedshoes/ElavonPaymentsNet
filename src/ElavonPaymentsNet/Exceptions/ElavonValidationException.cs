using System.Net;

namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the Elavon API returns a 400 Bad Request response,
/// indicating that one or more request fields failed validation.
/// Inspect <see cref="ValidationErrors"/> for field-level detail, or
/// <see cref="ElavonApiException.RawResponse"/> for the raw API body.
/// </summary>
public sealed class ElavonValidationException : ElavonApiException
{
    /// <summary>
    /// Per-field validation failures returned by the API, if available.
    /// </summary>
    public IReadOnlyList<ElavonValidationError>? ValidationErrors { get; }

    /// <summary>Initialises a new instance of <see cref="ElavonValidationException"/>.</summary>
    /// <param name="rawResponse">The raw response body, if available.</param>
    /// <param name="errorCode">A machine-readable error code from the API response, if available.</param>
    /// <param name="validationErrors">Per-field validation failures returned by the API, if available.</param>
    public ElavonValidationException(string? rawResponse, string? errorCode = null, IReadOnlyList<ElavonValidationError>? validationErrors = null)
        : base(System.Net.HttpStatusCode.BadRequest, rawResponse, errorCode)
    {
        ValidationErrors = validationErrors;
    }
}
