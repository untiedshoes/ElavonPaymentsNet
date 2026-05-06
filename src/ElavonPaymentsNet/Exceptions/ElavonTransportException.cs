namespace ElavonPaymentsNet.Exceptions;

/// <summary>
/// Thrown when the SDK cannot obtain an HTTP response from the gateway,
/// for example due to a network failure or timeout.
/// </summary>
public sealed class ElavonTransportException : ElavonApiException
{
    /// <summary>The transport-layer failure that caused this exception.</summary>
    public Exception TransportException { get; }

    /// <summary>
    /// Initialises a new instance of <see cref="ElavonTransportException"/>.
    /// </summary>
    /// <param name="message">A human-readable failure description.</param>
    /// <param name="transportException">The original transport exception.</param>
    public ElavonTransportException(string message, Exception transportException)
        : base(System.Net.HttpStatusCode.ServiceUnavailable, rawResponse: null, errorCode: "TransportError", innerException: transportException)
    {
        TransportException = transportException;
    }
}
